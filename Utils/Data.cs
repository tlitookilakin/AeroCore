using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
