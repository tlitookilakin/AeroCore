using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;

namespace AeroCore.Utils
{
    public static class Drawing
    {
        public static void DrawLine(this SpriteBatch batch, Vector2 from, Vector2 to, Color color, int thickness = 1, float depth = 0f)
            => batch.Draw(Game1.staminaRect, new Rectangle((int)from.X, (int)from.Y - thickness/2, (int)(to - from).Length(), thickness),
                null, color, (float)from.DirectionTo(to), new(0, thickness/2f), SpriteEffects.None, depth);
        public static void DrawLine(this SpriteBatch batch, Vector2 pivot, float angle, int length, Color color, 
            int thickness = 1, float offset = 0f, float depth = 0f)
            => batch.Draw(Game1.staminaRect, new Rectangle((int)(pivot.X - offset), (int)pivot.Y - thickness / 2, length, thickness),
                null, color, angle, new(offset, thickness / 2f), SpriteEffects.None, depth);
        public static void DrawLines(this SpriteBatch batch, IList<Vector2> points, Color color, int thickness = 1, float depth = 0f)
        {
            for (int i = 0; i < points.Count - 1; i++)
                batch.DrawLine(points[i], points[i + 1], color, thickness, depth);
        }
        public static double DirectionTo(this Vector2 from, Vector2 to)
        {
            var v = to - from;
            return Math.Atan2(v.Y, v.X);
        }
    }
}
