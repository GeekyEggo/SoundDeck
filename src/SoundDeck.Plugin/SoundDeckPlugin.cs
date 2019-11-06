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
    using SharpDeck.Threading;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// The main de-coupled entry point for the Sound Deck plugin.
    /// </summary>
    public static class SoundDeckPlugin
    {
        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        private static IServiceProvider Provider { get; set; }

        /// <summary>
        /// Runs the Sound Deck plugin asynchronously.
        /// </summary>
        /// <param name="args">The arguments supplied by the console or entry point.</param>
        /// <param name="provider">The service provider.</param>
        /// <returns>The task of running the Sound Deck.</returns>
        public static async Task RunAsync(string[] args, IServiceProvider provider)
        {
            Provider = provider;
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
        private static async void Client_DeviceDidConnect(object sender, DeviceConnectEventArgs e)
        {
            if (sender is StreamDeckClient client)
            {
                client.DeviceDidConnect -= Client_DeviceDidConnect;

                // declare the local function to update the default playback device id
                var devices = Provider.GetRequiredService<IAudioService>().Devices;
                Task updateGlobalSettings() => client.SetGlobalSettingsAsync(new PluginSettings(devices.DefaultPlaybackDevice?.Id));

                // set the default settings, and monitor changes
                await updateGlobalSettings();
                devices.DefaultPlaybackDeviceChanged += async (_, __) => await updateGlobalSettings();
            }
        }

        /// <summary>
        /// Handles the <see cref="IStreamDeckClient.Error"/> event of the main Stream Deck client.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="StreamDeckConnectionErrorEventArgs"/> instance containing the event data.</param>
        private static async void Client_Error(IStreamDeckClient sender, StreamDeckConnectionErrorEventArgs e)
        {
            using (SynchronizationContextSwitcher.NoContext())
            {
                await sender.LogMessageAsync(e.Exception.ToString());
                await sender.LogMessageAsync(e.Message);
            }
        }
    }
}
