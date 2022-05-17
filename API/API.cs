using AeroCore.Models;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AeroCore.API
{
    public class API : IAeroCoreAPI
    {
        public event Action<ILightingEventArgs> LightingEvent;

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
        internal void DoLighting(ILightingEventArgs ev) => LightingEvent?.Invoke(ev);
        internal API() {
            Patches.Lighting.LightingEvent += DoLighting;
        }
        #endregion internals
    }
}
