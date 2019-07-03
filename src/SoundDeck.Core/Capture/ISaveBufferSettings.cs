namespace SoundDeck.Core.Capture
{
    using SoundDeck.Core.IO;
    using System;

    /// <summary>
    /// Provides settings for saving the capture of an audio buffer.
    /// </summary>
    public interface ISaveBufferSettings : IAudioFileWriterSettings
    {
        /// <summary>
        /// Gets the duration.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Gets the output path, directory, where the file should be saved to.
        /// </summary>
        string OutputPath { get; }
    }
}
