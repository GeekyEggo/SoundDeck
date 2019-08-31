namespace SoundDeck.Core.Playback.Players
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a base implementation of a player designed to play a collection of files.
    /// </summary>
    public abstract class PlaylistPlayer : AudioPlayer, IPlaylistPlayer
    {
        /// <summary>
        /// The synchronization root object.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaylistPlayer"/> class.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="playlist">The playlist.</param>
        /// <param name="normalizationProvider">The normalization provider.</param>
        public PlaylistPlayer(string deviceId, Playlist playlist, INormalizationProvider normalizationProvider)
            : base(deviceId, normalizationProvider)
        {
            this.Playlist = playlist;
        }

        /// <summary>
        /// Gets or sets the maximum gain, i.e. the max volume.
        /// </summary>
        private float MaxGain { get; set; } = 0.35f;

        /// <summary>
        /// Gets the underlying action that determines how the player functions.
        /// </summary>
        public abstract PlaylistPlayerActionType Action { get; }

        /// <summary>
        /// Gets or sets the playlist.
        /// </summary>
        private Playlist Playlist { get; set; }

        /// <summary>
        /// Moves to the next item within the playlist, and plays it asynchronously; this may stop audio depending on the type of player.
        /// </summary>
        /// <returns>The task of playing the item.</returns>
        public virtual Task NextAsync()
        {
            if (this.Playlist.Length == 0)
            {
                return Task.CompletedTask;
            }

            return this.PlayNextAsnyc();
        }

        /// <summary>
        /// Plays the next item within the playlist.
        /// </summary>
        /// <returns>The task of playing the next item.</returns>
        protected virtual async Task PlayNextAsnyc()
        {
            try
            {
                await this._syncRoot.WaitAsync();
                if (this.Playlist.MoveNext())
                {
                    await this.PlayAsync(this.Playlist.Current, this.MaxGain);
                }
            }
            finally
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="dispose"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool dispose)
        {
            this._syncRoot?.Dispose();
            base.Dispose(dispose);
        }
    }
}
