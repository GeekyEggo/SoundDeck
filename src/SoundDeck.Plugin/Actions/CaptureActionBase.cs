namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SharpDeck.Events.Received;
    using SharpDeck.PropertyInspectors;
    using SoundDeck.Core;
    using SoundDeck.Core.Capture;
    using SoundDeck.Plugin.Models.Payloads;
    using SoundDeck.Plugin.Models.Settings;
    using SoundDeck.Plugin.Models.UI;

    /// <summary>
    /// Provides a base class for capturing audio.
    /// </summary>
    /// <typeparam name="TSettings">The type of the settings.</typeparam>
    public abstract class CaptureActionBase<TSettings, TCaptureDevice> : ActionBase<TSettings>
        where TSettings : class, ICaptureAudioSettings
        where TCaptureDevice : class, ICaptureDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCaptureAction{TSettings}"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="folderBrowserDialogProvider">The folder browser dialog.</param>
        public CaptureActionBase(IAudioService audioService, IFolderBrowserDialogProvider folderBrowserDialogProvider)
            : base(audioService)
        {
            this.FolderBrowserDialogProvider = folderBrowserDialogProvider;
        }

        /// <summary>
        /// Gets the folder browser dialog provider.
        /// </summary>
        public IFolderBrowserDialogProvider FolderBrowserDialogProvider { get; }

        /// <summary>
        /// Gets or sets the capture device.
        /// </summary>
        protected TCaptureDevice CaptureDevice { get; set; }

        /// <summary>
        /// Provides an entry point for the property inspector, allowing the user to select an output path.
        /// </summary>
        /// <returns>The payload containing information about the selected output path.</returns>
        [PropertyInspectorMethod]
        public async Task<FolderPickerPayload> GetOutputPath()
        {
            var settings = await this.GetSettingsAsync();

            var result = this.FolderBrowserDialogProvider.ShowDialog("Output Path", settings.OutputPath);
            if (result.IsSelected)
            {
                settings.OutputPath = result.SelectedPath;
                await this.SetSettingsAsync(settings);
            }

            return new FolderPickerPayload(settings?.OutputPath, result.IsSelected);
        }

        /// <summary>
        /// Gets the capture device, for the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The capture device.</returns>
        protected abstract TCaptureDevice GetCaptureDevice(TSettings settings);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            this.CaptureDevice?.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Handles the <see cref="StreamDeckActionEventReceiver.DidReceiveSettings" /> event.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs{TActionPayload}" /> instance containing the event data.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>The task of updating the state of the object based on the settings.</returns>
        protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, TSettings settings)
        {
            await base.OnDidReceiveSettings(args, settings);
            if (this.CaptureDevice?.Device.Key != settings.CaptureAudioDeviceId)
            {
                this.CaptureDevice?.Dispose();
                this.CaptureDevice = null;
            }

            if (this.CaptureDevice == null)
            {
                this.CaptureDevice = this.TryGetCaptureDevice(settings);
            }
        }

        /// <summary>
        /// Occurs when this instance is initialized.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        /// <param name="settings">The settings.</param>
        protected override void OnInit(ActionEventArgs<AppearancePayload> args, TSettings settings)
        {
            base.OnInit(args, settings);
            this.CaptureDevice = this.TryGetCaptureDevice(settings);
        }

        /// <summary>
        /// Attempts to get the capture device; otherwise logs the exception and shows an error on the Stream Deck.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The capture device; otherwise <c>false</c>.</returns>
        private TCaptureDevice TryGetCaptureDevice(TSettings settings)
        {
            try
            {
                return this.GetCaptureDevice(settings);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, $"Failed to get capture device \"{settings.CaptureAudioDeviceId}\".");
                _ = this.ShowAlertAsync();
            }

            return null;
        }
    }
}
