using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroCore.Models
{
    public class UseItemEventArgs
    {
        public readonly GameLocation Where;
        public readonly Point Tile;
        public readonly Farmer Who;
        public readonly bool NormalGameplay;
        public readonly bool IsTool;
        public readonly Item Item;
        public bool IsHandled
        {
            get => isHandled;
            set => isHandled = value || isHandled;
        }
        private bool isHandled = false;

        internal UseItemEventArgs(Item what, GameLocation where, int x, int y, Farmer who)
        {
            Where = where;
            Tile = new(x, y);
            Who = who;
            NormalGameplay = GetIsNormalGameplay();
            Item = what;
            IsTool = true;
        }
        internal UseItemEventArgs(Item what, GameLocation where)
        {
            Where = where;
            Tile = Game1.player.getTileLocationPoint();
            Who = Game1.player;
            NormalGameplay = GetIsNormalGameplay();
            Item = what;
            IsTool = false;
        }
        private bool GetIsNormalGameplay()
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
