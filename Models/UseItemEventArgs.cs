using AeroCore.API;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using StardewValley;

namespace AeroCore.Models
{
    public class UseItemEventArgs : IUseItemEventArgs
    {
        public GameLocation Where { get; }
        public Point Tile { get; }
        public Farmer Who { get; }
        public bool NormalGameplay { get; }
        public bool IsTool { get; }
        public Item Item { get; }
        public string ItemStringID { get; }
        public bool IsHandled
        {
            get => isHandled;
            set => isHandled = value || isHandled;
        }
        private bool isHandled = false;

        internal UseItemEventArgs(bool isTool, Item what)
        {
            Where = Game1.currentLocation;
            Who = Game1.player;
            NormalGameplay = GetIsNormalGameplay();
            Item = what;
            IsTool = isTool;
            Tile = Who.getTileLocationPoint();
            ItemStringID = Item.GetStringID();
        }
        private static bool GetIsNormalGameplay()
        {
            return 
                !Game1.eventUp && 
                !Game1.isFestival() && 
                !Game1.fadeToBlack && 
                !Game1.player.swimming.Value && 
                !Game1.player.bathingClothes.Value && 
                !Game1.player.onBridge.Value;
        }
    }
}
