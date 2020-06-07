namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Core.Playback;

    /// <summary>
    /// Provides settings for an action capable of playing audio.
    /// </summary>
    public interface IPlayAudioSettings : IPlaylistOptions
    {
        /// <summary>
        /// Gets or sets the audio device identifier to capture.
        /// </summary>
        string AudioDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the type of the action that occurs upon the button being pressed.
        /// </summary>
        PlaylistPlayerActionType Action { get; set; }
    }
}
