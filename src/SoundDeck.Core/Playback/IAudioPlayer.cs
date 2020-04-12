namespace SoundDeck.Core.Playback
{
    using System;

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
        /// Gets the device identifier the audio will be played on.
        /// </summary>
        string DeviceId { get; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        PlaybackStateType State { get; }

        /// <summary>
        /// Stops any audio being played on this player.
        /// </summary>
        void Stop();
    }
}
