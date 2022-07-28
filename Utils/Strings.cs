using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using DColor = System.Drawing.Color;
using System.Linq;
using System.Text;
using System.Globalization;
using StardewModdingAPI.Utilities;
using StardewModdingAPI;
using SObject = StardewValley.Object;
using StardewValley.Objects;
using StardewValley.Tools;

namespace AeroCore.Utils
{
    public static class Strings
    {
        internal static string[] ObjectPrefixes = 
            { "(O)", "(BC)", "(F)", "(B)", "(FL)", "(WP)", "(W)", "(H)", "(S)", "(P)"}; //tool creation not supported

        public static bool ToPoint(this string[] strings, out Point point, int offset = 0)
        {
            if (offset + 1 >= strings.Length)
            {
                point = new();
                return false;
            }
            return ToPoint(strings[offset], strings[offset + 1], out point);
        }
        public static bool ToPoint(string x, string y, out Point point)
        {
            if (int.TryParse(x, out int xx) && int.TryParse(y, out int yy))
            {
                point = new(xx, yy);
                return true;
            }
            point = new();
            return false;
        }
        public static bool ToVector2(this string[] strings, out Vector2 vec, int offset = 0)
        {
            if (offset + 1 >= strings.Length)
            {
                vec = new();
                return false;
            }
            return ToVector2(strings[offset], strings[offset + 1], out vec);
        }
        public static bool ToVector2(string x, string y, out Vector2 vec)
        {
            if (float.TryParse(x, out float xx) && float.TryParse(y, out float yy))
            {
                vec = new(xx, yy);
                return true;
            }
            vec = new();
            return false;
        }
        public static bool ToRect(this string[] strings, out Rectangle rect, int offset = 0)
        {
            if (offset + 3 >= strings.Length)
            {
                rect = new();
                return false;
            }
            return ToRect(strings[offset], strings[offset + 1], strings[offset + 2], strings[offset + 3], out rect);
        }
        public static bool ToRect(string x, string y, string w, string h, out Rectangle rect)
        {
            if (int.TryParse(x, out int xx) && 
                int.TryParse(y, out int yy) && 
                int.TryParse(w, out int ww) && 
                int.TryParse(h, out int hh))
            {
                rect = new(xx, yy, ww, hh);
                return true;
            }
            rect = new();
            return false;
        }
        public static bool FromCorners(this string[] strings, out Rectangle rect, int offset = 0)
        {
            if (offset + 3 >= strings.Length)
            {
                rect = new();
                return false;
            }
            return FromCorners(strings[offset], strings[offset + 1], strings[offset + 2], strings[offset + 3], out rect);
        }
        public static bool FromCorners(string x1, string y1, string x2, string y2, out Rectangle rect)
        {
            if (int.TryParse(x1, out int ax) &&
                int.TryParse(y1, out int ay) &&
                int.TryParse(x2, out int bx) &&
                int.TryParse(y2, out int by))
            {
                rect = new(Math.Min(ax, bx), Math.Min(ax, bx), Math.Abs(ax - bx), Math.Abs(ay - by));
                return true;
            }
            rect = new();
            return false;
        }
        public static string GetChunk(this string str, char delim, int which)
        {
            int i = 0;
            int n = 0;
            int z = 0;
            while(i < str.Length)
            {
                if (str[i] == delim)
                {
                    if (n == which)
                        return str[z..i];
                    n++;
                    z = i + 1;
                }
                i++;
            }
            if (n == which)
                return str[z..i];
            return "";
        }
        public static string ContentsToString(this IEnumerable<object> iter, string separator = ", ")
        {
            StringBuilder sb = new();
            foreach (object item in iter)
            {
                sb.Append(item.ToString());
                sb.Append(separator);
            }
            return sb.ToString();
        }
        public static string WithoutPath(this IAssetName name, string path)
        {
            if (!name.StartsWith(path, false))
                return null;

            if (name.IsEquivalentTo(path))
                return string.Empty;

            int count = PathUtilities.GetSegments(path).Length;
            return string.Join(PathUtilities.PreferredAssetSeparator, PathUtilities.GetSegments(name.ToString())[count..]);
        }
        public static bool TryGetItem(this string str, out Item item, Color? color = null)
        {
            str = str.Trim();
            item = ModEntry.DGA?.SpawnDGAItem(str, color) as Item;
            if (item is not null)
                return true;
            int id, i;
            if ((id = ModEntry.JA?.GetObjectId(str) ?? -1) != -1)
                i = 0;
            else if ((id = ModEntry.JA?.GetBigCraftableId(str) ?? -1) != -1)
                i = 1;
            else if ((id = ModEntry.JA?.GetWeaponId(str) ?? -1) != -1)
                i = 6;
            else if ((id = ModEntry.JA?.GetHatId(str) ?? -1) != -1)
                i = 7;
            else if ((id = ModEntry.JA?.GetClothingId(str) ?? -1) != -1)
                i = 8;
            else
            {
                i = 0;
                while (i < ObjectPrefixes.Length)
                    if (str.StartsWith(ObjectPrefixes[i]))
                        break;
                    else
                        i++;
                int clip = 0;
                if(i >= ObjectPrefixes.Length)
                    i = 0;
                else
                    clip = ObjectPrefixes[i].Length;
                if (!int.TryParse(str[clip..], out id))
                    return false;
            }
            switch (i)
            {
                case 0: item = new SObject(id, 1); return true; 
                case 1: item = new SObject(Vector2.Zero, id); return true;
                case 2: item = new Furniture(id, Vector2.Zero); return true;
                case 3: item = new Boots(id); return true;
                case 4: item = new Wallpaper(id, true); return true;
                case 5: item = new Wallpaper(id, false); return true;
                case 6: item = new MeleeWeapon(id); return true;
                case 7: item = new Hat(id); return true;
                case 8 or 9: item = new Clothing(id); return true;
            }
            item = null;
            return false;
        }
        /// <returns>A copy of the string with all whitespace stripped</returns>
        public static string Collapse(this string str)
        {
            var s = str.AsSpan();
            var r = new Span<char>(new char[s.Length]);
            int len = 0;
            int last = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (!char.IsWhiteSpace(s[i]))
                    continue;

                if (i - last <= 1)
                {
                    last = i + 1;
                    continue;
                }

                s[last..i].CopyTo(r[len..]);
                len += i - last;
                last = i + 1;
            }
            if (last < s.Length)
            {
                s[last..].CopyTo(r[len..]);
                len += s.Length - last;
            }
            return new string(r[..len]);
        }
        public static IList<string> SafeSplit(this ReadOnlySpan<char> s, char delim, bool RemoveEmpty = false)
        {
            List<string> result = new();
            bool dquote = false;
            bool squote = false;
            bool escaped = false;
            char c;
            int last = 0;
            var prev = new char[s.Length];
            int skip = 0;
            int skipped = 0;
            for(int i = 0; i < s.Length; i++)
            {
                if (escaped)
                {
                    escaped = false;
                    continue;
                }
                c = s[i];
                switch (c)
                {
                    case '"':
                        if (!squote)
                        {
                            dquote = !dquote;
                            s[skip..i].CopyTo(prev.AsSpan(skip - skipped));
                            skipped++;
                            skip = i + 1;
                        }
                        break;
                    case '\'':
                        if (!dquote)
                        {
                            squote = !squote;
                            s[skip..i].CopyTo(prev.AsSpan(skip - skipped));
                            skipped++;
                            skip = i + 1;
                        }
                        break;
                    case '\\':
                        escaped = true;
                        s[skip..i].CopyTo(prev.AsSpan(skip - skipped));
                        skipped++;
                        skip = i + 1;
                        break;
                    default:
                        if (c == delim && !dquote && !squote)
                        {
                            s[skip..i].CopyTo(prev.AsSpan(skip - skipped));
                            if (!RemoveEmpty || i - last - skipped > 0)
                                result.Add(new string(prev[last..(i - skipped)]));
                            last = i + 1;
                            skip = last;
                            skipped = 0;
                        }
                        break;
                }
            }
            s[skip..].CopyTo(prev.AsSpan(skip - skipped));
            if (s.Length - last - skipped > 0)
                result.Add(new string(prev[last..^skipped]));
            return result;
        }
        public static IList<string> SafeSplit(this string s, char delim, bool RemoveEmpty = false) => s.AsSpan().SafeSplit(delim, RemoveEmpty);

