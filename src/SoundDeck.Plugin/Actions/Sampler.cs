namespace SoundDeck.Plugin.Actions
{
    using System.Threading.Tasks;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;
    using SoundDeck.Core;
    using SoundDeck.Core.Capture;
    using SoundDeck.Core.Playback;
    using SoundDeck.Plugin.Contracts;
    using SoundDeck.Plugin.Extensions;
    using SoundDeck.Plugin.Models.Settings;
    using SoundDeck.Plugin.Models.UI;

    /// <summary>
    /// Provides a sampler action used to record audio, and then play it back.
    /// </summary>
    [StreamDeckAction("com.geekyeggo.sounddeck.sampler")]
    [StreamDeckActionState("Images/Sampler/Key0")]
    [StreamDeckActionState("Images/Sampler/Key1")]
    public class Sampler : CaptureActionBase<SamplerSettings, IAudioRecorder>, IPlayAudioAction
    {
        /// <summary>
        /// Represents the state at which the action will record a sample.
        /// </summary>
        private const int RECORD_STATE = 0;

        /// <summary>
        /// Represents the state at which the action will play the sample.
        /// </summary>
        private const int PLAY_STATE = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sampler"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="folderBrowserDialogProvider">The folder browser dialog.</param>
        public Sampler(IAudioService audioService, IFolderBrowserDialogProvider folderBrowserDialogProvider)
            : base(audioService, folderBrowserDialogProvider)
        {
        }

        /// <summary>
        /// Gets or sets the player.
        /// </summary>
        public IPlaylistPlayer Player { get; set; }

        /// <summary>
        /// Gets or sets the playlist.
        /// </summary>
        public Playlist Playlist { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is initialized.
        /// </summary>
        private bool IsInitialized { get; set; } = false;

        /// <summary>
        /// Gets the capture device, for the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The capture device.</returns>
        protected override IAudioRecorder GetCaptureDevice(SamplerSettings settings)
        {
            return !string.IsNullOrWhiteSpace(settings?.CaptureAudioDeviceId)
                ? this.AudioService.GetAudioRecorder(settings.CaptureAudioDeviceId)
                : null;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            this.Player?.Dispose();
            this.SetTitleAsync();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Occurs when an instance of an action appears.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
        {
            if (!this.IsInitialized)
            {
                this.IsInitialized = true;

                var state = string.IsNullOrWhiteSpace(args.Payload.GetSettings<SamplerSettings>().FilePath) ? RECORD_STATE : PLAY_STATE;
                await this.SetStateAsync(state);
            }
        }

        /// <summary>
        /// Handles the <see cref="DidReceiveSettings" /> event.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs`1" /> instance containing the event data.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>The task of updating the state of the object based on the settings.</returns>
        protected override Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, SamplerSettings settings)
        {
            this.SetPlayerSettings(settings);
            return base.OnDidReceiveSettings(args, settings);
        }

        /// <summary>
        /// Occurs when this instance is initialized.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        /// <param name="settings">The settings.</param>
        protected override void OnInit(ActionEventArgs<AppearancePayload> args, SamplerSettings settings)
        {
            base.OnInit(args, settings);
            this.SetPlayerSettings(settings);
        }

        /// <summary>
        /// Occurs when <see cref="IStreamDeckConnection.KeyDown" /> is received for this instance.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs{TPayload}" /> instance containing the event data.</param>
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            await base.OnKeyDown(args);
            var settings = args.Payload.GetSettings<SamplerSettings>();

            // determine if clearing is active
            if (SamplerClearer.IsActive)
            {
                this.Player.Stop();
                if (!string.IsNullOrWhiteSpace(settings.FilePath))
                {
                    settings.FilePath = string.Empty;
                    await this.SetSettingsAsync(settings);
                }

                return;
            }

            // when there is a file, play it
            if (!string.IsNullOrWhiteSpace(settings.FilePath))
            {
                await this.Player.NextAsync();
                return;
            }

            // otherwise, lets check if we can capture
            if (this.CaptureDevice != null)
            {
                this.CaptureDevice.Settings = settings;
                await this.CaptureDevice?.StartAsync();
            }
        }

        /// <summary>
        /// Occurs when <see cref="IStreamDeckConnection.KeyUp" /> is received for this instance.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs{TPayload}" /> instance containing the event data.</param>
        protected override async Task OnKeyUp(ActionEventArgs<KeyPayload> args)
        {
            await base.OnKeyUp(args);
            var settings = args.Payload.GetSettings<SamplerSettings>();

            // clearing, do nothing
            if (SamplerClearer.IsActive)
            {
                await this.SetStateAsync(RECORD_STATE);
                SamplerClearer.SetIsActive(false, this);

                await this.ShowOkAsync();
                return;
            }

            // no audio file, so finish capturing
            if (string.IsNullOrWhiteSpace(settings.FilePath))
            {
                // save the capture, and settings
                settings.FilePath = await this.CaptureDevice?.StopAsync();
                await this.SetSettingsAsync(settings);
                this.Playlist.SetOptions(settings);

                await this.ShowOkAsync();
                return;
            }

            // reset to play audio state
            await this.SetStateAsync(PLAY_STATE);
        }
    }
}
