namespace SoundDeck.Plugin.Contracts
{
    using SharpDeck.Interactivity;
    using SoundDeck.Core;
    using SoundDeck.Core.Playback;

    /// <summary>
    /// Provides information about an action used to play audio.
    /// </summary>
    public interface IPlayAudioAction : IButton
    {
        /// <summary>
        /// Gets the audio service.
        /// </summary>
        IAudioService AudioService { get; }

        /// <summary>
        /// Gets or sets the playlist controller.
        /// </summary>
        IPlaylistController PlaylistController { get; set; }
    }
}
