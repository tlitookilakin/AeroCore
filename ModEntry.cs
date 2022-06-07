using AeroCore.Integration;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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
        internal static IDGAAPI DGA;

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

            helper.Events.GameLoop.SaveLoaded += EnteredWorld;
            helper.Events.GameLoop.GameLaunched += Init;
        }
        private void Init(object _, GameLaunchedEventArgs ev)
        {
            if (helper.ModRegistry.IsLoaded("spacechase0.DynamicGameAssets"))
                DGA = helper.ModRegistry.GetApi<IDGAAPI>("spacechase0.DynamicGameAssets");
            api.InitAll();
            harmony.PatchAll();
        }
        private void EnteredWorld(object _, SaveLoadedEventArgs ev)
        {
            Utils.Reflection.mp = mp.GetValue();
        }
        public override object GetApi() => api;
        public static API.API GetStaticApi() => api;
    }
}
