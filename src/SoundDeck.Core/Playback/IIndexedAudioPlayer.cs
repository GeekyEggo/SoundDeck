namespace SoundDeck.Core.Playback
{
    /// <summary>
    /// Provides information about an audio player capable of playing an indexed item.
    /// </summary>
    public interface IIndexedAudioPlayer
    {
        /// <summary>
        /// Gets the audio player.
        /// </summary>
        IAudioPlayer AudioPlayer { get; }

        /// <summary>
        /// Gets the index of the current file being played.
        /// </summary>
        int Index { get; }
    }
}
