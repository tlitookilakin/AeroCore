using AeroCore.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;

namespace AeroCore.Particles
{
    [ModInit]
    public class SimpleSkin : IParticleSkin
    {
        internal static void Init()
        {
            API.API.knownPartSkins.Add("simple", (c) => new SimpleSkin(c));
        }

        public string Texture
        {
            get => textureName;
            set
            {
                textureName = value;
                texture = ModEntry.helper.GameContent.Load<Texture2D>(value);
            }
        }
        private string textureName;
        private Texture2D texture;
        public string[] Colors
        {
            get => colNames;
            set
            {
                colNames = value;
                var cs = new List<Color>();
                foreach (var s in value)
                    if(s.TryParseColor(out var c))
                        cs.Add(c);
                colors = cs.ToArray();
            }
        }
        private Color[] colors = Array.Empty<Color>();
        private string[] colNames = Array.Empty<string>();
        public Rectangle Region { get; set; }
        public int Variants { get; set; } = 1;
        public int FrameCount { get; set; } = 1;
        public int FrameSpeed { get; set; } = 250;
        public int ColorTime { get; set; } = 1000;
        public bool LoopColor { get; set; } = false;
        public float MinScale { get; set; } = 1f;
        public float MaxScale { get; set; } = 1f;
        public float MinSpin { get; set; } = 0f;
        public float MaxSpin { get; set; } = 0f;

        private int[] variant;
        private float[] spin;
        private float[] scales;
        private Rectangle[] regions;
        private int[] frames;
        private Vector2 origin;

        public SimpleSkin(int count)
        {
            variant = new int[count];
            spin = new float[count];
            scales = new float[count];
            regions = new Rectangle[count];
            frames = new int[count];
        }
        public void Cleanup()
        {
            variant.Clear();
            spin.Clear();
            scales.Clear();
            regions.Clear();
            frames.Clear();
        }

        public void Draw(SpriteBatch batch, Vector2[] positions, int[] life, Vector2 scale, Vector2 offset = default, float depth = 0)
        {
            if (texture is null || Variants < 1 || FrameCount < 1)
                return;

            for(int i = 0; i < positions.Length; i++)
            {
                if (life[i] <= 0)
                    continue;

                var clife = life[i];
                if (clife == 1) 
                {
                    variant[i] = Game1.random.Next(0, Variants);
                    spin[i] = (float)(Game1.random.NextDouble() * (MaxSpin - MinSpin) + MinSpin) / 1000f;
                    scales[i] = (float)(Game1.random.NextDouble() * (MaxScale - MinScale) + MinScale);
                    frames[i] = 0;
                    regions[i] = CalculateRegion(0, variant[i]);
                }
                var cframe = clife / FrameSpeed % FrameCount;
                if (cframe != frames[i])
                {
                    frames[i] = cframe;
                    regions[i] = CalculateRegion(cframe, variant[i]);
                }
                int whichc = clife / ColorTime;
                batch.Draw(
                    texture,
                    positions[i] + offset,
                    regions[i],
                    Color.Lerp(ColorAt(whichc), ColorAt(whichc + 1), clife % ColorTime),
                    spin[i] * clife,
                    origin,
                    scales[i] * scale,
                    SpriteEffects.None,
                    depth
                    );
            }
        }

        public void Startup()
        {
            origin = new(Region.Width / 2, Region.Height / 2);
        }

        private Rectangle CalculateRegion(int frame, int variant)
            => new(
                Region.X + frame * Region.Width,
                Region.Y + variant * Region.Height,
                Region.Width, Region.Height
                );
        private Color ColorAt(int index)
            => LoopColor ? colors[index % colors.Length] : colors[Math.Min(index, colors.Length - 1)];
    }
}
