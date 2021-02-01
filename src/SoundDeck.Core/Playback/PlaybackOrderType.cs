namespace SoundDeck.Core.Playback
{
    /// <summary>
    /// Provides an enumeration of playlist orders; either sequential, or random.
    /// </summary>
    public enum PlaybackOrderType
    {
        /// <summary>
        /// The playback is sequential.
        /// </summary>
        Sequential = 0,

        /// <summary>
        /// The playback is random.
        /// </summary>
        Random = 1
    }
}
