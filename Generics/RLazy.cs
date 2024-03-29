﻿using System;

namespace AeroCore.Generics
{
    public class RLazy<T>
    {
        private Func<T> generator;
        private T cached;
        private bool isCached = false;
        public T Value => GetValue();
        public bool IsCached => isCached;
        public RLazy(Func<T> Generator)
        {
            generator = Generator;
        }
        public T GetValue()
        {
            if (!isCached)
            {
                cached = generator();
                isCached = true;
            }
            return cached;
        }
        public void Reset()
        {
            cached = default;
            isCached = false;
        }
    }
}
