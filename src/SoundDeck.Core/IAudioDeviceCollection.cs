namespace SoundDeck.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides a collection of <see cref="AudioDevice"/>.
    /// </summary>
    public interface IAudioDeviceCollection : IReadOnlyCollection<AudioDevice>, IDisposable
    {
    }
}
