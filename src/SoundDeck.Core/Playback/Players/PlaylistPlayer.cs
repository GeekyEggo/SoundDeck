namespace SoundDeck.Core.Playback.Players
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a base implementation of a player designed to play a collection of files.
    /// </summary>
    public abstract class PlaylistPlayer : IPlaylistPlayer
    {
        /// <summary>
        /// The maximum gain.
        /// </summary>
        private const float MAX_GAIN = 0.35f;

        /// <summary>
        /// The synchronization root object.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// The playlist playback type.
        /// </summary>
        private PlaylistPlaybackType _playlistPlaybackType = PlaylistPlaybackType.Single;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaylistPlayer" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public PlaylistPlayer(PlaylistPlayerOptions options)
        {
            this.Player = new AudioPlayer(options.DeviceId, options.NormalizationProvider);
            this.Playlist = options.Playlist;
        }

        /// <summary>
        /// Occurs when the audio player is disposed.
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        /// Occurs when the time of the current audio being played, changed.
        /// </summary>
        public event EventHandler<PlaybackTimeEventArgs> TimeChanged
        {
            add { this.Player.TimeChanged += value; }
            remove { this.Player.TimeChanged -= value; }
        }

        /// <summary>
        /// Gets the underlying action that determines how the player functions.
        /// </summary>
        public abstract PlaylistPlayerActionType Action { get; }

        /// <summary>
        /// Gets the device identifier the audio will be played on.
        /// </summary>
        public string DeviceId => this.Player.DeviceId;

        /// <summary>
        /// Gets the state.
        /// </summary>
        public PlaybackStateType State { get; private set; }

        /// <summary>
        /// Gets the internal player responsible for playing clips within the <see cref="Playlist"/>.
        /// </summary>
        protected IAudioFilePlayer Player { get; private set; }

        /// <summary>
        /// Gets the playlist.
        /// </summary>
        protected IPlaylist Playlist { get; }

        /// <summary>
        /// Gets or sets the type of the playback.
        /// </summary>
        protected PlaylistPlaybackType PlaybackType
        {
            get => this._playlistPlaybackType;
            set
            {
                this._playlistPlaybackType = value;
                this.Player.IsLooped = this._playlistPlaybackType == PlaylistPlaybackType.SingleLoop;
            }
        }

        /// <summary>
        /// Gets or sets the internal cancellation token source; this is used to cancel all audio on the player.
        /// </summary>
        private CancellationTokenSource InternalCancellationTokenSource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        private bool IsDisposed { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Moves to the next item within the playlist, and plays it asynchronously; this may stop audio depending on the type of player.
        /// </summary>
        /// <returns>The task of playing the item.</returns>
        public Task NextAsync()
        {
            if (this.Playlist.Count == 0)
            {
                return Task.CompletedTask;
            }

            return this.ActionAsync();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="dispose"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool dispose)
        {
            this.InternalCancellationTokenSource?.Cancel();
            if (dispose)
            {
                if (!this.IsDisposed)
                {
                    this.Player.Dispose();
                    this.Disposed?.Invoke(this, EventArgs.Empty);
                }

                this.IsDisposed = true;
            }
        }

        /// <summary>
        /// Stops any audio being played on this player.
        /// </summary>
        public void Stop()
        {
            try
            {
                this._syncRoot.Wait();

                this.Player.Stop();
                this.InternalCancellationTokenSource?.Cancel();
                this.InternalCancellationTokenSource = null;

                this.State = PlaybackStateType.Stopped;
            }
            finally
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Applies the next action asynchronously.
        /// </summary>
        /// <returns>The task of running the action.</returns>
        protected abstract Task ActionAsync();

        /// <summary>
        /// Continues playing asynchronously.
        /// </summary>
        /// <returns>The task of playing.</returns>
        protected virtual Task PlayAsync()
        {
            try
            {
                this._syncRoot.Wait();

                if (this.InternalCancellationTokenSource == null)
                {
                    this.InternalCancellationTokenSource = new CancellationTokenSource();
                }

                this.State = PlaybackStateType.Playing;
                return this.PlayInternalAsync(this.InternalCancellationTokenSource.Token);
            }
            finally
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Provides an internal method for continuing the playback of the <see cref="Playlist"/>.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task of playback.</returns>
        private async Task PlayInternalAsync(CancellationToken cancellationToken)
        {
            do
            {
                if (!this.Playlist.MoveNext())
                {
                    break;
                }

                await this.Player.PlayAsync(this.Playlist.Current.Path, MAX_GAIN);
            }
            while (this.CanContinuePlaying(cancellationToken));

            try
            {
                await this._syncRoot.WaitAsync();
                this.State = PlaybackStateType.Stopped;
            }
            finally
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Determines when playback can continue based on the state of this instance, the <see cref="Playlist"/> and the <paramref name="cancellationToken"/>.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns><c>true</c> when playback can continue; otherwise <c>false</c>.</returns>
        private bool CanContinuePlaying(CancellationToken cancellationToken)
        {
            return !cancellationToken.IsCancellationRequested
                && (this.PlaybackType == PlaylistPlaybackType.ContiunousLoop || (this.PlaybackType == PlaylistPlaybackType.Continuous && !this.Playlist.IsLast));
        }
    }
}
