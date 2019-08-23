namespace SoundDeck.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides a collection of <see cref="AudioDevice"/>.
    /// </summary>
    public interface IAudioDeviceCollection : IReadOnlyCollection<AudioDevice>, IDisposable
    {
        /// <summary>
        /// Occurs when the <see cref="DefaultPlaybackDevice"/> changed.
        /// </summary>
        event EventHandler DefaultPlaybackDeviceChanged;

        /// <summary>
        /// Gets the default playback device.
        /// </summary>
        AudioDevice DefaultPlaybackDevice { get; }
    }
}
