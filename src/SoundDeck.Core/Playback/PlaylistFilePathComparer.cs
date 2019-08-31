namespace SoundDeck.Core.Playback
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides an <see cref="IEqualityComparer{PlaylistFile}"/> that compares based on their <see cref="PlaylistFile.Path"/>.
    /// </summary>
    public class PlaylistFilePathComparer : IEqualityComparer<PlaylistFile>
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns><c>true</c> if the specified objects are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(PlaylistFile x, PlaylistFile y)
            => x?.Path?.Equals(y?.Path) == true;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. </returns>
        public int GetHashCode(PlaylistFile obj)
            => obj.Path.GetHashCode();
    }
}
