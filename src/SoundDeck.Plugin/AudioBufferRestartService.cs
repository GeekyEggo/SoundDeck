namespace SoundDeck.Plugin.Windows
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using SharpDeck.Connectivity;
    using SoundDeck.Core;

    /// <summary>
    /// Provides a service that restarts all audio buffers within <see cref="IAudioService"/> when <see cref="IStreamDeckConnection.SystemDidWakeUp"/> occurs.
    /// </summary>
    public class AudioBufferRestartService : IHostedService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioBufferRestartService"/> class.
        /// </summary>
        /// <param name="connection">The connection to the Stream Deck.</param>
        /// <param name="audioBufferService">The audio buffer service.</param>
        public AudioBufferRestartService(IStreamDeckConnection connection, IAudioBufferService audioBufferService)
        {
            this.AudioBufferService = audioBufferService;
            this.Connection = connection;
        }

        /// <summary>
        /// Gets the audio buffer service.
        /// </summary>
        private IAudioBufferService AudioBufferService { get; }

        /// <summary>
        /// Gets the connection to the Stream Deck.
        /// </summary>
        private IStreamDeckConnection Connection { get; }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.Connection.SystemDidWakeUp += (_, __) => this.AudioBufferService.Restart();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
