using AeroCore.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;

namespace AeroCore.User
{
    [ModInit]
    internal class CursorLight
    {
        private static readonly Vector2 tileSize = new(64, 64);
        private static readonly Vector2 offset = new(-32, -32);
        private static Vector2 tile = new();
        internal static bool isLightActive = false;
        private static Texture2D tex; 
        internal static void Init()
        {
            tex = ModEntry.helper.ModContent.Load<Texture2D>("assets/cursorlight.png");
            Patches.Lighting.LightingEvent += DoLightDraw;
            ModEntry.helper.Events.Input.CursorMoved += UpdateMousePos;
            ModEntry.helper.Events.Input.ButtonPressed += ButtonPressed;
            ModEntry.helper.Events.Input.ButtonReleased += ButtonReleased;
        }
        private static void UpdateMousePos(object _, CursorMovedEventArgs ev)
            => tile = ev.NewPosition.Tile * tileSize + offset;
        private static void ButtonPressed(object _, ButtonPressedEventArgs ev)
        {
            if (ModEntry.Config.CursorLightBind.JustPressed())
                isLightActive = ModEntry.Config.CursorLightHold || !isLightActive;
        }
        private static void ButtonReleased(object _, ButtonReleasedEventArgs ev)
        {
            if (ModEntry.Config.CursorLightHold)
                isLightActive = false;
        }
        private static void DoLightDraw(LightingEventArgs ev)
        {
            if (isLightActive)
                ev.batch.Draw(
                    tex, ev.GlobalToLocal(tile), null,
                    Color.Black * ModEntry.Config.CursorLightIntensity,
                    0f, Vector2.Zero, ev.scale * 4f, SpriteEffects.None, 0f
                );
        }
    }
}
