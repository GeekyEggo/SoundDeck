namespace SoundDeck.Core.Playback
{
    using System.Threading.Tasks;

    /// <summary>
    /// An audio player capable of playing an audio file.
    /// </summary>
    public interface IAudioFilePlayer : IAudioPlayer
    {
        /// <summary>
        /// Gets the name of the file being played.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is looped.
        /// </summary>
        bool IsLooped { get; set; }

        /// <summary>
        /// Gets or sets the volume of the audio being played; this can be between 0 and 1.
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// Plays the audio file asynchronously.
        /// </summary>
        /// <param name="file">The file to play.</param>
        /// <returns>The task of the audio file being played.</returns>
        Task PlayAsync(AudioFileInfo file);
    }
}
