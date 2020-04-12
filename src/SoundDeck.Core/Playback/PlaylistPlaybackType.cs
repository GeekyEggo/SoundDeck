namespace SoundDeck.Core.Playback
{
    /// <summary>
    /// Defines the available types of playback for a playlist player.
    /// </summary>
    public enum PlaylistPlaybackType
    {
        /// <summary>
        /// A single audio clip is played.
        /// </summary>
        Single = 1,

        /// <summary>
        /// A single audio clip is played on loop.
        /// </summary>
        SingleLoop = 2,

        /// <summary>
        /// The entire playlist is played until the end.
        /// </summary>
        Continuous = 3,

        /// <summary>
        /// The entire playlist is played, and then looped when the end of the playlist is reached.
        /// </summary>
        ContiunousLoop = 4
    }
}
