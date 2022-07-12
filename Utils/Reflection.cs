using AeroCore.ReflectedValue;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AeroCore.Utils
{
    [ModInit]
    public static class Reflection
    {
        private static readonly MethodInfo addItemMethod = typeof(Reflection).MethodNamed("AddItem");
        private static readonly MethodInfo addItemsMethod = typeof(Reflection).MethodNamed("AddItems");
        private static readonly Dictionary<KeyValuePair<Type, Type>, MethodInfo> addItemCache = new();
        private static readonly Dictionary<KeyValuePair<Type, Type>, MethodInfo> addItemsCache = new();
        internal static Multiplayer mp = null;
        public static Multiplayer Multiplayer => mp;
        public static readonly Dictionary<Type, JsonConverter> KnownConverters = new();

        internal static void Init()
        {
            var ns = "StardewModdingAPI.Framework.Serialization.";
            KnownConverters.Add(typeof(Vector2), (JsonConverter)TypeNamed(ns + "Vector2Converter").New());
            KnownConverters.Add(typeof(Point), (JsonConverter)TypeNamed(ns + "PointConverter").New());
            KnownConverters.Add(typeof(Rectangle), (JsonConverter)TypeNamed(ns + "RectangleConverter").New());
            KnownConverters.Add(typeof(Keybind), (JsonConverter)TypeNamed(ns + "KeybindConverter").New());
            KnownConverters.Add(typeof(Color), new Framework.ColorConverter());
        }

        public static Type TypeNamed(string name) => AccessTools.TypeByName(name);
        public static MethodInfo MethodNamed(this Type type, string name) => AccessTools.Method(type, name);
        public static MethodInfo MethodNamed(this Type type, string name, Type[] args) => AccessTools.Method(type, name, args);
        public static MethodInfo PropertyGetter(this Type type, string name) => AccessTools.PropertyGetter(type, name);
        public static MethodInfo PropertySetter(this Type type, string name) => AccessTools.PropertySetter(type, name);
        public static FieldInfo FieldNamed(this Type type, string name) => AccessTools.Field(type, name);
        public static object New(this Type type, params object[] args)
            => Activator.CreateInstance(type, args);
        public static IValue<T> ValueNamed<T>(this Type type, string name)
            => (AccessTools.Property(type, name) is not null) ? new InstanceProperty<T>(type, name) : 
                (AccessTools.Field(type, name) is not null) ? new InstanceField<T>(type, name) : 
            throw new NullReferenceException($"Type '{type.FullName}' does not have a property or field named '{name}'.");
        public static IStaticValue<T> StaticValueNamed<T>(this Type type, string name)
        {
            FieldInfo field;
            var prop = AccessTools.PropertyGetter(type, name);
            return (prop is not null && prop.IsStatic) ? new StaticProperty<T>(type, name) :
                ((field = AccessTools.Field(type, name)) is not null && field.IsStatic) ? new StaticField<T>(type, name) :
            throw new NullReferenceException($"Type '{type.FullName}' does not have a static property or field named '{name}'.");
        }
        public static ValueChain ValueRef(this Type type, string name)
            => new(type, name);
        public static ValueChain MethodRef(this Type type, string name, params object[] args)
        {
            var method = type.MethodNamed(name);
            if (method is null)
                throw new NullReferenceException($"Type '{type.FullName}' does not contain method '{name}'.");
            else
                return new(type, method, args);
        }
        public static CodeInstruction WithLabels(this CodeInstruction code, params Label[] labels)
        {
            foreach (Label label in labels)
                code.labels.Add(label);

            return code;
        }
        public static bool AddDictionaryEntry(IModContentHelper helper, IAssetData asset, object key, string path)
        {
            Type T = asset.DataType;

            if (!T.IsGenericType || T.GetGenericTypeDefinition() != typeof(Dictionary<,>))
                return false;

            Type[] types = T.GetGenericArguments();
            if(key.GetType().IsAssignableTo(types[0]))
                return false;

            KeyValuePair<Type, Type> typ = new(types[0], types[1]);
            if (!addItemCache.TryGetValue(typ, out var method))
                addItemCache.Add(typ, method = addItemMethod.MakeGenericMethod(types));
            method.Invoke(null, new object[] {helper, asset, key, path});
            return true;
        }
        public static void AddItem<k, v>(IModContentHelper helper, IAssetData asset, k key, string path)
        {
            var model = asset.AsDictionary<k, v>().Data;
            var entry = helper.Load<v>(path);
            model.Add(key, entry);
        }
        public static bool AddDictionaryEntries(IModContentHelper helper, IAssetData asset, string path)
        {
            Type T = asset.DataType;

            if (!T.IsGenericType || T.GetGenericTypeDefinition() != typeof(Dictionary<,>))
                return false;

            Type[] types = T.GetGenericArguments();

            KeyValuePair<Type, Type> typ = new(types[0], types[1]);
            if (!addItemsCache.TryGetValue(typ, out var method))
                addItemsCache.Add(typ, method = addItemsMethod.MakeGenericMethod(types));
            method.Invoke(null, new object[] { helper, asset, path });
            return true;
        }
        public static void AddItems<k, v>(IModContentHelper helper, IAssetData asset, string path)
        {
            var model = asset.AsDictionary<k, v>().Data;
            foreach ((k key, v val) in helper.Load<Dictionary<k, v>>(path))
                model[key] = val;
        }
        public static T MapTo<T>(this IDictionary<string, JToken> dict, T obj)
        {
            Type type = obj.GetType();
            foreach ((var k, var v) in dict)
            {
                var p = type.GetProperty(k);
                if (p is null)
                    continue;
                p.SetValue(obj, v.ToObject(p.PropertyType));
            }
            return obj;
        }
        public static T MapTo<T>(T obj, object args)
        {
            var type = obj.GetType();
            var allflag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var field in args.GetType().GetFields(allflag))
            {
                var f = type.FieldNamed(field.Name);
                if (f is not null && field.FieldType.IsAssignableTo(f.FieldType) && !field.IsInitOnly && !field.IsLiteral)
                    f.SetValue(obj, field.GetValue(args));
            }
            foreach (var prop in args.GetType().GetProperties(allflag))
            {
                var p = AccessTools.Property(type, prop.Name);
                if (p is not null && prop.PropertyType.IsAssignableTo(p.PropertyType) && prop.CanWrite)
                    p.SetValue(obj, prop.GetValue(args));
            }
            return obj;
        }
        public static T ValueIgnoreCase<T>(this JObject obj, string fieldName)
        {
            JToken token = obj.GetValue(fieldName, StringComparison.OrdinalIgnoreCase);
            return token != null
                ? token.Value<T>()
                : default;
        }
    }
}
