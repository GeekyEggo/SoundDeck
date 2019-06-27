namespace SoundDeck.Core.Capture
{
    /// <summary>
    /// Provides methods for capturing audio.
    /// </summary>
    public interface IAudioRecorder : ICaptureDevice
    {
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
