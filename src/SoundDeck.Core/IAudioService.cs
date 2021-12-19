namespace SoundDeck.Core
{
    using System;
    using System.Collections.Generic;
    using SoundDeck.Core.Capture;
    using SoundDeck.Core.Playback;

    /// <summary>
    /// Provides a service for interacting with local audio devices.
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// Gets the audio devices.
        /// </summary>
        IReadOnlyCollection<IAudioDevice> Devices { get; }

        /// <summary>
        /// Gets an audio buffer for the specified device key.
        /// </summary>
        /// <param name="deviceId">The device key.</param>
        /// <param name="clipDuration">Duration of the clip.</param>
        /// <returns>The audio buffer.</returns>
        IAudioBuffer GetAudioBuffer(string deviceKey, TimeSpan clipDuration);

        /// <summary>
        /// Gets all active audio buffers.
        /// </summary>
        /// <returns>The audio buffers.</returns>
        IEnumerable<IAudioBuffer> GetAudioBuffers();

        /// <summary>
        /// Gets an audio recorder, capable of capturing the audio from the specified device key.
        /// </summary>
        /// <param name="deviceKey">The device key.</param>
        /// <returns>The audio recorder.</returns>
        IAudioRecorder GetAudioRecorder(string deviceKey);

        /// <summary>
        /// Gets the audio player for the specified <paramref name="deviceKey"/>.
        /// </summary>
        /// <param name="deviceId">The device key.</param>
        /// <returns>The audio player.</returns>
        IAudioPlayer GetAudioPlayer(string deviceKey);

        /// <summary>
        /// Gets a playlist controller with an audio player for the specified <paramref name="deviceKey"/>.
        /// </summary>
        /// <param name="deviceKey">The device key.</param>
        /// <param name="action">The action type.</param>
        /// <param name="playlist">The playlist.</param>
        /// <returns>The playlist player.</returns>
        IPlaylistController GetPlaylistController(string deviceKey, ControllerActionType action);

        /// <summary>
        /// Stops all <see cref="IAudioPlayer"/> associated with this instance.
        /// </summary>
        void StopAll();
    }
}
