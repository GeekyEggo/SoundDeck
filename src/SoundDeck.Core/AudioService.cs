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
        /// Gets or sets the players.
        /// </summary>
        private AudioPlayerCollection Players { get; } = new AudioPlayerCollection();

        /// <summary>
        /// Sets the logger factory.
        /// </summary>
        private ILoggerFactory LoggerFactory { get; }

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

        /// <inheritdoc/>
        public IPlaylistController CreatePlaylistController(string deviceKey, ControllerActionType action)
        {
            var audioPlayer = this.GetAudioPlayer(deviceKey);

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
        public IAudioPlayer GetAudioPlayer(string deviceKey)
        {
            var player = new AudioPlayer(
                AudioDevices.Current.GetDeviceByKey(deviceKey),
                this.LoggerFactory.CreateLogger<AudioPlayer>());

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
