using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AeroCore.Utils
{
    public static class Misc
    {
        public static IEnumerable<T> Take<T>(this IEnumerator<T> source, int count)
        {
            while(count > 0 && source.MoveNext())
            {
                yield return source.Current;
                count--;
            }
        }
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
            {
                for (int y = 0; y < rect.Height; y++)
                {
                    yield return new Point(x + rect.X, y + rect.Y);
                }
            }
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
        public static bool TryGetNext<T>(this IEnumerator<T> e, out T result)
        {
            if (e.MoveNext())
            {
                result = e.Current;
                return true;
            }
            result = default;
            return false;
        }
        public static T GetNext<T>(this IEnumerator<T> e)
        {
            if(e.MoveNext())
                return e.Current;
            else
                throw new IndexOutOfRangeException();
        }
    }
}
