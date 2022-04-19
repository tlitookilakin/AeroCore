using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Layers;

namespace AeroCore.Utils
{
    public static class Maps
    {
        public static string[] MapPropertyArray(GameLocation loc, string prop) => loc.getMapProperty(prop).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        public static IEnumerable<(xTile.Tiles.Tile, int, int)> tilesInLayer(Layer layer)
        {
            if (layer == null)
                yield break;

            for (int x = 0; x < layer.LayerWidth; x++)
            {
                for (int y = 0; y < layer.LayerHeight; y++)
                {
                    var tile = layer.Tiles[x, y];
                    if (tile != null)
                    {
                        yield return (tile, x, y);
                    }
                }
            }
        }
        public static IEnumerable<(xTile.Tiles.Tile, int, int)> tilesInLayer(xTile.Map map, string layerName)
        {
            foreach (var item in tilesInLayer(map.GetLayer(layerName)))
                yield return item;
        }
        public static bool TileHasProperty(this xTile.Tiles.Tile tile, string name, out string prop)
        {
            bool ret = tile.Properties.TryGetValue(name, out var val) || tile.TileIndexProperties.TryGetValue(name, out val);
            prop = val?.ToString();
            return ret;
        }
        public static void warpToTempMap(string path, Farmer who)
        {
            GameLocation temp = new(PathUtilities.NormalizeAssetName("Maps/" + path), "Temp");
            temp.map.LoadTileSheets(Game1.mapDisplayDevice);
            //if (path.Trim() == "EventVoid")
                //Events.drawVoid.Value = true; //anti-flicker
            Event e = Game1.currentLocation.currentEvent;
            Game1.currentLocation.cleanupBeforePlayerExit();
            Game1.currentLocation.currentEvent = null;
            Game1.currentLightSources.Clear();
            Game1.currentLocation = temp;
            Game1.currentLocation.resetForPlayerEntry();
            Game1.currentLocation.currentEvent = e;
            Game1.player.currentLocation = Game1.currentLocation;
            who.currentLocation = Game1.currentLocation;
            Game1.panScreen(0, 0);
        }
    }
}
