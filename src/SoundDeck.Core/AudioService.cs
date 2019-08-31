namespace SoundDeck.Core
{
    using System;
    using Microsoft.Extensions.Logging;
    using NAudio.CoreAudioApi;
    using NAudio.MediaFoundation;
    using NAudio.Wave;
    using SoundDeck.Core.Capture;
    using SoundDeck.Core.Capture.Sharing;
    using SoundDeck.Core.Playback;
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
            this.Devices = new AudioDeviceCollection();
            this.NormalizationProvider = new CachedNormalizationProvider();
            this.SharedAudioBufferManager = new SharedAudioBufferManager(logger);
        }

        /// <summary>
        /// Gets the audio devices.
        /// </summary>
        public IAudioDeviceCollection Devices { get; }

        /// <summary>
        /// Gets or sets the normalization provider.
        /// </summary>
        public INormalizationProvider NormalizationProvider { get; set; }

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
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
            => this.Devices.Dispose();

        /// <summary>
        /// Gets an audio buffer for the specified device identifier.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="clipDuration">Duration of the clip.</param>
        /// <returns>The audio buffer.</returns>
        public IAudioBuffer GetAudioBuffer(string deviceId, TimeSpan clipDuration)
            => this.SharedAudioBufferManager.GetOrAddAudioBuffer(deviceId, clipDuration);

        /// <summary>
        /// Gets an audio player for the specified device identifier.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>The audio player.</returns>
        public IAudioPlayer GetAudioPlayer(string deviceId)
            => new AudioPlayer(deviceId, this.NormalizationProvider);

        /// <summary>
        /// Gets an audio recorder, capable of capturing the audio from the specified device identifier.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>The audio recorder.</returns>
        public IAudioRecorder GetAudioRecorder(string deviceId)
            => new AudioRecorder(new MMDeviceEnumerator().GetDevice(deviceId));

        /// <summary>
        /// Gets the playlist player for the associated playlist player action type.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="action">The action type.</param>
        /// <param name="playlist">The playlist.</param>
        /// <returns>The playlist player.</returns>
        public IPlaylistPlayer GetPlaylistPlayer(string deviceId, PlaylistPlayerActionType action, Playlist playlist)
        {
            switch (action)
            {
                case PlaylistPlayerActionType.LoopStop:
                    return new LoopStopPlayer(deviceId, playlist, this.NormalizationProvider);

                case PlaylistPlayerActionType.PlayNext:
                    return new PlayNextPlayer(deviceId, playlist, this.NormalizationProvider);

                case PlaylistPlayerActionType.PlayStop:
                    return new PlayStopPlayer(deviceId, playlist, this.NormalizationProvider);
            }

            throw new NotSupportedException($"The provided playlist player action is not supported: {action}");
        }
    }
}
