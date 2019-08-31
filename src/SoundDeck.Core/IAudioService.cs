namespace SoundDeck.Core
{
    using System;
    using SoundDeck.Core.Capture;
    using SoundDeck.Core.Playback;

    /// <summary>
    /// Provides a service for interacting with local audio devices.
    /// </summary>
    public interface IAudioService : IDisposable
    {
        /// <summary>
        /// Gets the audio devices.
        /// </summary>
        IAudioDeviceCollection Devices { get; }

        /// <summary>
        /// Determines whether encoding to MP3 is possible based on the current environment.
        /// </summary>
        /// <returns><c>true</c> when encoding is possible; otherwise <c>false</c>.</returns>
        bool CanEncodeToMP3();

        /// <summary>
        /// Gets an audio buffer for the specified device identifier.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="clipDuration">Duration of the clip.</param>
        /// <returns>The audio buffer.</returns>
        IAudioBuffer GetAudioBuffer(string deviceId, TimeSpan clipDuration);

        /// <summary>
        /// Gets an audio player for the specified device identifier.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>The audio player.</returns>
        IAudioPlayer GetAudioPlayer(string deviceId);

        /// <summary>
        /// Gets an audio recorder, capable of capturing the audio from the specified device identifier.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>The audio recorder.</returns>
        IAudioRecorder GetAudioRecorder(string deviceId);

        /// <summary>
        /// Gets the playlist player for the associated playlist player action type.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="action">The action type.</param>
        /// <param name="playlist">The playlist.</param>
        /// <returns>The playlist player.</returns>
        IPlaylistPlayer GetPlaylistPlayer(string deviceId, PlaylistPlayerActionType action, Playlist playlist);
    }
}
