namespace SoundDeck.Core
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using NAudio.MediaFoundation;
    using NAudio.Wave;
    using SoundDeck.Core.Capture;
    using SoundDeck.Core.Capture.Sharing;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Core.Playback;
    using SoundDeck.Core.Playback.Players;
    using SoundDeck.Core.Playback.Volume;

    /// <summary>
    /// Provides a service for interacting with local audio devices.
    /// </summary>
    public sealed class AudioService : IAudioService
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly object _syncRoot = new object();

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
        /// Gets or sets the players.
        /// </summary>
        private IList<IAudioPlayer> Players { get; } = new List<IAudioPlayer>();

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
        /// Gets all active audio buffers.
        /// </summary>
        /// <returns>The audio buffers.</returns>
        public IEnumerable<IAudioBuffer> GetAudioBuffers()
            => this.SharedAudioBufferManager.GetAudioBuffers();

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
            => new AudioRecorder(deviceId);

        /// <summary>
        /// Gets the playlist player for the associated playlist player action type.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="action">The action type.</param>
        /// <param name="playlist">The playlist.</param>
        /// <returns>The playlist player.</returns>
        public IPlaylistPlayer GetPlaylistPlayer(string deviceId, PlaylistPlayerActionType action, IPlaylist playlist)
        {
            var player = this.GetPlaylistPlayerInternal(deviceId, action, playlist);
            player.Disposed += this.Player_Disposed;

            lock (this._syncRoot)
            {
                this.Players.Add(player);
            }

            return player;
        }

        /// <summary>
        /// Stops all <see cref="IAudioPlayer"/> associated with this instance.
        /// </summary>
        public void StopAll()
        {
            lock (this._syncRoot)
            {
                this.Players.ForEach(p => p.Stop());
            }
        }

        /// <summary>
        /// Handles the <see cref="IAudioPlayer.Disposed"/> event for players within <see cref="Players"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Player_Disposed(object sender, EventArgs e)
        {
            if (sender is IAudioPlayer player)
            {
                lock (this._syncRoot)
                {
                    player.Disposed -= this.Player_Disposed;
                    this.Players.Remove(player);
                }
            }
        }

        /// <summary>
        /// Gets the playlist player for the associated playlist player action type.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="action">The action type.</param>
        /// <param name="playlist">The playlist.</param>
        /// <returns>The playlist player.</returns>
        private PlaylistPlayer GetPlaylistPlayerInternal(string deviceId, PlaylistPlayerActionType action, IPlaylist playlist)
        {
            switch (action)
            {
                case PlaylistPlayerActionType.LoopStop:
                    return new LoopStopPlayer(deviceId, playlist, this.NormalizationProvider);

                case PlaylistPlayerActionType.LoopAllStop:
                    return new LoopAllStopPlayer(deviceId, playlist, this.NormalizationProvider);

                case PlaylistPlayerActionType.LoopAllStopReset:
                    return new LoopAllStopResetPlayer(deviceId, playlist, this.NormalizationProvider);

                case PlaylistPlayerActionType.PlayNext:
                    return new PlayNextPlayer(deviceId, playlist, this.NormalizationProvider);

                case PlaylistPlayerActionType.PlayStop:
                    return new PlayStopPlayer(deviceId, playlist, this.NormalizationProvider);
            }

            throw new NotSupportedException($"The provided playlist player action is not supported: {action}");
        }
    }
}
