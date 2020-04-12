namespace SoundDeck.Core.Playback.Players
{
    using System.Threading.Tasks;

    /// <summary>
    /// A playlist player that provides loop all-reset functionality, whereby the entire playlist is looped from the first item in the playlist.
    /// </summary>
    public class LoopAllStopResetPlayer : LoopAllStopPlayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoopAllStopResetPlayer"/> class.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="playlist">The playlist.</param>
        /// <param name="normalizationProvider">The normalization provider.</param>
        public LoopAllStopResetPlayer(string deviceId, IPlaylist playlist, INormalizationProvider normalizationProvider)
            : base(deviceId, playlist, normalizationProvider)
        {
        }

        /// <summary>
        /// Continously plays the entire playlist asynchronously.
        /// </summary>
        /// <returns>The task of continuous playback.</returns>
        protected override Task RunContinouslyAsync()
        {
            this.Playlist.Reset();
            return base.RunContinouslyAsync();
        }
    }
}
