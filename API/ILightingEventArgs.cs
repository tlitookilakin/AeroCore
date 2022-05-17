﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AeroCore.API
{
    public interface ILightingEventArgs
    {
        public float intensity { get; }
        public Color ambient { get; }
        public Vector2 offset { get; }
        public float scale { get; }
        public SpriteBatch batch { get; }
        public Vector2 GlobalToLocal(Vector2 position);
        public Vector2 ScreenToLocal(Vector2 position);
        public Point GlobalToLocal(Point position);
        public Point ScreenToLocal(Point position);
    }
}
