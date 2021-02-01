namespace SoundDeck.Core.Playback
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides an audio player for an audio device.
    /// </summary>
    public interface IAudioPlayer : IDisposable
    {
        /// <summary>
        /// Occurs when the audio player is disposed.
        /// </summary>
        event EventHandler Disposed;

        /// <summary>
        /// Occurs when the time of the current audio being played, changed.
        /// </summary>
        event EventHandler<PlaybackTimeEventArgs> TimeChanged;

        /// <summary>
        /// Gets or sets the audio device identifier.
        /// </summary>
        string DeviceId { get; set; }

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
        /// Plays the audio file asynchronously.
        /// </summary>
        /// <param name="file">The file to play.</param>
        /// <returns>The task of the audio file being played.</returns>
        Task PlayAsync(AudioFileInfo file);

        /// <summary>
        /// Stops any audio being played on this player.
        /// </summary>
        void Stop();
    }
}
