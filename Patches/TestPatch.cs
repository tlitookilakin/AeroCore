using AeroCore.Models;
using AeroCore.Utils;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroCore.Patches
{
    [ModInit]
    internal class TestPatch
    {
        internal static void Init()
        {
            ModEntry.monitor.Log("test init");
        }
    }
}
