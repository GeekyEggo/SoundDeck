namespace SoundDeck.Core
{
    using System;

    /// <summary>
    /// Provides a configurable <see cref="IAudioBuffer"/>.
    /// </summary>
    public interface IConfigurableAudioBuffer : IAudioBuffer, IDisposable
    {
        /// <summary>
        /// Gets or sets the duration of the buffer.
        /// </summary>
        TimeSpan BufferDuration { get; set; }
    }
}
