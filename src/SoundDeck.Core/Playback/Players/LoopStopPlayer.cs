namespace SoundDeck.Core.Playback.Players
{
    using System.Threading.Tasks;

    /// <summary>
    /// A playlist player that provides loop-stop functionality.
    /// </summary>
    public class LoopStopPlayer : PlaylistPlayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoopStopPlayer"/> class.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="playlist">The playlist.</param>
        /// <param name="normalizationProvider">The normalization provider.</param>
        public LoopStopPlayer(string deviceId, IPlaylist playlist, INormalizationProvider normalizationProvider)
            : base(deviceId, playlist, normalizationProvider)
        {
            this.IsLooped = true;
        }

        /// <summary>
        /// Gets the underlying action that determines how the player functions.
        /// </summary>
        public override PlaylistPlayerActionType Action => PlaylistPlayerActionType.LoopStop;

        /// <summary>
        /// Plays the next item within the playlist.
        /// </summary>
        /// <returns>The task of playing the next item.</returns>
        protected override Task PlayNextAsnyc()
        {
            if (this.State == PlaybackStateType.Playing)
            {
                this.Stop();
                return Task.CompletedTask;
            }

            return base.PlayNextAsnyc();
        }
    }
}
