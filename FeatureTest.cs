using AeroCore.Models;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;

namespace AeroCore
{
    [ModInit]
    internal class FeatureTest
    {
        internal static void Init()
        {
            Patches.Action.ActionCursors.Add("Mailbox", 1);
        }
    }
}
