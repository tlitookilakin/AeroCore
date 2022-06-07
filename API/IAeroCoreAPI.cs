using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace AeroCore.API
{
    public interface IAeroCoreAPI
    {
        public delegate void ActionHandler(Farmer who, string what, Point tile, GameLocation where);

        public event Action<ILightingEventArgs> LightingEvent;
        public event Action<IUseItemEventArgs> UseItemEvent;
        public event Action<IUseObjectEventArgs> UseObjectEvent;
        public event Action<IHeldItemEventArgs> ItemHeldEvent;
        public event Action<IHeldItemEventArgs> StopItemHeldEvent;
        public void RegisterAction(string name, ActionHandler action, int cursor = 0);
        public void UnregisterAction(string name);
        public void ChangeActionCursor(string name, int which);
        public void RegisterTouchAction(string name, ActionHandler action);
        public void UnregisterTouchAction(string name);
    }
}
