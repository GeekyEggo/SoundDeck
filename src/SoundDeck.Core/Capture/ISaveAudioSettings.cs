namespace SoundDeck.Core.Capture
{
    using SoundDeck.Core.IO;

    /// <summary>
    /// Provides settings for saving an audio clip.
    /// </summary>
    public interface ISaveAudioSettings : IAudioFileWriterSettings
    {
        /// <summary>
        /// Gets the output path, directory, where the file should be saved to.
        /// </summary>
        string OutputPath { get; }
    }
}
