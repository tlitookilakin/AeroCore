using System;
using System.Collections;
using System.Collections.Generic;

namespace AeroCore.Generics
{
    public class BufferedEnumerator<T> : IEnumerator<T>
    {
        private readonly Queue<T> buffer = new();
        private readonly IEnumerator<T> basis;
        private T current = default;
        object IEnumerator.Current => current;
        public T Current => current;
        public int Count => buffer.Count;

        public BufferedEnumerator(IEnumerator<T> Base)
        {
            basis = Base;
        }
        public void Dispose() => basis.Dispose();
        public bool MoveNext()
        {
            if (buffer.Count > 0)
            {
                current = buffer.Dequeue();
                return true;
            }
            bool b = basis.MoveNext();
            current = b ? basis.Current : default;
            return b;
        }
        public void Reset()
        {
            buffer.Clear(); //no way to maintain position
            basis.Reset();
            current = basis.Current;
        }
        public IList<T> GetBuffer() => buffer.ToArray();
        public void Push(T item) => buffer.Enqueue(item);
        public void Clear() => buffer.Clear();
    }
}
