namespace SoundDeck.Plugin.Models.Settings
{
    using Newtonsoft.Json;
    using SoundDeck.Core.Playback;

    /// <summary>
    /// Provides settings for <see cref="Actions.PlayAudio"/>
    /// </summary>
    public class PlayAudioSettings : IPlayAudioSettings
    {
        /// <summary>
        /// Gets or sets the audio device identifier to playback the audio.
        /// </summary>
        [JsonProperty("audioDeviceId")]
        public string PlaybackAudioDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the type of the action that occurs upon the button being pressed.
        /// </summary>
        public ControllerActionType Action { get; set; } = ControllerActionType.PlayNext;

        /// <summary>
        /// Gets or sets the audio files to play.
        /// </summary>
        public AudioFileInfo[] Files { get; set; }

        /// <summary>
        /// Gets or sets the playback order.
        /// </summary>
        public PlaybackOrderType Order { get; set; } = PlaybackOrderType.Sequential;
    }
}
