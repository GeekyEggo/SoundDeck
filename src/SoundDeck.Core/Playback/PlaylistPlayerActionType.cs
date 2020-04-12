namespace SoundDeck.Core.Playback
{
    /// <summary>
    /// Provides an enumeration of actions that a <see cref="Playlist"/> can perform when <see cref="Playlist.NextAsync"/> is invoked.
    /// </summary>
    public enum PlaylistPlayerActionType
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
        LoopStop = 2,

        /// <summary>
        /// Starts the next audio clip and loops the entire playlist; stops on next press.
        /// </summary>
        LoopAllStop = 3,

        /// <summary>
        /// Starts the first audio clip and loops the entire playlist; stops on next press.
        /// </summary>
        LoopAllStopReset = 4,

        /// <summary>
        /// Continues to the play the playlist from the next clip, until stopped or the end of the playlist is reached.
        /// </summary>
        PlayAllStop = 5
    }
}
