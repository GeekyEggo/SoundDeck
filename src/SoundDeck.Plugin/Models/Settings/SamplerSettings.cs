namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Core.Capture;
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// Provides settings for a <see cref="Sampler"/>.
    /// </summary>
    public class SamplerSettings : ICaptureAudioSettings, ISaveAudioSettings
    {
        /// <summary>
        /// Gets or sets the audio device identifier to capture.
        /// </summary>
        public string CaptureAudioDeviceId { get; set; }

        /// <summary>
        /// Gets a value indicating whether to encode the audio buffer to an MP3.
        /// </summary>
        public bool EncodeToMP3 { get; set; }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets a value indicating whether to normalize the volume of the buffer.
        /// </summary>
        public bool NormalizeVolume { get; set; }
    }
}
