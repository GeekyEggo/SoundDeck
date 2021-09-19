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
        /// <param name="audioService">The audio service.</param>
        public AudioBufferRestartService(IStreamDeckConnection connection, IAudioService audioService)
        {
            this.AudioService = audioService;
            this.Connection = connection;
        }

        /// <summary>
        /// Gets the audio service.
        /// </summary>
        private IAudioService AudioService { get; }

        /// <summary>
        /// Gets the connection to the Stream Deck.
        /// </summary>
        private IStreamDeckConnection Connection { get; }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.Connection.SystemDidWakeUp += this.Connection_SystemDidWakeUp;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        /// <summary>
        /// Handles the <see cref="IStreamDeckConnection.SystemDidWakeUp"/> event for <see cref="Connection"/>, and restarts all of the audio buffers within <see cref="IAudioService"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SharpDeck.Events.Received.StreamDeckEventArgs"/> instance containing the event data.</param>
        private void Connection_SystemDidWakeUp(object sender, SharpDeck.Events.Received.StreamDeckEventArgs e)
        {
            foreach (var audioBuffer in this.AudioService.GetAudioBuffers())
            {
                audioBuffer.Restart();
            }
        }
    }
}
