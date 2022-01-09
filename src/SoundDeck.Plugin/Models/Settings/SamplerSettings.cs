namespace SoundDeck.Plugin.Models.Settings
{
    using Newtonsoft.Json;
    using SoundDeck.Core.Capture;
    using SoundDeck.Core.Playback;
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// Provides settings for a <see cref="Sampler"/>.
    /// </summary>
    public class SamplerSettings : ICaptureAudioSettings, ISaveAudioSettings, IPlayAudioSettings
    {
        /// <summary>
        /// Gets or sets the audio device identifier to capture.
        /// </summary>
        public string CaptureAudioDeviceId { get; set; }

        /// <summary>
        /// Gets a value indicating whether to encode the audio buffer to an MP3.
        /// </summary>
        public bool EncodeToMP3 { get; set; } = false;

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets a value indicating whether to normalize the volume of the buffer.
        /// </summary>
        public bool NormalizeVolume { get; set; } = true;

        /// <summary>
        /// Gets or sets the audio device identifier to playback the audio.
        /// </summary>
        public string PlaybackAudioDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the playback volume.
        /// </summary>
        public int PlaybackVolume { get; set; } = 75;

        /// <summary>
        /// Gets or sets the type of the action that occurs upon the button being pressed.
        /// </summary>
        public ControllerActionType Action { get; set; } = ControllerActionType.PlayNext;

        /// <summary>
        /// Gets the audio files to play.
        /// </summary>
        [JsonIgnore]
        public AudioFileInfo[] Files
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.FilePath)
                    ? new AudioFileInfo[0]
                    : new[]
                    {
                        new AudioFileInfo
                        {
                            Path = this.FilePath,
                            Volume = this.PlaybackVolume / 100f
                        }
                    };
            }
        }

        /// <summary>
        /// Gets the playback order.
        /// </summary>
        [JsonIgnore]
        public PlaybackOrderType Order { get; } = PlaybackOrderType.Sequential;

        /// <summary>
        /// Gets or sets the file path associated with the sampler.
        /// </summary>
        public string FilePath { get; set; }
    }
}
