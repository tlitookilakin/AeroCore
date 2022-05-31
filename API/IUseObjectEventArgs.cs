using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace AeroCore.API
{
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
