using Microsoft.Xna.Framework;
using StardewValley;

namespace AeroCore.API
{
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
}
