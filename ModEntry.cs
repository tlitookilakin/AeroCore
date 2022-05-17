using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;

namespace AeroCore
{
    public class ModEntry : Mod
    {
        internal static ITranslationHelper i18n;
        internal static IMonitor monitor;
        internal static IModHelper helper;
        internal static Harmony harmony;
        internal static string ModID;
        internal static API.API api;

        private IReflectedField<Multiplayer> mp;

        public override void Entry(IModHelper helper)
        {
            Monitor.Log("Hello and welcome to the Enrichment Center!", LogLevel.Debug);

            monitor = Monitor;
            ModEntry.helper = Helper;
            harmony = new(ModManifest.UniqueID);
            ModID = ModManifest.UniqueID;
            api = new();
            i18n = helper.Translation;

            mp = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer");

            helper.Events.GameLoop.SaveLoaded += (s, e) => Utils.Reflection.mp = mp.GetValue();

            api.InitAll(typeof(ModEntry));
            harmony.PatchAll();
        }
        public override object GetApi() => api;
    }
}
