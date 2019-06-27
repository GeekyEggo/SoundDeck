namespace SoundDeck.Plugin.Models.Settings
{
    using Newtonsoft.Json;
    using SoundDeck.Core;
    using SoundDeck.Core.Serialization;
    using System;

    /// <summary>
    /// Provides settings for <see cref="Actions.ClipAudio"/>.
    /// </summary>
    public class ClipAudioSettings : ISaveBufferSettings
    {
        /// <summary>
        /// Gets or sets the audio device identifier to capture.
        /// </summary>
        public string AudioDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the duration of the clip.
        /// </summary>
        [JsonConverter(typeof(TimeSpanJsonConverter))]
        public TimeSpan Duration { get; set; }

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
        public bool NormalizeVolume { get; set; } = true;
    }
}
