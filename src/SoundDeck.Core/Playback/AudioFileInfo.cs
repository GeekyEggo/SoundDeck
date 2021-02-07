namespace SoundDeck.Core.Playback
{
    /// <summary>
    /// Provides information about an audio file.
    /// </summary>
    public class AudioFileInfo
    {
        /// <summary>
        /// Gets or sets the path of the file.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the volume the file should be played at.
        /// </summary>
        public float Volume { get; set; } = 0.75F;

        /// <summary>
        /// Creates a new <see cref="AudioFileInfo"/> from the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The audio file info.</returns>
        public static AudioFileInfo FromPath(string path)
        {
            return new AudioFileInfo
            {
                Path = path
            };
        }
    }
}
