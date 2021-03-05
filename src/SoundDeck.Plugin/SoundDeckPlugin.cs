namespace SoundDeck.Plugin
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// The main de-coupled entry point for the Sound Deck plugin.
    /// </summary>
    public static class SoundDeckPlugin
    {
        /// <summary>
        /// Gets or sets the audio service.
        /// </summary>
        private static IAudioService AudioService { get; set; }

        /// <summary>
        /// Runs the Sound Deck plugin asynchronously.
        /// </summary>
        /// <param name="args">The arguments supplied by the console or entry point.</param>
        /// <param name="provider">The service provider.</param>
        /// <returns>The task of running the Sound Deck.</returns>
        public static Task RunAsync(IServiceProvider provider)
        {
            AudioService = provider.GetRequiredService<IAudioService>();

            return StreamDeckPlugin.Create()
                .WithServiceProvider(provider)
                .OnSetup(conn => conn.SystemDidWakeUp += Client_SystemDidWakeUp)
                .RunAsync(CancellationToken.None);
        }

        /// <summary>
        /// Handles the <see cref="IStreamDeckConnection.SystemDidWakeUp"/> event on the main Stream Deck connection.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="StreamDeckEventArgs"/> instance containing the event data.</param>
        private static void Client_SystemDidWakeUp(object sender, StreamDeckEventArgs e)
        {
            foreach (var audioBuffer in AudioService.GetAudioBuffers())
            {
                audioBuffer.Restart();
            }
        }
    }
}
