using SharpDeck.Manifest;
[assembly: StreamDeckPlugin(
    Name = "Sound Deck",
    Category = "Sound Deck",
    CategoryIcon = "Images/SoundDeck/Category",
    Description = @"With advanced audio device support; Sound Deck lets you play, record, and clip from any device!

- Multiple clip support, with custom Play-Action.
- Start/stop recording from any device.
- Clip-it recording... to record the last x seconds!",
    CodePath = "SoundDeck.exe",
    Icon = "Images/SoundDeck/Icon",
    PropertyInspectorPath = "PI/index.html",
    Author = "GeekyEggo",
    URL = "https://github.com/GeekyEggo/SoundDeck",
    WindowsMinimumVersion = "10")]

namespace SoundDeck.Plugin
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Exceptions;
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
        /// Gets or sets the Stream Deck client.
        /// </summary>
        private static IStreamDeckClient Client { get; set; }

        /// <summary>
        /// Runs the Sound Deck plugin asynchronously.
        /// </summary>
        /// <param name="args">The arguments supplied by the console or entry point.</param>
        /// <param name="provider">The service provider.</param>
        /// <returns>The task of running the Sound Deck.</returns>
        public static async Task RunAsync(string[] args, IServiceProvider provider)
        {
            AudioService = provider.GetRequiredService<IAudioService>();

            await StreamDeckClient.RunAsync(provider: provider, setup: client =>
            {
                client.DeviceDidConnect += Client_DeviceDidConnect;
                client.Error += Client_Error;
            });
        }

        /// <summary>
        /// Handles the <see cref="IStreamDeckClient.DeviceDidConnect"/> event of the main Stream Deck client.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DeviceConnectEventArgs"/> instance containing the event data.</param>
        private static void Client_DeviceDidConnect(object sender, DeviceConnectEventArgs e)
        {
            if (sender is StreamDeckClient client)
            {
                Client = client;
                client.DeviceDidConnect -= Client_DeviceDidConnect;

                // set the global settings, and update them if the default playback device changes
                UpdateGlobalSettings();
                AudioService.Devices.DefaultPlaybackDeviceChanged += (_, __) => UpdateGlobalSettings();
            }
        }

        /// <summary>
        /// Updates the global settings.
        /// </summary>
        private static async void UpdateGlobalSettings()
        {
            var settings = new PluginSettings(AudioService.Devices.DefaultPlaybackDevice?.Id);
            await Client.SetGlobalSettingsAsync(settings).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the <see cref="IStreamDeckClient.Error"/> event of the main Stream Deck client.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="StreamDeckConnectionErrorEventArgs"/> instance containing the event data.</param>
        private static async void Client_Error(IStreamDeckClient sender, StreamDeckConnectionErrorEventArgs e)
        {
            await sender.LogMessageAsync(e.Exception.ToString());
            await sender.LogMessageAsync(e.Message);
        }
    }
}
