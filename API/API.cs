﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        public void RegisterAction(string name, IAeroCoreAPI.ActionHandler action, int cursor = 0)
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
        public void RegisterTouchAction(string name, IAeroCoreAPI.ActionHandler action)
            => Patches.TouchAction.Actions.Add(name, action);
        public void UnregisterTouchAction(string name)
            => Patches.TouchAction.Actions.Remove(name);

        public void RegisterGMCMConfig<T>(IManifest who, IModHelper helper, T config, Action ConfigChanged = null, bool TitleScreenOnly = false) 
            where T : class, new()
        {
            if (!ModEntry.helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
                return;
            gmcm ??= ModEntry.helper.ModRegistry.GetApi<IGMCMAPI>("spacechase0.GenericModConfigMenu");

            var type = typeof(T);
            var defaults = new Dictionary<PropertyInfo, object>();
            var i18n = helper.Translation;
            foreach (var prop in type.GetProperties())
                defaults.Add(prop, prop.GetValue(config));
            defaultConfigValues.Add(who, defaults);
            gmcm.Register(
                who,
                () => ResetConfig(who, config),
                () => { helper.WriteConfig(config); ConfigChanged?.Invoke(); },
                TitleScreenOnly
            );
            var pages = new Dictionary<string, Dictionary<string, List<PropertyInfo>>>();
            foreach (var prop in type.GetProperties())
            {
                var pid = prop.GetCustomAttribute<GMCMPageAttribute>()?.ID ?? string.Empty;
                var sid = prop.GetCustomAttribute<GMCMSectionAttribute>()?.ID ?? string.Empty;
                if (!pages.TryGetValue(pid, out var cpage))
                    cpage = pages[pid] = new();
                if (!cpage.TryGetValue(sid, out var csec))
                    csec = cpage[sid] = new();
                csec.Add(prop);
            }
            foreach((var name, var page) in pages)
            {
                if (name != string.Empty)
                {
                    gmcm.AddPage(who, name, () => i18n.Get($"config.pages.{name}.label"));
                }
                else
                {
                    var img = type.GetCustomAttribute<GMCMImageAttribute>();
                    if (img is not null)
                        gmcm.AddImage(who, () => helper.ModContent.Load<Texture2D>(img.Path), scale: img.Scale);
                }

                foreach((var sname, var section) in page)
                {
                    gmcm.AddSectionTitle(who, () => i18n.Get($"config.{sname}.header"));
                    foreach(var option in section)
                    {
                        var img = option.GetCustomAttribute<GMCMImageAttribute>();
                        if (img is not null)
                            gmcm.AddImage(who, () => helper.ModContent.Load<Texture2D>(img.Path), scale: img.Scale);

                        var t = option.PropertyType;
                        if (t.IsEnum){
                            gmcm.AddTextOption(who,
                                () => option.GetValue(config).ToString(),
                                (s) => option.SetValue(config, Enum.Parse(t, s)),
                                () => i18n.Get($"config.{option.Name}.label"),
                                () => i18n.Get($"config.{option.Name}.desc"),
                                Enum.GetNames(t), (s) => TranslateEnum(helper, t.Name, s)
                            );
                        }
                        else if (t == typeof(string))
                        {
                            gmcm.AddTextOption(who,
                                () => (string)option.GetValue(config),
                                (s) => option.SetValue(config, s),
                                () => i18n.Get($"config.{option.Name}.label"),
                                () => i18n.Get($"config.{option.Name}.desc")
                            );
                        } else if (t == typeof(bool))
                        {
                            gmcm.AddBoolOption(who,
                                () => (bool)option.GetValue(config),
                                (b) => option.SetValue(config, b),
                                () => i18n.Get($"config.{option.Name}.label"),
                                () => i18n.Get($"config.{option.Name}.desc")
                            );
                        } else if (t == typeof(float))
                        {
                            var range = option.GetCustomAttribute<GMCMRangeAttribute>();
                            gmcm.AddNumberOption(who,
                                () => (float)option.GetValue(config),
                                (f) => option.SetValue(config, f),
                                () => i18n.Get($"config.{option.Name}.label"),
                                () => i18n.Get($"config.{option.Name}.desc"),
                                range?.Min, range?.Max
                            );
                        } else if (t == typeof(int))
                        {
                            var range = option.GetCustomAttribute<GMCMRangeAttribute>();
                            gmcm.AddNumberOption(who,
                                () => (int)option.GetValue(config),
                                (i) => option.SetValue(config, i),
                                () => i18n.Get($"config.{option.Name}.label"),
                                () => i18n.Get($"config.{option.Name}.desc"),
                                (int?)range?.Min, (int?)range?.Max
                            );
                        } else if (t == typeof(KeybindList))
                        {
                            gmcm.AddKeybindList(who,
                                () => (KeybindList)option.GetValue(config),
                                (kbl) => option.SetValue(config, kbl),
                                () => i18n.Get($"config.{option.Name}.label"),
                                () => i18n.Get($"config.{option.Name}.desc")
                            );
                        }
                        var text = option.GetCustomAttribute<GMCMParagraphAttribute>();
                        if (text is not null)
                            gmcm.AddParagraph(who, () => i18n.Get($"config.{option.Name}.text"));
                    }
                }

                if (name == string.Empty)
                    foreach (var link in pages.Keys)
                        if (link != string.Empty)
                            gmcm.AddPageLink(who, link,
                                () => i18n.Get($"config.pages.{link}.label"),
                                () => i18n.Get($"config.pages.{link}.desc")
                            );
            }
        }
        public void InitAll(params object[] args)
        {
            foreach(var type in Assembly.GetCallingAssembly().DefinedTypes)
            {
                var init = type.GetCustomAttribute<ModInitAttribute>();
                if (init is not null)
                {
                    string method = init.Method ?? "Init";
                    if (init.WhenHasMod is not null && !ModEntry.helper.ModRegistry.IsLoaded(init.WhenHasMod))
                        continue;
                    var m = AccessTools.DeclaredMethod(type, method);
                    if (m is not null && m.IsStatic)
                        m.Invoke(null, args);
                }
            }
        }
        #endregion interface_accessible
        #region internals
        private static IGMCMAPI gmcm;
        private static readonly Dictionary<IManifest, Dictionary<PropertyInfo, object>> defaultConfigValues = new();
        internal API() {
            Patches.Lighting.LightingEvent += (e) => LightingEvent?.Invoke(e);
            Patches.UseItem.OnUseItem += (e) => UseItemEvent?.Invoke(e);
            Patches.UseObject.OnUseObject += (e) => UseObjectEvent?.Invoke(e);
            Patches.UseItem.OnItemHeld += (e) => ItemHeldEvent?.Invoke(e);
            Patches.UseItem.OnStopItemHeld += (e) => StopItemHeldEvent?.Invoke(e);
        }
        internal static void ResetConfig(IManifest which, object config)
        {
            foreach((var prop, var val) in defaultConfigValues[which])
                prop.SetValue(config, val);
        }
        internal static string TranslateEnum(IModHelper helper, string enumName, string value)
            => helper.Translation.Get($"config.{enumName}.{value}");
        #endregion internals
    }
}
