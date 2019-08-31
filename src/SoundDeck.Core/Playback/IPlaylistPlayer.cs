namespace SoundDeck.Core.Playback
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a player for a <see cref="Playlist"/>.
    /// </summary>
    public interface IPlaylistPlayer : IDisposable
    {
        /// <summary>
        /// Occurs when the time of the current audio being played, changed.
        /// </summary>
        event EventHandler<PlaybackTimeEventArgs> TimeChanged;

        /// <summary>
        /// Gets the underlying action that determines how the player functions.
        /// </summary>
        PlaylistPlayerActionType Action { get; }

        /// <summary>
        /// Gets the device identifier the audio will be played on.
        /// </summary>
        string DeviceId { get; }

        /// <summary>
        /// Moves to the next item within the playlist, and plays it asynchronously; this may stop audio depending on the type of player.
        /// </summary>
        /// <returns>The task of playing the item.</returns>
        Task NextAsync();
    }
}
