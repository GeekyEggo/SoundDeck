namespace SoundDeck.Core
{
    using SoundDeck.Core.Capture;
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
        /// Gets the active audio devices.
        /// </summary>
        /// <returns>The audio devices</returns>
        IEnumerable<AudioDevice> GetDevices();

        /// <summary>
        /// Registers a new audio buffer listener.
        /// </summary>
        /// <param name="deviceId">The audio device identifier.</param>
        /// <param name="clipDuration">The clip duration for the buffer.</param>
        /// <returns>The registration.</returns>
        AudioBufferRegistration RegisterBufferListener(string deviceId, TimeSpan clipDuration);

        /// <summary>
        /// Unregisters the audio buffer listener.
        /// </summary>
        /// <param name="registration">The registration.</param>
        void UnregisterBufferListener(AudioBufferRegistration registration);
    }
}
