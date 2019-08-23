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
        /// Occurs when the time of the current audio being played, changed.
        /// </summary>
        event EventHandler<PlaybackTimeEventArgs> TimeChanged;

        /// <summary>
        /// Gets the device identifier the audio will be played on.
        /// </summary>
        string DeviceId { get; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        PlaybackStateType State { get; }

        /// <summary>
        /// Gets the current and total time of the audio being played.
        /// </summary>
        PlaybackTimeEventArgs Time { get; }

        /// <summary>
        /// Plays the audio file asynchronously.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="maxGain">The optional maximum gain; when null, the default volume is used.</param>
        /// <returns>The task of the audio file being played.</returns>
        Task PlayAsync(string file, float? maxGain = null);

        /// <summary>
        /// Stops any audio being played on this player.
        /// </summary>
        void Stop();
    }
}
