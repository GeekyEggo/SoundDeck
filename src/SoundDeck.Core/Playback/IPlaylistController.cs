namespace SoundDeck.Core.Playback
{
    using System;
    using System.Threading.Tasks;
    using SoundDeck.Core.Playback.Playlists;

    /// <summary>
    /// Provides information and methods for a playlist controller.
    /// </summary>
    public interface IPlaylistController : IIndexedAudioPlayer, IDisposable
    {
        /// <summary>
        /// Gets the type of the action that occurs upon the button being pressed.
        /// </summary>
        ControllerActionType Action { get; }

        /// <summary>
        /// Gets or sets the playback order when reading the playlist.
        /// </summary>
        PlaybackOrderType Order { get; set; }

        /// <summary>
        /// Gets or sets the playlist.
        /// </summary>
        IPlaylist Playlist { get; set; }

        /// <summary>
        /// Moves to the next item within the playlist, and plays it asynchronously; this may stop audio depending on the type of player.
        /// </summary>
        /// <returns>The task of playing the item.</returns>
        Task NextAsync();
    }
}
