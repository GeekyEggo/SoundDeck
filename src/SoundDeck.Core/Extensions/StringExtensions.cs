namespace SoundDeck.Core.Extensions
{
    using System;

    /// <summary>
    /// Provides extension methods for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a value indicating whether a specified substring occurs within this string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="stringComparison">The string comparison to be applied when matching.</param>
        /// <returns><c>true</c> when this instance contains <see cref="value"/>.</returns>
        public static bool Contains(this string source, string value, StringComparison stringComparison)
        {
            return value != null
                && source?.IndexOf(value, stringComparison) >= 0;
        }

        /// <summary>
        /// Trims <paramref name="value"/> from the end of this instance; matching is based on <paramref name="stringComparison"/>.
        /// </summary>
        /// <param name="source">The source; this instance.</param>
        /// <param name="value">The value to match.</param>
        /// <param name="stringComparison">The string comparison to be applied when matching.</param>
        /// <returns>This instance, with the <paramref name="value"/> trimmed when matched.</returns>
        public static string TrimEnd(this string source, string value, StringComparison stringComparison)
        {
            return source.EndsWith(value, stringComparison)
                ? source.Substring(0, source.Length - value.Length)
                : source;
        }
    }
}
