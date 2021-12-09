namespace SoundDeck.Core
{
    using System;
    using SoundDeck.Core.Capture;

    /// <summary>
    /// Provides methods for interacting with instances of <see cref="IAudioBuffer"/>.
    /// </summary>
    public interface IAudioBufferService
    {
        /// <summary>
        /// Gets an audio buffer for the specified device key.
        /// </summary>
        /// <param name="deviceId">The device key.</param>
        /// <param name="bufferDuration">The buffer duration.</param>
        /// <returns>The audio buffer.</returns>
        IAudioBuffer GetAudioBuffer(string deviceKey, TimeSpan bufferDuration);

        /// <summary>
        /// Restarts all audio buffers.
        /// </summary>
        void Restart();
    }
}
