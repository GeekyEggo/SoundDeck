namespace SoundDeck.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides a service for interacting with local audio devices.
    /// </summary>
    public interface IAudioService : IDisposable
    {
        /// <summary>
        /// Determines whether encoding to MP3 is possible based on the current environment.
        /// </summary>
        /// <returns><c>true</c> when encoding is possible; otherwise <c>false</c>.</returns>
        bool CanEncodeToMP3();

        /// <summary>
        /// Attempts to get the audio buffer for the specified device identifier.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>The audio buffer.</returns>
        IAudioBuffer GetBuffer(string deviceId);

        /// <summary>
        /// Gets the active audio devices.
        /// </summary>
        /// <returns>The audio devices</returns>
        IEnumerable<AudioDevice> GetDevices();
    }
}
