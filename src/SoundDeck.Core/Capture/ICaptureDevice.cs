namespace SoundDeck.Core.Capture
{
    using System;

    /// <summary>
    /// Provides information about a device capable of being captured.
    /// </summary>
    public interface ICaptureDevice : IDisposable
    {
        /// <summary>
        /// Gets the audio device identifier.
        /// </summary>
        string DeviceId { get; }
    }
}
