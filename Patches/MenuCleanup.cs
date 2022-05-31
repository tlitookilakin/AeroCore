using HarmonyLib;
using StardewValley;
using System;

namespace AeroCore.Patches
{
    [HarmonyPatch]
    internal class MenuCleanup
    {
        [HarmonyPatch(typeof(Game1), nameof(Game1.exitActiveMenu))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        internal static void Prefix()
        {
            if (Game1.activeClickableMenu is IDisposable d)
                d.Dispose();
        }
    }
}
