namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Core.Playback;

    /// <summary>
    /// Provides settings for <see cref="Actions.PlayAudioClip"/>
    /// </summary>
    public class PlayAudioClipSettings : IAudioPlaybackOptions
    {
        /// <summary>
        /// Gets or sets the audio device identifier to capture.
        /// </summary>
        public string AudioDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the type of the action that occurs upon the button being pressed.
        /// </summary>
        public PlaybackActionType Action { get; set; }

        /// <summary>
        /// Gets or sets the audio files to play.
        /// </summary>
        public string[] Files { get; set; }

        /// <summary>
        /// Gets or sets the playback order.
        /// </summary>
        public PlaybackOrderType Order { get; set; }
    }
}
