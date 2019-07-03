namespace SoundDeck.Core.IO
{
    /// <summary>
    /// Provides settings for writing an audio file.
    /// </summary>
    public interface IAudioFileWriterSettings
    {
        /// <summary>
        /// Gets a value indicating whether to encode the audio buffer to an MP3.
        /// </summary>
        bool EncodeToMP3 { get; }

        /// <summary>
        /// Gets a value indicating whether to normalize the volume of the buffer.
        /// </summary>
        bool NormalizeVolume { get; }
    }
}
