using AeroCore.Utils;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroCore.Particles
{
    public class SimpleBehavior : IParticleBehavior
    {
        public float Direction { get; set; }
        public float DirectionVariance { get; set; }
        public int MinSpeed { get; set; }
        public int MaxSpeed { get; set; }
        public int MinLife { get; set; }
        public int MaxLife { get; set; }
        public float MinCurve { get; set; }
        public float MaxCurve { get; set; }
        public float MinAcceleration { get; set; }
        public float MaxAcceleration { get; set; }

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
                if (life[i] == 1) //just spawned
                {
                    float spd = Game1.random.Next(MinSpeed, MaxSpeed) / 1000f;
                    float dir = (float)Game1.random.NextDouble() * DirectionVariance * 2f - DirectionVariance + Direction;
                    speed[i] = Data.DirLength(Data.DegToRad(dir), spd);
                    lifetime[i] = Game1.random.Next(MinLife, MaxLife);
                    curve[i] = Data.DegToRad(Game1.random.NextDouble() * (MaxCurve - MinCurve) + MinCurve);
                    acceleration[i] = (float)Game1.random.NextDouble() * (MaxAcceleration - MinAcceleration) + MinAcceleration;
                    life[i] = 0;
                }
                life[i] += millis;
                if (life[i] >= lifetime[i]) //dying
                {
                    life[i] = 0;
                } else //moving
                {
                    positions[i] = positions[i] + (speed[i] * millis);
                    speed[i] = speed[i].Rotate((float)(curve[i] * millis)) * (1f + acceleration[i]);
                }
            }
        }
    }
}
