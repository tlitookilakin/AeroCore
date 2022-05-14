using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroCore.Models
{
    /// <summary>Allows easily initializing static data from <see cref="API.InitAll(Type)"/></summary>
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
