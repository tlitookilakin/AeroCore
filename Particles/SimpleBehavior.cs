using AeroCore.Utils;
using Microsoft.Xna.Framework;
using StardewValley;
using System;

namespace AeroCore.Particles
{
    [ModInit]
    public class SimpleBehavior : IParticleBehavior
    {
        internal static void Init()
        {
            API.API.knownPartBehaviors.Add("simple", (c) => new SimpleBehavior(c));
        }

        public float Direction { get; set; } = 0f;
        public float DirectionVariance { get; set; } = 0f;
        public int MinSpeed { get; set; } = 16;
        public int MaxSpeed { get; set; } = 16;
        public int MinLife { get; set; } = 10;
        public int MaxLife { get; set; } = 10;
        public float MinCurve { get; set; } = 0f;
        public float MaxCurve { get; set; } = 0f;
        public float MinAcceleration { get; set; } = 0f;
        public float MaxAcceleration { get; set; } = 0f;

        private Vector2[] speed;
        private int[] lifetime;
        private double[] curve;
        private float[] acceleration;

        public SimpleBehavior(int count)
        {
            speed = new Vector2[count];
            lifetime = new int[count];
            curve = new double[count];
            acceleration = new float[count];
        }

        public void Cleanup()
        {
            speed.Clear();
            lifetime.Clear();
            curve.Clear();
            acceleration.Clear();
        }

        public void Startup(){
            //noop
        }

        public void Tick(ref Vector2[] positions, ref int[] life, int millis)
        {
            for(int i = 0; i < life.Length; i++)
            {
                if (life[i] <= 0)
                    continue;
                if (life[i] == 1) //just spawned
                {
                    float spd = Game1.random.Next(MinSpeed, MaxSpeed) / 1000f;
                    float dir = (float)Game1.random.NextDouble() * DirectionVariance * 2f - DirectionVariance + Direction;
                    speed[i] = Data.DirLength(Data.DegToRad(dir), spd);
                    lifetime[i] = Game1.random.Next(MinLife, MaxLife) * 1000;
                    curve[i] = Data.DegToRad(Game1.random.NextDouble() * (MaxCurve - MinCurve) + MinCurve);
                    acceleration[i] = (float)Game1.random.NextDouble() * (MaxAcceleration - MinAcceleration) + MinAcceleration;
                    life[i] = 0;
                }
                else
                {
                    life[i] += millis;
                    if (life[i] >= lifetime[i]) //dying
                    {
                        life[i] = 0;
                    }
                    else //moving
                    {
                        positions[i] = positions[i] + (speed[i] * millis);
                        speed[i] = speed[i].Rotate((float)(curve[i] * millis)) * (1f + acceleration[i]);
                    }
                }
            }
        }
    }
}
