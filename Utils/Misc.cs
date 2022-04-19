using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
