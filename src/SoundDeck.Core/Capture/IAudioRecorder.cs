namespace SoundDeck.Core.Capture
{
    using System.Threading.Tasks;

    /// <summary>
    /// Provides methods for capturing audio.
    /// </summary>
    public interface IAudioRecorder : ICaptureDevice
    {
        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        ISaveAudioSettings Settings { get; set; }

        /// <summary>
        /// Starts capturing audio asynchronously.
        /// </summary>
        /// <returns>The task of starting.</returns>
        Task StartAsync();

        /// <summary>
        /// Stops capturing audio asynchronously.
        /// </summary>
        /// <returns>The task of starting.</returns>
        Task StopAsync();
    }
}
