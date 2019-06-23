namespace SoundDeck.Core
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides an audio player for an audio device.
    /// </summary>
    public interface IAudioPlayer : IDisposable
    {
        /// <summary>
        /// Gets the device identifier the audio will be played on.
        /// </summary>
        string DeviceId { get; }

        /// <summary>
        /// Plays the audio file asynchronously.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The task of the audio file being played.</returns>
        Task PlayAsync(string file, CancellationToken token);

        /// <summary>
        /// Stops any audio being played on this player.
        /// </summary>
        void Stop();
    }
}
