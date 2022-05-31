using StardewValley;

namespace AeroCore.API
{
    public interface IHeldItemEventArgs
    {
        public Item Item { get; }
        public Farmer Who { get; }
        public string StringId { get; }
    }
}
