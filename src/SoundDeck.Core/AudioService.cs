namespace SoundDeck.Core
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
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
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="normalizationProvider">The normalization provider.</param>
        public AudioService(ILoggerFactory loggerFactory)
        {
            this.LoggerFactory = loggerFactory;
            this.SharedAudioBufferManager = new SharedAudioBufferManager(loggerFactory);
        }

        /// <summary>
        /// Gets the audio devices.
        /// </summary>
        public IReadOnlyCollection<IAudioDevice> Devices => AudioDevices.Current;

        /// <summary>
        /// Gets the playback manager.
        /// </summary>
        private AudioPlaybackManager Playback { get; } = new AudioPlaybackManager();

        /// <summary>
        /// Sets the logger factory.
        /// </summary>
        private ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Gets the audio buffer manager.
        /// </summary>
        private SharedAudioBufferManager SharedAudioBufferManager { get; }

        /// <inheritdoc/>
        public IAudioPlayer GetAudioPlayer(string deviceKey)
        {
            var player = this.CreateAudioPlayer(deviceKey);

            this.Playback.Add(player);
            return player;
        }

        /// <inheritdoc/>
        public IAudioBuffer GetAudioBuffer(string deviceKey, TimeSpan clipDuration)
            => this.SharedAudioBufferManager.GetOrAddAudioBuffer(deviceKey, clipDuration);

        /// <inheritdoc/>
        public IEnumerable<IAudioBuffer> GetAudioBuffers()
            => this.SharedAudioBufferManager.GetAudioBuffers();

        /// <inheritdoc/>
        public IAudioRecorder GetAudioRecorder(string deviceKey)
        {
            var device = AudioDevices.Current.GetDeviceByKey(deviceKey);
            return new AudioRecorder(device, this.LoggerFactory.CreateLogger<AudioRecorder>());
        }

        /// <inheritdoc/>
        public IPlaylistController GetPlaylistController(string deviceKey, ControllerActionType action)
        {
            var playlist = this.CreatePlaylistController(this.CreateAudioPlayer(deviceKey), action);

            this.Playback.Add(playlist);
            return playlist;
        }

        /// <summary>
        /// Stops all <see cref="IAudioPlayer"/> associated with this instance.
        /// </summary>
        public void StopAll()
            => this.Playback.StopAll();

        /// <summary>
        /// Creates a new audio player for the specified <paramref name="deviceKey" />.
        /// </summary>
        /// <param name="deviceKey">The audio device key.</param>
        /// <returns>The audio player.</returns>
        private IAudioPlayer CreateAudioPlayer(string deviceKey)
        {
            return new AudioPlayer(
                AudioDevices.Current.GetDeviceByKey(deviceKey),
                this.LoggerFactory.CreateLogger<IAudioPlayer>());
        }

        /// <summary>
        /// Creates a new <see cref="IPlaylistController"/>.
        /// </summary>
        /// <param name="audioPlayer">The audio player.</param>
        /// <param name="action">The action of the controller.</param>
        /// <returns>The playlist controller.</returns>
        private IPlaylistController CreatePlaylistController(IAudioPlayer audioPlayer, ControllerActionType action)
        {
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

                case ControllerActionType.PlayOverlap:
                    return new PlayOverlapController(audioPlayer);
            }

            throw new NotSupportedException($"The provided playlist player action is not supported: {action}");
        }
    }
}
