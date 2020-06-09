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
    [StreamDeckAction("Sampler", UUID, "Images/PlayAudio/Action", Tooltip = "Record and playback samples!")]
    [StreamDeckActionState("Images/PlayAudio/Key")]
    public class Sampler : CaptureActionBase<SamplerSettings, IAudioRecorder>, IPlayAudioAction
    {
        /// <summary>
        /// The unique identifier for the action.
        /// </summary>
        public const string UUID = "com.geekyeggo.sounddeck.sampler";

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
        /// Handles the <see cref="DidReceiveSettings" /> event.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs`1" /> instance containing the event data.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>The task of updating the state of the object based on the settings.</returns>
        protected override Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, SamplerSettings settings)
        {
            this.SetPlayerSettings(settings, this.SetTitleAsync);
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
            this.SetPlayerSettings(settings, this.SetTitleAsync);
        }

        /// <summary>
        /// Occurs when <see cref="IStreamDeckConnection.KeyDown" /> is received for this instance.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs{TPayload}" /> instance containing the event data.</param>
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            await base.OnKeyDown(args);
            var settings = args.Payload.GetSettings<SamplerSettings>();

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

            if (string.IsNullOrWhiteSpace(settings.FilePath))
            {
                // save the capture, and settings
                settings.FilePath = await this.CaptureDevice?.StopAsync();
                await this.SetSettingsAsync(settings);
                this.Playlist.SetOptions(settings);

                await this.ShowOkAsync();
            }
        }
    }
}
