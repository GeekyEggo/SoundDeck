namespace SoundDeck.Core.Extensions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides extension methods for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Iterates over each item within the <paramref name="source"/> an executes the <paramref name="action"/>.
        /// </summary>
        /// <typeparam name="T">The type of items within the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
            => source.ForEach((item, _) => action(item));

        /// <summary>
        /// Iterates over each item within the <paramref name="source"/> an executes the <paramref name="action"/>.
        /// </summary>
        /// <typeparam name="T">The type of items within the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
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
