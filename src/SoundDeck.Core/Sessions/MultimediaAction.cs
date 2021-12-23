namespace SoundDeck.Core.Sessions
{
    /// <summary>
    /// Provides an enumeration of possible multimedia actions.
    /// </summary>
    public enum MultimediaAction
    {
        /// <summary>
        /// Toggles between play / pause.
        /// </summary>
        TogglePlayPause = 0,

        /// <summary>
        /// Plays the media.
        /// </summary>
        Play = 1,

        /// <summary>
        /// Pauses the media.
        /// </summary>
        Pause = 2,

        /// <summary>
        /// Stops the media.
        /// </summary>
        Stop = 3,

        /// <summary>
        /// Skips to the previous item.
        /// </summary>
        SkipPrevious = 4,

        /// <summary>
        /// Skips to the next item.
        /// </summary>
        SkipNext = 5
    }
}
