namespace SoundDeck.Plugin
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SharpDeck;
    using SoundDeck.Core;

    /// <summary>
    /// The main de-coupled entry point for the Sound Deck plugin.
    /// </summary>
    public class SoundDeckPlugin
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SoundDeckPlugin"/> class.
        /// </summary>
        /// <param name="plugin">The plugin connector.</param>
        /// <param name="connection">The connection to the Stream Deck.</param>
        /// <param name="audioService">The audio service.</param>
        public SoundDeckPlugin(IStreamDeckPlugin plugin, IStreamDeckConnection connection, IAudioService audioService, ILogger<SoundDeckPlugin> logger)
        {
            this.Logger = logger;
            this.Plugin = plugin;

            connection.SystemDidWakeUp += (_, __) =>
            {
                foreach (var audioBuffer in audioService.GetAudioBuffers())
                {
                    audioBuffer.Restart();
                }
            };
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the Stream Deck plugin.
        /// </summary>
        private IStreamDeckPlugin Plugin { get; }

        /// <summary>
        /// Runs the Sound Deck plugin asynchronously.
        /// </summary>
        /// <returns>The task of running the Sound Deck.</returns>
        public Task RunAsync()
        {
            try
            {
                return this.Plugin.RunAsync();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Encountered error whilst running plugin.");
                throw;
            }
        }
    }
}
