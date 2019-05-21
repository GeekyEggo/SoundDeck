using System;
using System.Collections.Generic;

namespace SoundDeck.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
            => source.ForEach((x, _) => action(x));

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            int i = 0;
            foreach (var item in source)
            {
                action(item, i);
                i++;
            }
        }
    }
}
