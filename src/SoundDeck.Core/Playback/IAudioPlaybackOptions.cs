namespace SoundDeck.Core.Playback
{
    /// <summary>
    /// Provides options that determines what audio clips are played, and how.
    /// </summary>
    public interface IAudioPlaybackOptions
    {
        /// <summary>
        /// Gets the type of the action that occurs upon the button being pressed.
        /// </summary>
        PlaybackActionType Action { get; }

        /// <summary>
        /// Gets the audio files to play.
        /// </summary>
        string[] Files { get; }

        /// <summary>
        /// Gets the playback order.
        /// </summary>
        PlaybackOrderType Order { get; }
    }
}
