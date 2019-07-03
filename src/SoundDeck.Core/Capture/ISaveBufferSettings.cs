namespace SoundDeck.Core.Capture
{
    using System;

    /// <summary>
    /// Provides settings for saving the capture of an audio buffer.
    /// </summary>
    public interface ISaveBufferSettings : ISaveAudioSettings
    {
        /// <summary>
        /// Gets the duration.
        /// </summary>
        TimeSpan Duration { get; }
    }
}
