using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace AeroCore.Utils
{
    public static class Reflection
    {
        private static MethodInfo addItemMethod = typeof(Reflection).MethodNamed("AddItem");
        private static MethodInfo addItemsMethod = typeof(Reflection).MethodNamed("AddItems");
        internal static Multiplayer mp = null;
        public static Multiplayer Multiplayer => mp;

        public static MethodInfo MethodNamed(this Type type, string name) => AccessTools.Method(type, name);
        public static MethodInfo MethodNamed(this Type type, string name, Type[] args) => AccessTools.Method(type, name, args);
        public static MethodInfo PropertyGetter(this Type type, string name) => AccessTools.PropertyGetter(type, name);
        public static MethodInfo PropertySetter(this Type type, string name) => AccessTools.PropertySetter(type, name);
        public static FieldInfo FieldNamed(this Type type, string name) => AccessTools.Field(type, name);
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

            addItemMethod.MakeGenericMethod(types).Invoke(null, new object[] {helper, asset, key, path});
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
            addItemsMethod.MakeGenericMethod(types).Invoke(null, new object[] { helper, asset, path });
            return true;
        }
        public static void AddItems<k, v>(IModContentHelper helper, IAssetData asset, string path)
        {
            var model = asset.AsDictionary<k, v>().Data;
            foreach ((k key, v val) in helper.Load<Dictionary<k, v>>(path))
                model[key] = val;
        }
    }
}
