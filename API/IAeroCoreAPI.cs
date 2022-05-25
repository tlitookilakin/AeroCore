using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace AeroCore.API
{
    public interface IAeroCoreAPI
    {
        public event Action<ILightingEventArgs> LightingEvent;
        public void RegisterAction(string name, Action<Farmer, string, Point> action, int cursor = 0);
        public void UnregisterAction(string name);
        public void ChangeActionCursor(string name, int which);
    }
}
