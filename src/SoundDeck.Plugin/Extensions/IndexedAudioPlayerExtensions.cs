namespace SoundDeck.Plugin.Extensions
{
    using SoundDeck.Core.Playback;
    using SoundDeck.Plugin.Models.Payloads;

    /// <summary>
    /// Provides extension methods for <see cref="IIndexedAudioPlayer"/>.
    /// </summary>
    public static class IndexedAudioPlayerExtensions
    {
        /// <summary>
        /// When the current audio being played matches the indexes defined within the specified <paramref name="payload"/>, the volume of the audio player is updated.
        /// </summary>
        /// <param name="player">The player; this instance.</param>
        /// <param name="payload">The payload.</param>
        public static void TrySetVolume(this IIndexedAudioPlayer player, AdjustPlaylistFileVolumePayload payload)
        {
            if (player.Index == payload.Index
                && player.AudioPlayer != null)
            {
                player.AudioPlayer.Volume = payload.Volume;
            }
        }

        /// <summary>
        /// When the <paramref name="index"/> matches <see cref="IIndexedAudioPlayer.Index"/> the audio is stopped.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="index">The index.</param>
        public static void TryStop(this IIndexedAudioPlayer player, int index)
        {
            if (player.Index == index)
            {
                player.AudioPlayer?.Stop();
            }
        }
    }
}
