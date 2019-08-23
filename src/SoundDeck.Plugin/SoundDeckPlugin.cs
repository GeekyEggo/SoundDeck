using SharpDeck.Manifest;
[assembly: StreamDeckPlugin(
    Name = "Sound Deck",
    Category = "Sound Deck",
    CategoryIcon = "Images/SoundDeck/Category",
    Description = "An advanced soundboard with support for output audio device selection. SoundDeck also supports recording audio, or clipping the last precious seconds of audio, from any of your sound devices",
    CodePath = "SoundDeck.exe",
    Icon = "Images/SoundDeck/Icon",
    PropertyInspectorPath = "PI/index.html",
    Version = "1.0.0",
    Author = "GeekyEggo",
    WindowsMinimumVersion = "10")]

namespace SoundDeck.Plugin
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SharpDeck;
    using SharpDeck.Exceptions;
    using SoundDeck.Core;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Plugin.Actions;
    using Microsoft.Extensions.DependencyInjection;
    using SoundDeck.Plugin.Models.Settings;
    using SharpDeck.Events.Received;

    /// <summary>
    /// The main de-coupled entry point for the Sound Deck plugin.
    /// </summary>
    public class SoundDeckPlugin
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly static object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="SoundDeckPlugin"/> class.
        /// </summary>
        /// <param name="provider">The service provider.</param>
        public SoundDeckPlugin(IServiceProvider provider)
        {
            this.Provider = provider;
        }

        /// <summary>
        /// Gets or sets the running plugin.
        /// </summary>
        private static Task Plugin { get; set; }

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        private IServiceProvider Provider { get; }

        /// <summary>
        /// Runs the Sound Deck plugin asynchronously.
        /// </summary>
        /// <param name="args">The arguments supplied by the console or entry point.</param>
        /// <returns>The task of running the Sound Deck.</returns>
        public Task RunAsync(string[] args)
        {
            lock (_syncRoot)
            {
                if (Plugin == null)
                {
                    Plugin = this.RunClientIndefinitelyAsync(args);
                }
            }

            return Plugin;
        }

        /// <summary>
        /// Runs the Stream Deck client indefinitely, attempting to recover from exceptions where possible.
        /// </summary>
        /// <param name="clientArgs">The arguments.</param>
        /// <returns>The task of running the client.</returns>
        private async Task RunClientIndefinitelyAsync(string[] clientArgs)
        {
            try
            {
                using (var client = new StreamDeckClient(clientArgs))
                {
                    // register actions
                    client.RegisterAction(ClipAudio.UUID, args => this.Provider.GetInstance<ClipAudio>(args));
                    client.RegisterAction(PlayAudio.UUID, args => this.Provider.GetInstance<PlayAudio>(args));
                    client.RegisterAction(RecordAudio.UUID, args => this.Provider.GetInstance<RecordAudio>(args));

                    // register events
                    client.DeviceDidConnect += this.Client_DeviceDidConnect;
                    client.Error += this.GetErrorEventHandler(client);

                    await client.StartAsync(CancellationToken.None);
                }
            }
            catch
            {
                // restart the client
                await this.RunClientIndefinitelyAsync(clientArgs);
            }
        }

        /// <summary>
        /// Handles the <see cref="IStreamDeckReceiver.DeviceDidConnect"/> event of the main Stream Deck client.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DeviceConnectEventArgs"/> instance containing the event data.</param>
        private async void Client_DeviceDidConnect(object sender, DeviceConnectEventArgs e)
        {
            if (sender is StreamDeckClient client)
            {
                client.DeviceDidConnect -= this.Client_DeviceDidConnect;

                // declare the local function to update the default playback device id
                var devices = this.Provider.GetRequiredService<IAudioService>().Devices;
                Task updateGlobalSettings() => client.SetGlobalSettingsAsync(new PluginSettings(devices.DefaultPlaybackDevice?.Id));

                // set the default settings, and monitor changes
                await updateGlobalSettings();
                devices.DefaultPlaybackDeviceChanged += async (_, __) => await updateGlobalSettings();
            }
        }

        /// <summary>
        /// Gets the error event handler used to log any errors encountered during the lifecycle of the <see cref="StreamDeckClient"/>.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>The event handler</returns>
        private EventHandler<StreamDeckClientErrorEventArgs> GetErrorEventHandler(StreamDeckClient client)
        {
            return async (sender, args) =>
            {
                await client.LogMessageAsync(args.Exception.ToString());
                await client.LogMessageAsync(args.WebSocketMessage);

                if (!string.IsNullOrWhiteSpace(args.Context))
                {
                    await client.ShowAlertAsync(args.Context);
                }
            };
        }
    }
}
