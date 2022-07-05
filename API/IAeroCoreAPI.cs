using Microsoft.Xna.Framework;
using StardewModdingAPI;
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

        /// <summary>Registers a custom action</summary>
        /// <param name="name">The name of the action</param>
        /// <param name="action">The code to run when the action is activated</param>
        /// <param name="cursor">Which cursor to use when hovering over this action.</param>
        public void RegisterAction(string name, ActionHandler action, int cursor = 0);

        /// <summary>Unregisters a registered action</summary>
        public void UnregisterAction(string name);

        /// <summary>Change the hover cursor of a given action</summary>
        /// <param name="name">The action to change the cursor of</param>
        /// <param name="which">The cursor index to change it to. check LooseSprites/Cursors for reference.</param>
        public void ChangeActionCursor(string name, int which);

        /// <summary>Register a custom touchaction</summary>
        /// <param name="name">The name of the touchaction</param>
        /// <param name="action">The code to run when the touchaction is activated</param>
        public void RegisterTouchAction(string name, ActionHandler action);

        /// <summary>Unregister a registered touch action</summary>
        public void UnregisterTouchAction(string name);

        /// <summary>Initializes all <see cref="ModInitAttribute"/> marked classes in your mod</summary>
        /// <param name="ModClass">Any type from your mod</param>
        public void InitAll();

        /// <summary>Builds and registers a config with GMCM if it is installed. Can be enhanced with attributes.</summary>
        /// <typeparam name="T">The config type</typeparam>
        /// <param name="config">The config instance</param>
        /// <param name="ConfigChanged">An action that is executed whenever settings are changed for this config</param>
        /// <param name="TitleScreenOnly">Whether or not it should only be available on the title screen</param>
        public void RegisterGMCMConfig<T>(IManifest who, IModHelper helper, T config, Action ConfigChanged = null, bool TitleScreenOnly = false) where T : class, new();
    }
}
