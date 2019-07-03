namespace SoundDeck.Core.Capture
{
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
        /// Starts capturing audio.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops capturing audio.
        /// </summary>
        void Stop();
    }
}
