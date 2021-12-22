namespace SoundDeck.Core
{
    /// <summary>
    /// Provides an enumeration of possible multimedia actions.
    /// </summary>
    public enum MultimediaAction
    {
        /// <summary>
        /// The skips to the next item.
        /// </summary>
        SkipNext = 0,

        /// <summary>
        /// The skips to the previous item.
        /// </summary>
        SkipPrevious = 1,

        /// <summary>
        /// Stops playback.
        /// </summary>
        Stop = 2,

        /// <summary>
        /// Toggle play/pause.
        /// </summary>
        TogglePlayPause = 3,
    }
}
