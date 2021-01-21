namespace SoundDeck.Plugin.Models.Settings
{
    using Newtonsoft.Json;
    using SoundDeck.Core.Capture;

    /// <summary>
    /// Provides settings for recording audio.
    /// </summary>
    public class RecordAudioSettings : ICaptureAudioSettings, ISaveAudioSettings
    {
        /// <summary>
        /// Gets or sets the audio device identifier to capture.
        /// </summary>
        [JsonProperty("audioDeviceId")]
        public string CaptureAudioDeviceId { get; set; }

        /// <summary>
        /// Gets a value indicating whether to encode the audio buffer to an MP3.
        /// </summary>
        public bool EncodeToMP3 { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether to normalize the volume of the buffer.
        /// </summary>
        public bool NormalizeVolume { get; set; } = true;

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        public string OutputPath { get; set; }
    }
}
