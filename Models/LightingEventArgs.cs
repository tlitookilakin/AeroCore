using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace AeroCore.Models
{
    public class LightingEventArgs
    {
        public float intensity { get; }
        public Color ambient { get; }
        public Vector2 offset { get; }
        public float scale { get; }
        public SpriteBatch batch => Game1.spriteBatch;

        internal LightingEventArgs(float intensity, Color ambient, Vector2 offset)
        {
            this.offset = offset;
            this.intensity = intensity;
            this.ambient = ambient;
            this.scale = 2f / Game1.options.lightingQuality;
        }
    }
}
