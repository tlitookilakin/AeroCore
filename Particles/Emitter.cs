using Microsoft.Xna.Framework;
using StardewValley;
using System;

namespace AeroCore.Particles
{
    public class Emitter : IParticleEmitter
    {
        public Rectangle Region { get; set; } = new();
        public int Rate
        {
            get => rate;
            set => rate = Math.Max(value, 1);
        }
        private int rate = 100;
        public int RateVariance
        {
            get => rateVariance;
            set => rateVariance = Math.Max(value, 0);
        }
        private int rateVariance = 0;
        public int BurstMin
        {
            get => burstMin;
            set => burstMin = Math.Max(value, 1);
        }
        private int burstMin = 1;
        public int BurstMax
        {
            get => burstMax;
            set => burstMax = Math.Max(value, burstMin);
        }
        private int burstMax = 1;
        public bool Radial { get; set; } = false;

        private int timeSinceLast;
        private int variance;

        public Emitter()
        {
            Reset();
        }
        public Emitter(IParticleEmitter from) : this()
        {
            Region = from.Region;
            Rate = from.Rate;
            RateVariance = from.RateVariance;
            BurstMin = from.BurstMin;
            BurstMax = from.BurstMax;
            Radial = from.Radial;
        }

        public void Reset()
        {
            timeSinceLast = 0;
            variance = Game1.random.Next(rateVariance * 2) - rateVariance;
        }

        public void Tick(ref Vector2[] position, ref int[] life, int millis)
        {
            timeSinceLast += millis;
            int pindex = 0;
            while (timeSinceLast >= rate + variance)
            {
                timeSinceLast -= rate + variance;
                variance = Game1.random.Next(rateVariance * 2) - rateVariance;
                int count = Game1.random.Next(burstMax - burstMin) + burstMin;
                while (count > 0 && pindex < life.Length)
                {
                    count--;
                    while (pindex < life.Length && life[pindex] > 0)
                        pindex++;
                    if (pindex < life.Length)
                        Emit(ref position, ref life, pindex);
                }
            }
        }
        private void Emit(ref Vector2[] pos, ref int[] life, int index)
        {
            if (Radial)
            {
                float dir = (float)Game1.random.NextDouble() * MathF.PI * 2f;
                float dist = (float)Game1.random.NextDouble();
                pos[index] = new Vector2(MathF.Cos(dir) * dist * Region.Width, MathF.Sin(dir) * dist * Region.Height);
            } else
            {
                pos[index] = new(Game1.random.Next(Region.Width) + Region.X, Game1.random.Next(Region.Height) + Region.Y);
            }
            life[index] = 1;
        }
    }
}
