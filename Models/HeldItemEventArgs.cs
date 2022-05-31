using AeroCore.API;
using AeroCore.Utils;
using StardewValley;

namespace AeroCore.Models
{
    public class HeldItemEventArgs : IHeldItemEventArgs
    {
        public Item Item { get; }
        public Farmer Who { get; }
        public string StringId { get; }

        internal HeldItemEventArgs(Farmer who, Item item)
        {
            Who = who;
            Item = item;
            StringId = item.GetStringID();
        }
    }
}
