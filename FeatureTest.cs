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
        private static readonly Point tileSize = new(64, 64);
        private static Point tile = new();
        internal static void Init()
        {
            Patches.Lighting.LightingEvent += DoLightDraw;
            ModEntry.helper.Events.Input.CursorMoved += UpdateMousePos;
            Patches.Action.ActionCursors.Add("Mailbox", 1);
        }
        private static void UpdateMousePos(object _, CursorMovedEventArgs ev)
        {
            tile = ev.NewPosition.Tile.ToPoint() * tileSize;
        }
        private static void DoLightDraw(LightingEventArgs ev)
        {
            int ts = (int)(64 * ev.scale);
            ev.batch.Draw(Game1.staminaRect, new Rectangle(ev.GlobalToLocal(tile), new(ts, ts)), Color.Black);
            ev.batch.Draw(Game1.staminaRect, new Rectangle(1, 1, 1, 1), Color.Black);
        }
    }
}
