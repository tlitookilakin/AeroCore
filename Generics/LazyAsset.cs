using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Runtime.CompilerServices;

namespace AeroCore.Generics
{
    [ModInit]
    public abstract class LazyAsset
    {
        internal Func<string> getPath;
        internal bool ignoreLocale;

        internal static readonly ConditionalWeakTable<LazyAsset, IModHelper> Watchers = new();
        internal static void Init()
        {
            ModEntry.helper.Events.Content.AssetsInvalidated += CheckWatchers;
        }
        internal static void CheckWatchers(object _, AssetsInvalidatedEventArgs ev)
        {
            foreach ((var asset, var helper) in Watchers) 
            {
                string path = asset.getPath();
                foreach (var name in asset.ignoreLocale ? ev.NamesWithoutLocale : ev.Names)
                {
                    if (name.IsEquivalentTo(path))
                    {
                        asset.Reload(); break;
                    }
                }
            }
        }
        internal abstract void Reload();
    }
    public class LazyAsset<T> : LazyAsset
    {
        private IModHelper helper;
        private T cached = default;
        private bool isCached = false;

        public LogLevel errorLevel;
        public T Value => GetAsset();

        public LazyAsset(IModHelper Helper, Func<string> AssetPath, bool IgnoreLocale = true, LogLevel ErrorLevel = LogLevel.Trace)
        {
            getPath = AssetPath;
            errorLevel = ErrorLevel;
            helper = Helper;
            ignoreLocale = IgnoreLocale;

            Watchers.Add(this, Helper);
        }
        public T GetAsset()
        {
            if (!isCached)
            {
                isCached = true;
                cached = helper.GameContent.Load<T>(getPath());
            }
            return cached;
        }
        internal override void Reload()
        {
            cached = default;
            isCached = false;
        }
    }
}
