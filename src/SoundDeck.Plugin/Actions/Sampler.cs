namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;
    using SoundDeck.Core;
    using SoundDeck.Core.Capture;
    using SoundDeck.Core.Playback;
    using SoundDeck.Plugin.Models.Settings;
    using SoundDeck.Plugin.Models.UI;

    /// <summary>
    /// Provides a sampler action used to record audio, and then play it back.
    /// </summary>
    [StreamDeckAction("Sampler", UUID, "Images/PlayAudio/Action", Tooltip = "Record and playback samples!")]
    [StreamDeckActionState("Images/PlayAudio/Key")]
    public class Sampler : BaseCaptureAction<SamplerSettings, IAudioRecorder>
    {
        /// <summary>
        /// The unique identifier for the action.
        /// </summary>
        public const string UUID = "com.geekyeggo.sounddeck.sampler";

        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

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
        /// Occurs when <see cref="IStreamDeckConnection.KeyDown" /> is received for this instance.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs{TPayload}" /> instance containing the event data.</param>
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            try
            {
                await this._syncRoot.WaitAsync();

                if (this.CaptureDevice != null)
                {
                    this.CaptureDevice.Settings = args.Payload.GetSettings<SamplerSettings>();
                    await this.CaptureDevice?.StartAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Occurs when <see cref="IStreamDeckConnection.KeyUp" /> is received for this instance.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs{TPayload}" /> instance containing the event data.</param>
        protected override async Task OnKeyUp(ActionEventArgs<KeyPayload> args)
        {
            try
            {
                await this._syncRoot.WaitAsync();
                var filename = await this.CaptureDevice?.StopAsync();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                this._syncRoot.Release();
            }
        }
    }
}
