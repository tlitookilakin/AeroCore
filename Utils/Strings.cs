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

namespace AeroCore.Utils
{
    public static class Strings
    {
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
        public static bool ToVec2(this string[] strings, out Vector2 vec, int offset = 0)
        {
            if (offset + 1 >= strings.Length)
            {
                vec = new();
                return false;
            }
            return ToVec2(strings[offset], strings[offset + 1], out vec);
        }
        public static bool ToVec2(string x, string y, out Vector2 vec)
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
            if (int.TryParse(x, out int xx) && int.TryParse(y, out int yy) && int.TryParse(w, out int ww) && int.TryParse(h, out int hh))
            {
                rect = new(xx, yy, ww, hh);
                return true;
            }
            rect = new();
            return false;
        }
        public static string IterableToString(this IEnumerable<object> iter)
        {
            StringBuilder sb = new();
            sb.Append('[');
            foreach (object item in iter)
            {
                sb.Append(item.ToString());
                sb.Append(", ");
            }
            sb.Append(']');
            return sb.ToString();
        }
        public static string WithoutPath(this IAssetName name, string path)
        {
            if (!name.StartsWith(path, false))
                return null;

            int count = PathUtilities.GetSegments(path).Length;
            return string.Join(PathUtilities.PreferredAssetSeparator, PathUtilities.GetSegments(name.ToString()).Skip(count));
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
                            skipped++;
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
                                result.Add(prev[last..(i - skipped)].ToString());
                            last = i + 1;
                            skip = last;
                            skipped = 0;
                        }
                        break;
                }
            }
            s[skip..].CopyTo(prev.AsSpan(skip - skipped));
            if (s.Length - last - skipped > 0)
                result.Add(prev[last..^skipped].ToString());
            return result;
        }
        public static IEnumerable<string> SafeSplit(this string s, char delim, bool RemoveEmpty = false) => s.AsSpan().SafeSplit(delim, RemoveEmpty);

        /// <summary>Parses a color from a string. Valid formats: #rgb #rgba #rrggbb #rrggbbaa r,g,b r,g,b,a</summary>
        /// <param name="str">The string to parse from</param>
        /// <param name="color">The color parsed, if successful</param>
        /// <returns>Whether or not a color could be parsed from the string</returns>
        public static bool TryParseColor(string str, out Color color)
        {
            color = Color.Transparent;

            if (str is null || str.Length == 0)
                return false;

            if (str.ToLowerInvariant() == "black")
            {
                color = Color.Black;
                return true;
            }

            DColor c = DColor.FromName(str);
            if (c != DColor.Black)
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
