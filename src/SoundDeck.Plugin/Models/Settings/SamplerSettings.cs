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
        public bool EncodeToMP3 { get; set; } = true;

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets a value indicating whether to normalize the volume of the buffer.
        /// </summary>
        public bool NormalizeVolume { get; set; } = false;

        /// <summary>
        /// Gets or sets the audio device identifier to playback the audio.
        /// </summary>
        public string PlaybackAudioDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the type of the action that occurs upon the button being pressed.
        /// </summary>
        public PlaylistPlayerActionType Action { get; set; } = PlaylistPlayerActionType.PlayNext;

        /// <summary>
        /// Gets the audio files to play.
        /// </summary>
        [JsonIgnore]
        public PlaylistFile[] Files
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.FilePath)
                    ? new PlaylistFile[0]
                    : new[] { new PlaylistFile { Path = this.FilePath } };
            }
        }

        /// <summary>
        /// Gets the playback order.
        /// </summary>
        [JsonIgnore]
        public PlaylistOrderType Order { get; } = PlaylistOrderType.Sequential;

        /// <summary>
        /// Gets or sets the file path associated with the sampler.
        /// </summary>
        public string FilePath { get; set; }
    }
}
