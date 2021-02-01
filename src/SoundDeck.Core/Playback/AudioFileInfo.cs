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
    }
}
