namespace SoundDeck.Core.Playback
{
    using System;

    /// <summary>
    /// Provides methods for an instance that can be stopped.
    /// </summary>
    public interface IStopper : IDisposable
    {
        /// <summary>
        /// Occurs when this instance is disposed.
        /// </summary>
        event EventHandler Disposed;

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();
    }
}
