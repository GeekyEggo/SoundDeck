namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Core.Playback;

    /// <summary>
    /// Provides settings for an action capable of playing audio.
    /// </summary>
    public interface IPlayAudioSettings
    {
        /// <summary>
        /// Gets or sets the type of the action that occurs upon the button being pressed.
        /// </summary>
        ControllerActionType Action { get; set; }

        /// <summary>
        /// Gets the audio files to play.
        /// </summary>
        AudioFileInfo[] Files { get; }

        /// <summary>
        /// Gets the playback order.
        /// </summary>
        PlaybackOrderType Order { get; }

        /// <summary>
        /// Gets or sets the audio device identifier to playback the audio.
        /// </summary>
        string PlaybackAudioDeviceId { get; set; }
    }
}
