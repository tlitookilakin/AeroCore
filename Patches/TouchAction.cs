using AeroCore.API;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace AeroCore.Patches
{
    [HarmonyPatch]
    internal class TouchAction
    {
        internal static readonly Dictionary<string, IAeroCoreAPI.ActionHandler> Actions = new();

        [HarmonyPatch(typeof(GameLocation),nameof(GameLocation.performTouchAction))]
        [HarmonyPostfix]
        internal static void DoTouchAction(GameLocation __instance, string fullActionString, Vector2 playerStandingPosition)
        {
            string name = fullActionString.GetChunk(' ', 0);
            if (!Game1.eventUp && Actions.TryGetValue(name, out var exec))
                exec(Game1.player, fullActionString[name.Length..], playerStandingPosition.ToPoint(), __instance);
        }
    }
}
