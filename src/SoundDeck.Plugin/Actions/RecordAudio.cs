namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SoundDeck.Core;
    using SoundDeck.Core.Capture;
    using SoundDeck.Plugin.Models;
    using SoundDeck.Plugin.Models.Settings;
    using SoundDeck.Plugin.Models.UI;

    /// <summary>
    /// Provides an action for recording audio via start/stop.
    /// </summary>
    [StreamDeckAction("com.geekyeggo.sounddeck.recordaudio")]
    public class RecordAudio : CaptureActionBase<RecordAudioSettings, IAudioRecorder>
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordAudio"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="folderBrowserDialogProvider">The folder browser dialog.</param>
        /// <param name="args">The <see cref="ActionEventArgs{AppearancePayload}"/> instance containing the event data.</param>
        public RecordAudio(IAudioService audioService, IFolderBrowserDialogProvider folderBrowserDialogProvider)
            : base(audioService, folderBrowserDialogProvider)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is initialized.
        /// </summary>
        private bool IsInitialized { get; set; }

        /// <summary>
        /// Occurs when an instance of an action appears.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
        {
            if (!this.IsInitialized)
            {
                this.IsInitialized = true;
                await this.SetStateAsync(0);
            }
        }

        /// <summary>
        /// Gets the capture device, for the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The capture device.</returns>
        protected override IAudioRecorder GetCaptureDevice(RecordAudioSettings settings)
        {
            return !string.IsNullOrWhiteSpace(settings?.CaptureAudioDeviceId)
                ? this.AudioService.GetAudioRecorder(settings.CaptureAudioDeviceId)
                : null;
        }

        /// <summary>
        /// Occurs when the user presses a key.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs{KeyPayload}" /> instance containing the event data.</param>
        /// <returns>The task of the key down.</returns>
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            try
            {
                await this._syncRoot.WaitAsync();

                switch (args.Payload.State)
                {
                    case RecordAudioState.START:
                        this.CaptureDevice.Settings = args.Payload.GetSettings<RecordAudioSettings>();
                        await this.CaptureDevice.StartAsync();
                        break;

                    case RecordAudioState.STOP:
                        await this.CaptureDevice.StopAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Recording audio failed.");
                await this.ShowAlertAsync();
            }
            finally
            {
                this._syncRoot.Release();
            }
        }
    }
}
