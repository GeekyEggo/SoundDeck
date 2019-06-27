namespace SoundDeck.Plugin.Actions
{
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;
    using SharpDeck.PropertyInspectors;
    using SharpDeck.PropertyInspectors.Payloads;
    using SoundDeck.Core;
    using SoundDeck.Core.Enums;
    using SoundDeck.Core.Playback;
    using SoundDeck.Plugin.Models.Settings;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides an Elgato Stream Deck action for playing an audio clip.
    /// </summary>
    [StreamDeckAction("Play Audio", UUID, "Images/PlayAudio/Action", Tooltip = "Play an audio clip, or three... or more, in the order your heart desires.")]
    [StreamDeckActionState("Images/PlayAudio/Key")]
    public class PlayAudio  : StreamDeckAction<PlayAudioSettings>
    {
        /// <summary>
        /// The unique identifier for the action.
        /// </summary>
        public const string UUID = "com.geekyEggo.soundDeck.playAudio";

        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayAudio"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="args">The <see cref="ActionEventArgs{AppearancePayload}"/> instance containing the event data.</param>
        public PlayAudio(IAudioService audioService, ActionEventArgs<AppearancePayload> args)
        {
            var settings = args.Payload.GetSettings<PlayAudioSettings>();

            this.AudioService = audioService;
            this.Playback = new AudioPlaybackCollection(
                settings?.AudioDeviceId != null ? this.AudioService.GetAudioPlayer(settings.AudioDeviceId) : null,
                settings);
        }

        /// <summary>
        /// Gets the audio service.
        /// </summary>
        private IAudioService AudioService { get; }

        /// <summary>
        /// Gets the playback collection.
        /// </summary>
        private AudioPlaybackCollection Playback { get; }

        /// <summary>
        /// Provides an entry point for the property inspector, which can be used to get the audio devices available on the system.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public Task<OptionsPayload> GetAudioDevices()
        {
            var options = this.AudioService.Devices
                .Where(d => d.Enabled && d.Flow == AudioFlowType.Playback)
                .Select(d => new Option(d.FriendlyName, d.Id));

            return Task.FromResult(new OptionsPayload(options));
        }

        /// <summary>
        /// Raises the <see cref="E:SharpDeck.StreamDeckActionEventReceiver.DidReceiveSettings" /> event.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        /// <returns>The task of updating the state of the object based on the settings.</returns>
        protected override Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args)
        {
            lock (this._syncRoot)
            {
                var settings = args.Payload.GetSettings<PlayAudioSettings>();
                if (this.Playback.Player?.DeviceId != settings.AudioDeviceId)
                {
                    this.Playback.Player?.Dispose();
                    this.Playback.Player = null;
                }

                if (this.Playback.Player == null && !string.IsNullOrWhiteSpace(settings.AudioDeviceId))
                {
                    this.Playback.Player = this.AudioService.GetAudioPlayer(settings.AudioDeviceId);
                }

                this.Playback.SetOptions(settings);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Occurs when the user presses a key.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            await base.OnKeyDown(args);
            await this.Playback.NextAsync();
        }
    }
}
