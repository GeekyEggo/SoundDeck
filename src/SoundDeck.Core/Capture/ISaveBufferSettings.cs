namespace SoundDeck.Core.Capture
{
    using System;

    /// <summary>
    /// Provides settings for saving the capture of an audio buffer.
    /// </summary>
    public interface ISaveBufferSettings
    {
        /// <summary>
        /// Gets the duration.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Gets a value indicating whether to encode the audio buffer to an MP3.
        /// </summary>
        bool EncodeToMP3 { get; }

        /// <summary>
        /// Gets a value indicating whether to normalize the volume of the buffer.
        /// </summary>
        bool NormalizeVolume { get; }

        /// <summary>
        /// Gets the output path, directory, where the file should be saved to.
        /// </summary>
        string OutputPath { get; }
    }
}
