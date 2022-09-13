using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;

namespace AeroCore.Patches
{
    [HarmonyPatch(typeof(Game1))]
    internal class FadeHooks
    {
        internal static event Action<int> AfterFadeOut;
        internal static event Action<int> AfterFadeIn;

        [HarmonyPatch("onFadeToBlackComplete")]
        [HarmonyPostfix]
        internal static void FadeOut()
        {
            AfterFadeOut?.Invoke(Context.ScreenId);
        }

        [HarmonyPatch("onFadedBackInComplete")]
        [HarmonyPostfix]
        internal static void FadeIn()
        {
            AfterFadeIn?.Invoke(Context.ScreenId);
        }
    }
}
