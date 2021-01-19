namespace SoundDeck.Core.Playback
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a player for a <see cref="Playlist"/>.
    /// </summary>
    public interface IPlaylistPlayer : IAudioPlayer, IDisposable
    {
        /// <summary>
        /// Gets the underlying action that determines how the player functions.
        /// </summary>
        PlaylistPlayerActionType Action { get; }

        /// <summary>
        /// Gets the name of the file being played.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Gets or sets the volume of the audio being played; this can be between 0 and 1.
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// Moves to the next item within the playlist, and plays it asynchronously; this may stop audio depending on the type of player.
        /// </summary>
        /// <returns>The task of playing the item.</returns>
        Task NextAsync();
    }
}
