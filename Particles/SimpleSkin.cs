﻿using AeroCore.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;

namespace AeroCore.Particles
{
    public class SimpleSkin : IParticleSkin
    {

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
        private Color[] colors = { Color.White };
        private string[] colNames = { "White" };
        public Rectangle Region { get; set; }
        public int Variants { get; set; } = 1;
        public int HorizontalVariants { get; set; } = 1;
        public int FrameCount { get; set; } = 1;
        public int HorizontalFrames { get; set; } = 1;
        public int FrameLoops { get; set; } = 1;
        public int ColorLoops { get; set; } = 1;
        public float MinScale { get; set; } = 1f;
        public float MaxScale { get; set; } = 1f;
        public float MinSpin { get; set; } = 0f;
        public float MaxSpin { get; set; } = 0f;
        public bool UseYDepth { get; set; } = false;
        public float DepthOffset { get; set; } = 0f;

        private int[] variant;
        private float[] spin;
        private float[] scales;
        private Rectangle[] regions;
        private int[] frames;
        private Vector2 origin;

        public void Init(int count)
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

        public void Draw(SpriteBatch batch, Vector2[] positions, int[] life, int[] maxLife, Vector2 scale, Vector2 offset = default, float depth = 0)
        {
            if (texture is null || Variants < 1 || FrameCount < 1 || colors.Length < 1)
                return;

            for(int i = 0; i < positions.Length; i++)
            {
                if (life[i] == 0)
                    continue;

                var clife = life[i];
                var mlife = maxLife[i];
                if (clife < 0) 
                {
                    variant[i] = Game1.random.Next(0, Variants);
                    var max = MathF.Max(MaxSpin, MinSpin);
                    spin[i] = (float)(Game1.random.NextDouble() * (max - MinSpin) + MinSpin) / 1000f;
                    max = MathF.Max(MaxScale, MinScale);
                    scales[i] = (float)(Game1.random.NextDouble() * (max - MinScale) + MinScale);
                    frames[i] = 0;
                    regions[i] = CalculateRegion(0, variant[i]);
                    clife = -clife;
                }
                var cframe = (FrameLoops >= 0 ? clife * (FrameLoops * FrameCount / mlife) : life[i] * -FrameLoops / 1000) % FrameCount;
                if (cframe != frames[i])
                {
                    frames[i] = cframe;
                    regions[i] = CalculateRegion(cframe, variant[i]);
                }
                int ctime = mlife / Math.Max(ColorLoops * colors.Length - 1, 1);
                int whichc = clife / ctime % colors.Length;
                batch.Draw(
                    texture,
                    positions[i] + offset,
                    regions[i],
                    colors[whichc].Interpolate(colors[(whichc + 1) % colors.Length], clife % ctime / (float)ctime),
                    spin[i] * clife,
                    origin,
                    scales[i] * scale,
                    SpriteEffects.None,
                    (UseYDepth ? positions[i].Y * .0001f : depth) + DepthOffset
                    );
            }
        }

        public void Startup()
        {
            origin = new(Region.Width / 2, Region.Height / 2);
        }

        private Rectangle CalculateRegion(int frame, int variant)
            => new(
                Region.X + variant % HorizontalVariants * Region.Width * HorizontalFrames + frame % HorizontalFrames * Region.Width,
                Region.Y + variant / HorizontalVariants * Region.Height * (FrameCount / HorizontalFrames + (FrameCount % HorizontalFrames == 0 ? 0 : 1))
                + frame / HorizontalFrames * Region.Height,
                Region.Width, Region.Height
                );
    }
}
