using System;

namespace AeroCore
{
    /// <summary>Allows easily initializing static data from <see cref="API.InitAll()"/></summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public class ModInitAttribute : Attribute
    {
        public string Method { get; set; }
        public string WhenHasMod { get; set; }
        public ModInitAttribute()
        {
            Method = "Init";
            WhenHasMod = null;
        }
    }
}
