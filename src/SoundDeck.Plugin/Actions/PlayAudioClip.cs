namespace SoundDeck.Plugin.Actions
{
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;
    using SharpDeck.PropertyInspectors;
    using SharpDeck.PropertyInspectors.Payloads;
    using SoundDeck.Core;
    using SoundDeck.Core.Enums;
    using SoundDeck.Plugin.Models.Settings;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides an Elgato Stream Deck action for playing an audio clip.
    /// </summary>
    [StreamDeckAction("Play Audio", UUID, "Images/Action", Tooltip = "Play an audio clip.")]
    [StreamDeckActionState("Images/Action")]
    public class PlayAudioClip  : StreamDeckAction<PlayAudioClipSettings>
    {
        /// <summary>
        /// The unique identifier for the action.
        /// </summary>
        public const string UUID = "com.geekyEggo.soundDeck.playAudioClip";

        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayAudioClip"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="args">The <see cref="ActionEventArgs{AppearancePayload}"/> instance containing the event data.</param>
        public PlayAudioClip(IAudioService audioService, ActionEventArgs<AppearancePayload> args)
        {
            var settings = args.Payload.GetSettings<PlayAudioClipSettings>();

            this.AudioService = audioService;
            if (settings?.AudioDeviceId != null)
            {
                this.AudioPlayer = this.AudioService.GetAudioPlayer(settings.AudioDeviceId);
            }
        }

        /// <summary>
        /// Gets the audio service.
        /// </summary>
        private IAudioService AudioService { get; }

        /// <summary>
        /// Gets or sets the audio player.
        /// </summary>
        private IAudioPlayer AudioPlayer { get; set; }

        /// <summary>
        /// Provides an entry point for the property inspector, which can be used to get the audio devices available on the system.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public Task<OptionsPayload> GetAudioDevices()
        {
            var options = this.AudioService.Devices
                .Where(d => d.Enabled && d.Flow == AudioFlow.Playback)
                .Select(d => new Option(d.FriendlyName, d.Id));

            return Task.FromResult(new OptionsPayload(options));
        }

        /// <summary>
        /// Raises the <see cref="E:SharpDeck.StreamDeckActionEventReceiver.DidReceiveSettings" /> event.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        /// <returns>The task of updating the state of the object based on the settings.</returns>
        protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args)
        {
            try
            {
                await this._syncRoot.WaitAsync();

                var settings = args.Payload.GetSettings<PlayAudioClipSettings>();
                if (this.AudioPlayer?.DeviceId != settings.AudioDeviceId)
                {
                    this.AudioPlayer?.Dispose();
                    this.AudioPlayer = null;
                }

                if (this.AudioPlayer == null && !string.IsNullOrWhiteSpace(settings.AudioDeviceId))
                {
                    this.AudioPlayer = this.AudioService.GetAudioPlayer(settings.AudioDeviceId);
                }
            }
            finally
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Occurs when the user presses a key.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            await base.OnKeyDown(args);

            try
            {
                await this._syncRoot.WaitAsync();
                if (this.AudioPlayer != null)
                {
                    this.AudioPlayer.Stop();

                    var settings = args.Payload.GetSettings<PlayAudioClipSettings>();
                    //if (!string.IsNullOrWhiteSpace(settings.File))
                    //{
                    //    await this.AudioPlayer.PlayAsync(settings.File, CancellationToken.None);
                    //}
                }
            }
            finally
            {
                this._syncRoot.Release();
            }
        }
    }
}