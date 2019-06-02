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
        /// Saves the buffered audio data for the duration, to the specified output path.
        /// </summary>
        /// <param name="duration">The duration.</param>
        /// <param name="outputPath">The output path.</param>
        /// <returns>The file path.</returns>
        Task<string> SaveAsync(TimeSpan duration, string outputPath);
    }
}
