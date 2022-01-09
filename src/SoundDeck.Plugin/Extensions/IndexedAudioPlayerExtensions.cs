namespace SoundDeck.Plugin.Extensions
{
    using SoundDeck.Core.Playback;

    /// <summary>
    /// Provides extension methods for <see cref="IIndexedAudioPlayer"/>.
    /// </summary>
    public static class IndexedAudioPlayerExtensions
    {
        /// <summary>
        /// When the current audio being played matches the indexes defined, the volume of the audio player is updated.
        /// </summary>
        /// <param name="player">The player; this instance.</param>
        /// <param name="index">The index of the file.</param>
        /// <param name="volume">The volume.</param>
        public static void TrySetVolume(this IIndexedAudioPlayer player, int index, float volume)
        {
            if (player?.Index == index
                && player.AudioPlayer != null)
            {
                player.AudioPlayer.Volume = volume;
            }
        }

        /// <summary>
        /// When the <paramref name="index"/> matches <see cref="IIndexedAudioPlayer.Index"/> the audio is stopped.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="index">The index.</param>
        public static void TryStop(this IIndexedAudioPlayer player, int index)
        {
            if (player?.Index == index)
            {
                player.AudioPlayer?.Stop();
            }
        }
    }
}
