using System;
using System.Collections.Generic;

namespace AeroCore.API
{
    public interface IAeroCoreAPI
    {
        public event Action<ILightingEventArgs> LightingEvent;
    }
}
