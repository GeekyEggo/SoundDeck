namespace SoundDeck.Core
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides an audio buffer, used to save captured audio data.
    /// </summary>
    public interface IAudioBuffer : IDisposable
    {
        /// <summary>
        /// Gets or sets the duration of the buffer.
        /// </summary>
        TimeSpan BufferDuration { get; set; }

        /// <summary>
        /// Gets the audio device identifier.
        /// </summary>
        string DeviceId { get; }

        /// <summary>
        /// Saves an audio file of the current buffer.
        /// </summary>
        /// <param name="settings">The settings containing information about how, and where to save the capture.</param>
        /// <returns>The file path.</returns>
        Task<string> SaveAsync(ISaveBufferSettings settings);
    }
}