        /// <summary>Parses a color from a string. Valid formats: #rgb #rgba #rrggbb #rrggbbaa r,g,b r,g,b,a</summary>
        /// <param name="str">The string to parse from</param>
        /// <param name="color">The color parsed, if successful</param>
        /// <returns>Whether or not a color could be parsed from the string</returns>
        public static bool TryParseColor(this string str, out Color color)
        {
            color = Color.Transparent;

            if (str is null || str.Length == 0)
                return false;

            DColor c = DColor.FromName(str);
            if (c.ToArgb() != 0)
            {
                color = new(c.R, c.G, c.B, c.A);
                return true;
            }

            ReadOnlySpan<char> s = str.AsSpan();
            if (s[0] == '#')
            {
                if (s.Length <= 3)
                    return false;

                if (s.Length > 6)
                {
                    if (int.TryParse(s[1..3], NumberStyles.HexNumber, null, out int r) &&
                        int.TryParse(s[3..5], NumberStyles.HexNumber, null, out int g) &&
                        int.TryParse(s[5..7], NumberStyles.HexNumber, null, out int b))
                    {
                        if (s.Length > 8 && int.TryParse(s[7..9], NumberStyles.HexNumber, null, out int a))
                            color = new(r, g, b, a);
                        else
                            color = new(r, g, b);
                        return true;
                    }
                } else
                {
                    if (int.TryParse(s[1..2], NumberStyles.HexNumber, null, out int r) &&
                        int.TryParse(s[2..3], NumberStyles.HexNumber, null, out int g) &&
                        int.TryParse(s[3..4], NumberStyles.HexNumber, null, out int b))
                    {
                        if (s.Length > 4 && int.TryParse(s[4..5], NumberStyles.HexNumber, null, out int a))
                            color = new(r, g, b, a);
                        else
                            color = new(r, g, b);
                        return true;
                    }
                }
            }
            else
            {
                string[] vals = str.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (vals.Length > 2)
                {
                    if(int.TryParse(vals[0], out int r) && 
                        int.TryParse(vals[1], out int g) && 
                        int.TryParse(vals[2], out int b))
                    {
                        if(vals.Length > 3 && int.TryParse(vals[3], out int a))
                            color = new Color(r, g, b, a);
                        else
                            color = new Color(r, g, b);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
