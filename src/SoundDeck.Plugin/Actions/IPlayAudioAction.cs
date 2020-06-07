namespace SoundDeck.Plugin.Actions
{
    using SoundDeck.Core;
    using SoundDeck.Core.Playback;

    /// <summary>
    /// Provides information about an action used to play audio.
    /// </summary>
    public interface IPlayAudioAction
    {
        /// <summary>
        /// Gets the audio service.
        /// </summary>
        IAudioService AudioService { get; }

        /// <summary>
        /// Gets or sets the player.
        /// </summary>
        IPlaylistPlayer Player { get; set; }

        /// <summary>
        /// Gets or sets the playlist.
        /// </summary>
        Playlist Playlist { get; set; }
    }
}
