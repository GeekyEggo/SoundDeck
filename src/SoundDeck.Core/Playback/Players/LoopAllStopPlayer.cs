namespace SoundDeck.Core.Playback.Players
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A playlist player that provides loop all-stop functionality, whereby the entire playlist is looped.
    /// </summary>
    public class LoopAllStopPlayer : PlaylistPlayer
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private static readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="LoopStopPlayer"/> class.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="playlist">The playlist.</param>
        /// <param name="normalizationProvider">The normalization provider.</param>
        public LoopAllStopPlayer(string deviceId, IPlaylist playlist, INormalizationProvider normalizationProvider)
            : base(deviceId, playlist, normalizationProvider)
        {
        }

        /// <summary>
        /// Gets the underlying action that determines how the player functions.
        /// </summary>
        public override PlaylistPlayerActionType Action => PlaylistPlayerActionType.LoopAllStop;

        /// <summary>
        /// Gets or sets the internal cancellation token; this is used to cancel the loop.
        /// </summary>
        private CancellationTokenSource InternalCancellationToken { get; set; } = null;

        /// <summary>
        /// Plays the next item within the playlist.
        /// </summary>
        /// <returns>The task of playing the next item.</returns>
        protected override Task PlayNextAsnyc()
        {
            try
            {
                _syncRoot.Wait();

                if (this.InternalCancellationToken != null)
                {
                    // when there is a cancellation token, stop playback
                    this.Stop();

                    this.InternalCancellationToken?.Cancel();
                    this.InternalCancellationToken = null;
                }
                else
                {
                    // begin playback
                    this.InternalCancellationToken = new CancellationTokenSource();
                    return this.RunContinouslyAsync();
                }
            }
            finally
            {
                _syncRoot.Release();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Continously plays the entire playlist asynchronously.
        /// </summary>
        /// <returns>The task of continuous playback.</returns>
        protected virtual async Task RunContinouslyAsync()
        {
            while (this.InternalCancellationToken?.IsCancellationRequested == false)
            {
                await base.PlayNextAsnyc();
            }
        }
    }
}
