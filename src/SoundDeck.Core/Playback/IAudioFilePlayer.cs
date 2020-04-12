namespace SoundDeck.Core.Playback
{
    using System.Threading.Tasks;

    /// <summary>
    /// An audio player capable of playing an audio file.
    /// </summary>
    public interface IAudioFilePlayer : IAudioPlayer
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is looped.
        /// </summary>
        bool IsLooped { get; set; }

        /// <summary>
        /// Plays the audio file asynchronously.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="maxGain">The optional maximum gain; when null, the default volume is used.</param>
        /// <returns>The task of the audio file being played.</returns>
        Task PlayAsync(string file, float? maxGain = null);
    }
}
