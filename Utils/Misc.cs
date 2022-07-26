using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AeroCore.Utils
{
    public static class Misc
    {
        private static readonly PerScreen<List<Response>> PagedResponses = new(() => new());
        private static readonly PerScreen<int> PageIndex = new();
        private static readonly PerScreen<Action<Farmer, string>> PagedResponseConfirmed = new();
        private static readonly PerScreen<string> PagedQuestion = new();
        public static Point LocalToGlobal(int x, int y) => new(x + Game1.viewport.X, y + Game1.viewport.Y);
        public static Point LocalToGlobal(Point pos) => LocalToGlobal(pos.X, pos.Y);
        public static Vector2 LocalToGlobal(float x, float y) => new(x + Game1.viewport.X, y + Game1.viewport.Y);
        public static Vector2 LocalToGlobal(Vector2 pos) => LocalToGlobal(pos.X, pos.Y);
        public static IEnumerable<Point> PointsIn(this Rectangle rect)
        {
            for (int x = 0; x < rect.Width; x++)
                for (int y = 0; y < rect.Height; y++)
                    yield return new Point(x + rect.X, y + rect.Y);
        }
        public static IList<Point> AllPointsIn(this Rectangle rect)
        {
            var points = new Point[rect.Width * rect.Height];
            for (int x = 0; x < rect.Width; x++)
                for (int y = 0; y < rect.Height; y++)
                    points[x + y * rect.Width] = new(x + rect.X, y + rect.Y);
            return points;
        }
        public static bool IsFestivalAtLocation(string Location)
            => Location is not null && Game1.weatherIcon == 1 && Location.Equals(Game1.whereIsTodaysFest, StringComparison.OrdinalIgnoreCase);
        public static bool IsFestivalReady()
        {
            if (Game1.weatherIcon != 1)
                return true;

            string c = ModEntry.helper.GameContent.Load<Dictionary<string, string>>($"Data/Festivals/{Game1.currentSeason}{Game1.dayOfMonth}")["conditions"];

            return !int.TryParse(c.GetChunk('/', 1).GetChunk(' ',0), out int time) || time <= Game1.timeOfDay;
        }
        public static ReadOnlySpan<T> Concat<T>(this ReadOnlySpan<T> s1, ReadOnlySpan<T> s2)
        {
            var array = new T[s1.Length + s2.Length];
            s1.CopyTo(array);
            s2.CopyTo(array.AsSpan(s1.Length));
            return new(array);
        }
        public static void ShowPagedResponses(string question, Response[] responses, Action<Farmer, string> on_response, bool auto_select_single = false)
        {
            if (responses.Length == 0)
                return;

            if (responses.Length == 1 && auto_select_single)
            {
                on_response(Game1.player, responses[0].responseKey);
                return;
            }

            PagedResponses.Value = new(responses);
            PageIndex.Value = 0;
            PagedResponseConfirmed.Value = on_response;
            PagedQuestion.Value = question;

            ShowResponsePage();
        }
        private static void ShowResponsePage()
        {
            List<Response> visible = new();
            if (PageIndex.Value > 0)
                visible.Add(new("_prevPage", '@' + ModEntry.i18n.Get("misc.generic.previous")));

            for(int i = PageIndex.Value * 5; i < PagedResponses.Value.Count; i++)
                visible.Add(PagedResponses.Value[i]);

            if (PagedResponses.Value.Count > (PageIndex.Value + 1) * 5)
                visible.Add(new("_nextPage", ModEntry.i18n.Get("misc.generic.next") + '>'));

            visible.Add(new("_cancel", ModEntry.i18n.Get("misc.generic.cancel")));

            Game1.currentLocation.createQuestionDialogue(PagedQuestion.Value, visible.ToArray(), HandlePagedResponse);
        }
        private static void HandlePagedResponse(Farmer who, string key)
        {
            if(key == "_nextPage" || key == "_prevPage")
            {
                if(key == "_nextPage")
                    PageIndex.Value++;
                else
                    PageIndex.Value--;
                ShowResponsePage();
            }
            else
            {
                PagedResponses.Value.Clear();
                PageIndex.Value = 0;
                PagedQuestion.Value = null;
                if(key != "_cancel")
                    PagedResponseConfirmed.Value(who, key);
                PagedResponseConfirmed.Value = null;
            }
        }
        public static string GetStringID(this Item item) 
            => ModEntry.DGA?.GetDGAItemId(item) ?? item.ParentSheetIndex.ToString();
        public static bool TryLoadAsset<T>(IMonitor mon, IModHelper helper, string path, out T asset)
        {
            try
            {
                asset = helper.GameContent.Load<T>(path);
            } catch(ContentLoadException e)
            {
                mon.Log(ModEntry.i18n.Get("misc.assetLoadFailed", new { Path = path, Msg = e.Message }), LogLevel.Warn);
                asset = default;
                return false;
            }
            return true;
        }
        public static IEnumerable<Building> GetAllBuildings()
        {
            if (!Game1.hasLoadedGame || Game1.getFarm() is null)
                return Array.Empty<Building>();

            return Game1.getFarm().buildings;
        }
        public static bool RemoveNamedItemsFromInventory(this Farmer who, string what, int count)
        {
            List<Item> matched = new();
            int has = 0;
            foreach (var item in who.Items)
            {
                if (item.Name == what)
                {
                    matched.Add(item);
                    has += item.Stack;
                }
            }
            if (has < count)
                return false;
            for (int i = 0; i < matched.Count && count > 0; i++)
            {
                var item = matched[i];
                var s = item.Stack;
                if (count >= item.Stack)
                    who.removeItemFromInventory(item);
                else
                    item.Stack -= count;
                count -= s;
            }
            return true;
        }
        public static bool RemoveItemsFromInventory(this Farmer who, Item what, int count = -1)
        {
            if (count == -1)
                count = what.Stack;
            List<Item> matched = new();
            int has = 0;
            foreach (var item in who.Items) 
            {
                if (what.canStackWith(item) && item.canStackWith(what)) 
                {
                    matched.Add(item); 
                    has += item.Stack;
                }
            }
            if (has < count)
                return false;
            for(int i = 0; i < matched.Count && count > 0; i++)
            {
                var item = matched[i];
                var s = item.Stack;
                if (count >= item.Stack)
                    who.removeItemFromInventory(item);
                else
                    item.Stack -= count;
                count -= s;
            }
            return true;
        }
        public static bool HasItemNamed(this Farmer who, string what, int count)
        {
            what = what.Trim();
            int has = 0;
            foreach (var item in who.Items)
            {
                if (item.Name == what)
                    has += item.Stack;
                if (has >= count)
                    return true;
            }
            return false;
        }
        public static bool HasItem(this Farmer who, Item what, int count)
        {
            int has = 0;
            foreach (var item in who.Items)
            {
                if (what.canStackWith(item) && item.canStackWith(what))
                    has += item.Stack;
                if (has >= count)
                    return true;
            }
            return false;
        }
    }
}
