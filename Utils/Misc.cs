using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
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
        public static Point LocalToGlobal(int x, int y)
        {
            return new(x + Game1.viewport.X, y + Game1.viewport.Y);
        }
        public static Point LocalToGlobal(Point pos)
        {
            return LocalToGlobal(pos.X, pos.Y);
        }
        public static IEnumerable<Point> pointsIn(this Rectangle rect)
        {
            for (int x = 0; x < rect.Width; x++)
                for (int y = 0; y < rect.Height; y++)
                    yield return new Point(x + rect.X, y + rect.Y);
        }
        public static IList<Point> allPointsIn(this Rectangle rect)
        {
            var points = new Point[rect.Width * rect.Height];
            for (int x = 0; x < rect.Width; x++)
                for (int y = 0; y < rect.Height; y++)
                    points[x + y * rect.Width] = new(x + rect.X, y + rect.Y);
            return points;
        }
        public static bool IsFestivalAtLocation(string Location)
        {
            return Location is not null && Game1.weatherIcon == 1 && Game1.whereIsTodaysFest.ToLowerInvariant() == Location.ToLowerInvariant();
        }
        public static bool IsFestivalReady()
        {
            if (Game1.weatherIcon != 1)
                return true;

            return !int.TryParse(
                ModEntry.helper.GameContent.Load<Dictionary<string, string>>(
                    "Data/Festivals/" + Game1.currentSeason + Game1.dayOfMonth)["conditions"].Split('/')[1].Split(' ')[0],
                    out int time) || time <= Game1.timeOfDay;
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

            PagedResponses.Value.AddRange(responses);
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
                visible.Add(new("_nextPage", '>' + ModEntry.i18n.Get("misc.generic.next")));

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
                PagedResponseConfirmed.Value = null;
                PagedQuestion.Value = null;
                if(key != "_cancel")
                    PagedResponseConfirmed.Value(who, key);
            }
        }
    }
}
