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
        /// Gets or sets the connection.
        /// </summary>
        private static IStreamDeckConnection Connection { get; set; }

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
                .OnSetup(conn =>
                {
                    conn.DeviceDidConnect += Client_DeviceDidConnect;
                    conn.SystemDidWakeUp += Client_SystemDidWakeUp;
                })
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

        /// <summary>
        /// Handles the <see cref="IStreamDeckConnection.DeviceDidConnect"/> event of the main Stream Deck connection.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DeviceConnectEventArgs"/> instance containing the event data.</param>
        private static void Client_DeviceDidConnect(object sender, DeviceConnectEventArgs e)
        {
            if (sender is IStreamDeckConnection connection)
            {
                Connection = connection;
                connection.DeviceDidConnect -= Client_DeviceDidConnect;

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
            await Connection.SetGlobalSettingsAsync(settings).ConfigureAwait(false);
        }
    }
}
