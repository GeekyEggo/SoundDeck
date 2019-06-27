namespace SoundDeck.Plugin.Actions
{
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.PropertyInspectors;
    using SharpDeck.PropertyInspectors.Payloads;
    using SoundDeck.Core;
    using SoundDeck.Core.Capture;
    using SoundDeck.Plugin.Models.Payloads;
    using SoundDeck.Plugin.Models.Settings;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    /// <summary>
    /// Provides a base class for capturing audio.
    /// </summary>
    /// <typeparam name="TSettings">The type of the settings.</typeparam>
    public abstract class BaseCaptureAction<TSettings, TCaptureDevice> : StreamDeckAction<TSettings>
        where TSettings : class, ICaptureAudioSettings
        where TCaptureDevice : class, ICaptureDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCaptureAction{TSettings}"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        public BaseCaptureAction(IAudioService audioService)
        {
            this.AudioService = audioService;
        }

        /// <summary>
        /// Gets the audio service.
        /// </summary>
        public IAudioService AudioService { get; }

        /// <summary>
        /// Gets or sets the capture device.
        /// </summary>
        protected TCaptureDevice CaptureDevice { get; set; }

        /// <summary>
        /// Provides an entry point for the property inspector, which can be used to get the audio devices available on the system.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public Task<OptionsPayload> GetAudioDevices()
        {
            var options = this.AudioService.Devices
                .Where(d => d.Enabled)
                .GroupBy(d => d.Flow)
                .Select(g =>
                {
                    var children = g.Select(opt => new Option(opt.FriendlyName, opt.Id)).ToList();
                    return new Option(g.Key.ToString(), children);
                });

            return Task.FromResult(new OptionsPayload(options));
        }

        /// <summary>
        /// Provides an entry point for the property inspector, allowing the user to select an output path.
        /// </summary>
        /// <returns>The payload containing information about the selected output path.</returns>
        [PropertyInspectorMethod]
        public async Task<FolderPickerPayload> GetOutputPath()
        {
            var settings = await this.GetSettingsAsync();
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Output Path";
                dialog.SelectedPath = settings.OutputPath;
                dialog.UseDescriptionForTitle = true;

                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    settings.OutputPath = dialog.SelectedPath;
                    await this.SetSettingsAsync(settings);

                    return new FolderPickerPayload(settings.OutputPath, true);
                }
            }

            return new FolderPickerPayload(settings?.OutputPath, false);
        }

        /// <summary>
        /// Gets the capture device, for the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The capture device.</returns>
        protected abstract TCaptureDevice GetCaptureDevice(TSettings settings);

        /// <summary>
        /// Handles the <see cref="StreamDeckActionEventReceiver.DidReceiveSettings" /> event.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs{TActionPayload}" /> instance containing the event data.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>The task of updating the state of the object based on the settings.</returns>
        protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, TSettings settings)
        {
            await base.OnDidReceiveSettings(args, settings);
            if (this.CaptureDevice?.DeviceId != settings.AudioDeviceId)
            {
                this.CaptureDevice?.Dispose();
                this.CaptureDevice = null;
            }

            if (this.CaptureDevice == null)
            {
                this.CaptureDevice = this.GetCaptureDevice(settings);
            }
        }
    }
}
