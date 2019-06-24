namespace SoundDeck.Plugin.Models.Settings
{
    /// <summary>
    /// Provides settings for <see cref="Actions.PlayAudioClip"/>
    /// </summary>
    public class PlayAudioClipSettings
    {
        /// <summary>
        /// Gets or sets the audio device identifier to capture.
        /// </summary>
        public string AudioDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the audio files to play.
        /// </summary>
        public string[] Files { get; set; }
    }
}
