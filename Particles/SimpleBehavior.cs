﻿using AeroCore.Utils;
using Microsoft.Xna.Framework;
using StardewValley;
using System;

namespace AeroCore.Particles
{
    public class SimpleBehavior : IParticleBehavior
    {
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
        private double[] curve;
        private float[] acceleration;

        public void Init(int count)
        {
            speed = new Vector2[count];
            curve = new double[count];
            acceleration = new float[count];
        }

        public void Cleanup()
        {
            speed.Clear();
            curve.Clear();
            acceleration.Clear();
        }

        public void Startup(){
            //noop
        }

        public void Tick(ref Vector2[] positions, ref int[] life, ref int[] maxLife, int millis)
        {
            for(int i = 0; i < life.Length; i++)
            {
                if (life[i] == 0)
                    continue;
                int clife = life[i];
                if (clife < 0) //just spawned
                {
                    if (maxLife[i] == 0)
                    {
                        float spd = Game1.random.Next(MinSpeed, MaxSpeed) / 1000f;
                        float dir = (float)Game1.random.NextDouble() * DirectionVariance * 2f - DirectionVariance + Direction;
                        speed[i] = Data.DirLength(Data.DegToRad(dir), spd);
                        maxLife[i] = Game1.random.Next(MinLife, MaxLife) * 1000;
                        curve[i] = Data.DegToRad(Game1.random.NextDouble() * (MaxCurve - MinCurve) + MinCurve) / 1000f;
                        acceleration[i] = (float)Game1.random.NextDouble() * (MaxAcceleration - MinAcceleration) + MinAcceleration;
                        clife = -clife;
                    } else
                    {
                        clife = millis - clife;
                        life[i] = clife;
                    }
                }
                else
                {
                    life[i] += millis;
                    clife += millis;
                }
                if (clife >= maxLife[i]) //dying
                {
                    life[i] = 0;
                    maxLife[i] = 0;
                }
                else //moving
                {
                    positions[i] = positions[i] + (speed[i] * clife);
                    speed[i] = speed[i].Rotate((float)(curve[i] * clife)) * (1f + acceleration[i]);
                }
            }
        }
    }
}
