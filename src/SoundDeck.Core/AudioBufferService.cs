namespace SoundDeck.Core
{
    using System;
    using System.Collections.Concurrent;
    using Microsoft.Extensions.Logging;
    using SoundDeck.Core.Capture;
    using SoundDeck.Core.Capture.Sharing;

    /// <summary>
    /// Provides methods for interacting with instances of <see cref="IAudioBuffer"/>.
    /// </summary>
    public class AudioBufferService : IAudioBufferService
    {
        /// <summary>
        /// Initializes a new instance of class <see cref="AudioBufferService"/>.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        public AudioBufferService(ILoggerFactory loggerFactory)
        {
            this.LoggerFactory = loggerFactory;
        }

        /// <summary>
        /// Gets the logger factory.
        /// </summary>
        private ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Gets the shared audio buffers, grouped by device key.
        /// </summary>
        private ConcurrentDictionary<string, SharedAudioBufferFactory> AudioBufferFactories { get; } = new ConcurrentDictionary<string, SharedAudioBufferFactory>();

        /// <inheritdoc/>
        public IAudioBuffer GetAudioBuffer(string deviceKey, TimeSpan bufferDuration)
        {
            var factory = this.AudioBufferFactories.GetOrAdd(deviceKey, _ => new SharedAudioBufferFactory(deviceKey, bufferDuration, this.LoggerFactory));
            return factory.Create(bufferDuration);
        }

        /// <inheritdoc/>
        public void Restart()
        {
            foreach (var sharedAudioBuffer in this.AudioBufferFactories.Values)
            {
                sharedAudioBuffer.AudioBuffer.Restart();
            }
        }
    }
}
