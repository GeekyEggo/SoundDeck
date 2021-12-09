namespace SoundDeck.Core
{
    using SoundDeck.Core.Capture;
    using SoundDeck.Core.Playback;
    using System;
    using System.Collections.Generic;

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
        /// Determines whether encoding to MP3 is possible based on the current environment.
        /// </summary>
        /// <returns><c>true</c> when encoding is possible; otherwise <c>false</c>.</returns>
        bool CanEncodeToMP3();

        /// <summary>
        /// Creates a playlist controller with an audio player for the specified <paramref name="deviceKey"/>.
        /// </summary>
        /// <param name="deviceKey">The device key.</param>
        /// <param name="action">The action type.</param>
        /// <param name="playlist">The playlist.</param>
        /// <returns>The playlist player.</returns>
        IPlaylistController CreatePlaylistController(string deviceKey, ControllerActionType action);

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
        /// Stops all <see cref="IAudioPlayer"/> associated with this instance.
        /// </summary>
        void StopAll();
    }
}
