namespace SoundDeck.Core.Playback
{
    /// <summary>
    /// Provides an enumeration of current states an <see cref="IAudioPlayer"/> can be in.
    /// </summary>
    public enum PlaybackStateType
    {
        /// <summary>
        /// The audio player is stopped.
        /// </summary>
        Stopped = 0,

        /// <summary>
        /// The audio player is playing.
        /// </summary>
        Playing = 1,
    }
}
