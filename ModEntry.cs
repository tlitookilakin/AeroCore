using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;

namespace AeroCore
{
    public class ModEntry : Mod
    {
        internal ITranslationHelper i18n => Helper.Translation;
        internal static IMonitor monitor;
        internal static IModHelper helper;
        internal static Harmony harmony;
        internal static string ModID;

        private IReflectedField<Multiplayer> mp;

        public override void Entry(IModHelper helper)
        {
            Monitor.Log("Hello and welcome to the Enrichment Center!", LogLevel.Debug);

            monitor = Monitor;
            ModEntry.helper = Helper;
            harmony = new(ModManifest.UniqueID);
            ModID = ModManifest.UniqueID;

            mp = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer");

            helper.Events.GameLoop.SaveLoaded += (s, e) => Utils.Reflection.mp = mp.GetValue();

            API.InitAll(typeof(ModEntry));
        }
    }
}
