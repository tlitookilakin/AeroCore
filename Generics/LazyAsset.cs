using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;

namespace AeroCore.Generics
{
    public class LazyAsset<T> : IDisposable
    {
        private IModHelper helper;
        private Func<string> getPath;
        private T cached = default;
        private bool isCached = false;
        private Func<T> getDefault;
        private IMonitor monitor;
        private bool disposedValue;

        public LogLevel errorLevel;
        public T Value => GetAsset();

        public LazyAsset(IModHelper Helper, IMonitor Monitor, Func<string> AssetPath, Func<T> DefaultAsset = null, LogLevel ErrorLevel = LogLevel.Trace)
        {
            getPath = AssetPath;
            errorLevel = ErrorLevel;
            getDefault = DefaultAsset;
            helper = Helper;
            monitor = Monitor;

            helper.Events.Content.AssetsInvalidated += WatchContent;
        }
        private void WatchContent(object sender, AssetsInvalidatedEventArgs ev)
        {
            string path = getPath();
            foreach (var name in ev.Names)
                if (name.IsEquivalentTo(path))
                {
                    Reload(); 
                    return;
                }
        }
        public T GetAsset()
        {
            if (!isCached)
            {
                string path = getPath();
                try
                {
                    cached = helper.GameContent.Load<T>(path);
                }
                catch (ContentLoadException e)
                {
                    monitor.Log($"Could not load asset at path '{path}':\n{e.Message}", errorLevel);
                    cached = getDefault is not null ? getDefault() : default;
                }
                isCached = true;
            }
            return cached;
        }
        private void Reload()
        {
            cached = default;
            isCached = false;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    helper.Events.Content.AssetsInvalidated -= WatchContent;
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
