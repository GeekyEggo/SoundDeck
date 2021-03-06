namespace SoundDeck.Plugin.Models.Settings
{
    /// <summary>
    /// Provides base information about settings for an action capable of capturing audio.
    /// </summary>
    public interface ICaptureAudioSettings
    {
        /// <summary>
        /// Gets or sets the audio device identifier to capture.
        /// </summary>
        string CaptureAudioDeviceId { get; set; }

        /// <summary>
        /// Gets a value indicating whether to encode the audio buffer to an MP3.
        /// </summary>
        bool EncodeToMP3 { get; set; }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        string OutputPath { get; set; }

        /// <summary>
        /// Gets a value indicating whether to normalize the volume of the buffer.
        /// </summary>
        bool NormalizeVolume { get; set; }
    }
}
