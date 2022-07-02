using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using SObject = StardewValley.Object;
using System;

namespace AeroCore.API
{
    public interface IHeldItemEventArgs
    {
        public Item Item { get; }
        public Farmer Who { get; }
        public string StringId { get; }
    }
    public interface ILightingEventArgs
    {
        public float intensity { get; }
        public Color ambient { get; }
        public Vector2 offset { get; }
        public Vector2 worldOffset { get; }
        public float scale { get; }
        public SpriteBatch batch { get; }
        public Vector2 GlobalToLocal(Vector2 position);
        public Vector2 ScreenToLocal(Vector2 position);
        public Point GlobalToLocal(Point position);
        public Point ScreenToLocal(Point position);
    }
    public interface IUseItemEventArgs
    {
        public GameLocation Where { get; }
        public Point Tile { get; }
        public Farmer Who { get; }
        public bool NormalGameplay { get; }
        public bool IsTool { get; }
        public Item Item { get; }
        public bool IsHandled { get; set; }
        public string ItemStringID { get; }
    }
    public interface IUseObjectEventArgs
    {
        public SObject Object { get; }
        public Vector2 Tile { get; }
        public Farmer Who { get; }
        public string ObjectStringID { get; }
        public bool IsChecking { get; }
        public bool IsHandled { get; set; }
    }
}
