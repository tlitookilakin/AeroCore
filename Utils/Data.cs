using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AeroCore.Utils
{
    public static class Data
    {
        public static IEnumerable<T> Take<T>(this IEnumerator<T> source, int count)
        {
            while (count > 0 && source.MoveNext())
            {
                yield return source.Current;
                count--;
            }
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
            if (e.MoveNext())
                return e.Current;
            else
                throw new IndexOutOfRangeException();
        }
        public static void DisposeAll(this IList<IDisposable> items)
        {
            for (int i = 0; i < items.Count; i++)
                items[i].Dispose();
        }
        public static void DisposeAll<T>(this IList<T> items)
        {
            for (int i = 0; i < items.Count; i++)
                if(items[i] is IDisposable d)
                    d.Dispose();
        }
        public static IList<T> TransformItems<T>(this IList<T> list, Func<T, T> transformer)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = transformer(list[i]);
            }
            return list;
        }
        public static V GetOrAdd<K, V>(this IDictionary<K, V> dict, K key, Func<V> add)
        {
            if (dict.TryGetValue(key, out V v))
                return v;
            v = add();
            dict.Add(key, v);
            return v;
        }
        public static V GetOrAdd<K, V>(this IDictionary<K, V> dict, K key, V add)
        {
            if (dict.TryGetValue(key, out V v))
                return v;
            dict.Add(key, v);
            return add;
        }

        public static void Clear(this Array arr)
            => Array.Clear(arr, 0, arr.Length);

        public static Vector2 DirLength(float angle, float length)
            => new(MathF.Cos(angle) * length, MathF.Sin(angle) * length);
        public static float DegToRad(float angle)
            => angle / 360F * MathF.Tau;
        public static double DegToRad(double angle)
            => angle / 360F * Math.Tau;
        public static Vector2 Rotate(this Vector2 vec, float angle)
        {
            vec.X = vec.X * MathF.Cos(angle) - vec.Y * MathF.Sin(angle);
            vec.Y = vec.X * MathF.Sin(angle) + vec.Y * MathF.Cos(angle);
            return vec;
        }
    }
}
