using HarmonyLib;
using StardewValley;
using System;

namespace AeroCore.Patches
{
    [HarmonyPatch(typeof(GameLocation))]
    internal class LocationCleanup
    {
        internal static event Action<GameLocation> Cleanup;

        [HarmonyPatch(nameof(GameLocation.cleanupBeforePlayerExit))]
        [HarmonyPostfix]
        internal static void AfterCleanup(GameLocation __instance)
            => Cleanup?.Invoke(__instance);
    }
}
