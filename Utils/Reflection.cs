using HarmonyLib;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AeroCore.Utils
{
    public static class Reflection
    {
        private static MethodInfo addItemMethod = typeof(Reflection).MethodNamed("AddItem");
        public static MethodInfo MethodNamed(this Type type, string name) => AccessTools.Method(type, name);
        public static MethodInfo MethodNamed(this Type type, string name, Type[] args) => AccessTools.Method(type, name, args);
        public static FieldInfo FieldNamed(this Type type, string name) => AccessTools.Field(type, name);
        public static CodeInstruction WithLabels(this CodeInstruction code, params Label[] labels)
        {
            foreach (Label label in labels)
                code.labels.Add(label);

            return code;
        }
        public static bool AddDictionaryEntry<T>(IModContentHelper helper, IAssetData asset, object key, string path)
        {
            if (!typeof(T).IsGenericType || typeof(T).GetGenericTypeDefinition() != typeof(Dictionary<,>))
                return false;

            Type[] types = typeof(T).GetGenericArguments();
            if(key.GetType().IsAssignableTo(types[0]))
                return false;

            addItemMethod.MakeGenericMethod(types).Invoke(null, new object[] {helper, asset, key, path});
            return true;
        }
        public static void AddItem<k, v>(IModContentHelper helper, IAssetData asset, k key, string path)
        {
            var model = asset.AsDictionary<k, v>().Data;
            var entry = helper.Load<v>($"assets/{path}");
            model.Add(key, entry);
        }
    }
}
