namespace SoundDeck.Core.Playback
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides an audio player for an audio device.
    /// </summary>
    public interface IAudioPlayer : IStopper, IDisposable
    {
        /// <summary>
        /// Occurs when the time of the current audio being played, changed.
        /// </summary>
        event EventHandler<PlaybackTimeEventArgs> TimeChanged;

        /// <summary>
        /// Gets or sets the audio device.
        /// </summary>
        IAudioDevice Device { get; set; }

        /// <summary>
        /// Gets the name of the file being played.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is looped.
        /// </summary>
        bool IsLooped { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is playing.
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Gets or sets the volume of the audio being played; this can be between 0 and 1.
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// Shallow clones this instance.
        /// </summary>
        IAudioPlayer Clone();

        /// <summary>
        /// Plays the audio file asynchronously.
        /// </summary>
        /// <param name="file">The file to play.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>The task of the audio file being played.</returns>
        Task PlayAsync(AudioFileInfo file, CancellationToken cancellationToken = default);
    }
}
