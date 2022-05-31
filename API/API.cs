using AeroCore.Models;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AeroCore.API
{
    public class API : IAeroCoreAPI
    {
        #region interface_accessible
        public event Action<ILightingEventArgs> LightingEvent;
        public event Action<IUseItemEventArgs> UseItemEvent;
        public event Action<IUseObjectEventArgs> UseObjectEvent;
        public event Action<IHeldItemEventArgs> ItemHeldEvent;
        public event Action<IHeldItemEventArgs> StopItemHeldEvent;

        public void RegisterAction(string name, Action<Farmer, string, Point> action, int cursor = 0)
        {
            Patches.Action.Actions[name] = action;
            ChangeActionCursor(name, cursor);
        }
        public void UnregisterAction(string name)
        {
            Patches.Action.Actions.Remove(name);
            Patches.Action.ActionCursors.Remove(name);
        }
        public void ChangeActionCursor(string name, int cursor)
        {
            cursor = Math.Clamp(cursor, 0, 7);
            if (cursor != 0)
                Patches.Action.ActionCursors[name] = cursor;
            else
                Patches.Action.ActionCursors.Remove(name);
        }
        #endregion interface_accessible
        /// <summary>Initializes all <see cref="ModInitAttribute"/> marked classes in your mod</summary>
        /// <param name="ModClass">Any type from your mod</param>
        public void InitAll(Type ModClass)
        {
            var ass = ModClass.Assembly;
            foreach(var type in ass.DefinedTypes)
            {
                var init = (from n in type.CustomAttributes where n.AttributeType == typeof(ModInitAttribute) select n).FirstOrDefault();
                if (init is not null)
                {
                    string method = "Init";
                    foreach (var arg in init.NamedArguments)
                        switch (arg.MemberName) {
                            case "Method": method = (string)arg.TypedValue.Value ?? "Init"; break;
                            case "WhenHasMod":
                                var v = (string)arg.TypedValue.Value;
                                if (v is not null && !ModEntry.helper.ModRegistry.IsLoaded(v))
                                    goto done;
                                break;
                        }
                    var m = AccessTools.DeclaredMethod(type, method);
                    if (m is not null && m.IsStatic)
                        m.Invoke(null, Array.Empty<object>());
                }
                done:
                continue;
            }
        }
        #region internals
        internal API() {
            Patches.Lighting.LightingEvent += (e) => LightingEvent?.Invoke(e);
            Patches.UseItem.OnUseItem += (e) => UseItemEvent?.Invoke(e);
            Patches.UseObject.OnUseObject += (e) => UseObjectEvent?.Invoke(e);
            Patches.UseItem.OnItemHeld += (e) => ItemHeldEvent?.Invoke(e);
            Patches.UseItem.OnStopItemHeld += (e) => StopItemHeldEvent?.Invoke(e);
        }
        #endregion internals
    }
}
