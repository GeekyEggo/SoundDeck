namespace SoundDeck.Core.Playback.Controllers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SoundDeck.Core.Playback.Playlists;

    /// <summary>
    /// Provides a base class for controlling a <see cref="IPlaylist"/> through an audio player.
    /// </summary>
    public abstract class PlaylistController : IPlaylistController
    {
        /// <summary>
        /// The synchronization root object.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Private member field for <see cref="Order"/>.
        /// </summary>
        private PlaybackOrderType _order = PlaybackOrderType.Sequential;

        /// <summary>
        /// Private member field for <see cref="PlaybackType"/>
        /// </summary>
        private ContinuousPlaybackType _playbackType = ContinuousPlaybackType.Single;

        /// <summary>
        /// Private member field for <see cref="Playlist"/>.
        /// </summary>
        private IPlaylist _playlist;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaylistController"/> class.
        /// </summary>
        /// <param name="audioPlayer">The audio player.</param>
        internal PlaylistController(IAudioPlayer audioPlayer)
        {
            this.AudioPlayer = audioPlayer;
            this.Playlist = new AudioFileCollection();
        }

        /// <summary>
        /// Gets the underlying action that determines how the player functions.
        /// </summary>
        public abstract ControllerActionType Action { get; }

        /// <summary>
        /// Gets the audio player.
        /// </summary>
        public IAudioPlayer AudioPlayer { get; private set; }

        /// <summary>
        /// Gets the index of the current file being played.
        /// </summary>
        public int Index => this.Enumerator?.CurrentIndex ?? -1;

        /// <summary>
        /// Gets or sets the playback order when reading the playlist.
        /// </summary>
        public PlaybackOrderType Order
        {
            get => this._order;
            set
            {
                if (this._order != value)
                {
                    this.StopThen(() =>
                    {
                        this._order = value;
                        this.SetEnumerator();
                    });
                }
            }
        }

        /// <summary>
        /// Gets or sets the playlist.
        /// </summary>
        public IPlaylist Playlist
        {
            get => this._playlist;
            set
            {
                this.StopThen(() =>
                {
                    this._playlist = value;
                    this.SetEnumerator();
                });
            }
        }

        /// <summary>
        /// Gets the enumerator responsible for iterating over the <see cref="Playlist"/>.
        /// </summary>
        protected IPlaylistEnumerator Enumerator { get; private set; }

        /// <summary>
        /// Gets or sets the type of the playback.
        /// </summary>
        protected ContinuousPlaybackType PlaybackType
        {
            get => this._playbackType;
            set
            {
                this._playbackType = value;
                this.AudioPlayer.IsLooped = this._playbackType == ContinuousPlaybackType.SingleLoop;
            }
        }

        /// <summary>
        /// Gets or sets the internal cancellation token source; this is used to cancel all audio on the player.
        /// </summary>
        private CancellationTokenSource AudioPlayerCancellationTokenSource { get; set; }

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
            this.AudioPlayerCancellationTokenSource?.Cancel();

            if (dispose)
            {
                if (!this.IsDisposed)
                {
                    this.AudioPlayer?.Dispose();
                    this.AudioPlayer = null;
                }

                this.IsDisposed = true;
            }
        }

        /// <summary>
        /// Stops any audio being played on this player.
        /// </summary>
        public void Stop()
        {
            this.AudioPlayerCancellationTokenSource?.Cancel();

            try
            {
                this._syncRoot.Wait();
                this.AudioPlayerCancellationTokenSource = null;
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
        protected virtual async Task PlayAsync()
        {
            try
            {
                this._syncRoot.Wait();

                if (this.AudioPlayerCancellationTokenSource == null)
                {
                    this.AudioPlayerCancellationTokenSource = new CancellationTokenSource();
                    this.AudioPlayerCancellationTokenSource.Token.Register(() => this.AudioPlayer.Stop());
                }

                do
                {
                    if (!this.Enumerator.TryMoveNext(out var current))
                    {
                        break;
                    }

                    await this.AudioPlayer.PlayAsync(current);
                }
                while (this.CanContinuePlaying(this.Enumerator, this.AudioPlayerCancellationTokenSource.Token));
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
        private bool CanContinuePlaying(IPlaylistEnumerator enumerator, CancellationToken cancellationToken)
        {
            return !cancellationToken.IsCancellationRequested
                && (this.PlaybackType == ContinuousPlaybackType.ContiunousLoop || (this.PlaybackType == ContinuousPlaybackType.Continuous && !enumerator.IsLast));
        }

        /// <summary>
        /// Sets the <see cref="Enumerator"/> based on the <see cref="Order"/>.
        /// </summary>
        private void SetEnumerator()
        {
            this.Enumerator = this.Order == PlaybackOrderType.Random
                ? new RandomizedPlaylistEnumerator(this.Playlist)
                : new PlaylistEnumerator(this.Playlist);
        }

        /// <summary>
        /// Stops any audio, and then synchronously invokes the action.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        private void StopThen(Action action)
        {
            this.Stop();
            try
            {
                this._syncRoot.Wait();
                action();
            }
            finally
            {
                this._syncRoot.Release();
            }
        }
    }
}
