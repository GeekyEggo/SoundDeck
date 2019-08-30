namespace SoundDeck.Core.Playback
{
    /// <summary>
    /// Provides an enumeration of actions that a <see cref="AudioPlaybackCollection"/> can perform when <see cref="AudioPlaybackCollection.NextAsync"/> is invoked.
    /// </summary>
    public enum PlaybackActionType
    {
        /// <summary>
        /// Stops the current audio clip, and plays the next one.
        /// </summary>
        PlayNext = 0,

        /// <summary>
        /// Stops the current audio clip if one is playing; otherwise the next one is played.
        /// </summary>
        PlayStop = 1,

        /// <summary>
        /// Starts the next audio clip on loop; stops on next press.
        /// </summary>
        LoopStop = 2
    }
}
