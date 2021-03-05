namespace SoundDeck.Core
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using NAudio.MediaFoundation;
    using NAudio.Wave;
    using SoundDeck.Core.Capture;
    using SoundDeck.Core.Capture.Sharing;
    using SoundDeck.Core.Playback;
    using SoundDeck.Core.Playback.Controllers;
    using SoundDeck.Core.Playback.Players;

    /// <summary>
    /// Provides a service for interacting with local audio devices.
    /// </summary>
    public sealed class AudioService : IAudioService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="normalizationProvider">The normalization provider.</param>
        public AudioService(ILogger<AudioService> logger)
        {
            this.SharedAudioBufferManager = new SharedAudioBufferManager(logger);
        }

        /// <summary>
        /// Gets the audio devices.
        /// </summary>
        public IReadOnlyCollection<AudioDevice> Devices => AudioDevices.Current;

        /// <summary>
        /// Gets or sets the players.
        /// </summary>
        private AudioPlayerCollection Players { get; } = new AudioPlayerCollection();

        /// <summary>
        /// Gets the audio buffer manager.
        /// </summary>
        private SharedAudioBufferManager SharedAudioBufferManager { get; }

        /// <summary>
        /// Determines whether encoding to MP3 is possible based on the current environment.
        /// </summary>
        /// <returns><c>true</c> when encoding is possible; otherwise <c>false</c>.</returns>
        public bool CanEncodeToMP3()
            => MediaFoundationEncoder.SelectMediaType(AudioSubtypes.MFAudioFormat_MP3, Constants.DefaultWaveFormat, Constants.DesiredBitRate) != null;

        /// <summary>
        /// Creates a playlist controller with an audio player for the specified <paramref name="deviceId"/>.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="action">The action type.</param>
        /// <param name="playlist">The playlist.</param>
        /// <returns>The playlist player.</returns>
        public IPlaylistController CreatePlaylistController(string deviceId, ControllerActionType action)
        {
            var audioPlayer = this.GetAudioPlayer(deviceId);

            switch (action)
            {
                case ControllerActionType.LoopStop:
                    return new PlayStopController(audioPlayer, action, ContinuousPlaybackType.SingleLoop);

                case ControllerActionType.LoopAllStop:
                    return new PlayStopController(audioPlayer, action, ContinuousPlaybackType.ContiunousLoop);

                case ControllerActionType.LoopAllStopReset:
                    return new PlayStopResetController(audioPlayer, action, ContinuousPlaybackType.ContiunousLoop);

                case ControllerActionType.PlayNext:
                    return new PlayNextController(audioPlayer);

                case ControllerActionType.PlayStop:
                    return new PlayStopController(audioPlayer, action, ContinuousPlaybackType.Single);

                case ControllerActionType.PlayAllStop:
                    return new PlayStopController(audioPlayer, action, ContinuousPlaybackType.Continuous);
            }

            throw new NotSupportedException($"The provided playlist player action is not supported: {action}");
        }

        /// <summary>
        /// Gets an audio buffer for the specified device identifier.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="clipDuration">Duration of the clip.</param>
        /// <returns>The audio buffer.</returns>
        public IAudioBuffer GetAudioBuffer(string deviceId, TimeSpan clipDuration)
            => this.SharedAudioBufferManager.GetOrAddAudioBuffer(deviceId, clipDuration);

        /// <summary>
        /// Gets all active audio buffers.
        /// </summary>
        /// <returns>The audio buffers.</returns>
        public IEnumerable<IAudioBuffer> GetAudioBuffers()
            => this.SharedAudioBufferManager.GetAudioBuffers();

        /// <summary>
        /// Gets an audio recorder, capable of capturing the audio from the specified device identifier.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>The audio recorder.</returns>
        public IAudioRecorder GetAudioRecorder(string deviceId)
            => new AudioRecorder(deviceId);

        /// <summary>
        /// Gets the audio player for the specified <paramref name="deviceId"/>.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>The audio player.</returns>
        public IAudioPlayer GetAudioPlayer(string deviceId)
        {
            var player = new AudioPlayer(deviceId);
            this.Players.Add(player);

            return player;
        }

        /// <summary>
        /// Stops all <see cref="IAudioPlayer"/> associated with this instance.
        /// </summary>
        public void StopAll()
            => this.Players.StopAll();
    }
}
