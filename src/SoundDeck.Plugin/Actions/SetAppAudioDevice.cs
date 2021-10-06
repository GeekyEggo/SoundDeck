namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Threading.Tasks;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SoundDeck.Core;
    using SoundDeck.Core.Interop;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides an action for setting the default audio device for a process.
    /// </summary>
    [StreamDeckAction("com.geekyeggo.sounddeck.setappaudiodevice")]
    public class SetAppAudioDevice : ActionBase<SetAppAudioDeviceSettings>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetAppAudioDevice" /> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="appAudioService">The application audio service.</param>
        public SetAppAudioDevice(IAudioService audioService, IAppAudioService appAudioService)
            : base(audioService)
        {
            this.AppAudioService = appAudioService;
        }

        /// <summary>
        /// Gets the application audio service.
        /// </summary>
        private IAppAudioService AppAudioService { get; }

        /// <summary>
        /// Occurs when <see cref="IStreamDeckConnection.KeyDown" /> is received for this instance.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs" /> instance containing the event data.</param>
        /// <returns>The task of handling the event.</returns>
        protected async override Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            await base.OnKeyDown(args);

            try
            {
                var settings = args.Payload.GetSettings<SetAppAudioDeviceSettings>();

                // When the process selection type is by name, validate we have a name and then set the default audio device.
                if (settings.ProcessSelectionType == ProcessSelectionType.ByName)
                {
                    if (string.IsNullOrWhiteSpace(settings.ProcessName))
                    {
                        throw new ArgumentNullException($"Cannot set default audio device for app: The process name has not been specified.");
                    }

                    this.AppAudioService.SetDefaultAudioDevice(settings.ProcessName, AudioFlowType.Playback, settings.AudioDeviceId);
                }
                else
                {
                    // The process selection type is foreground, so select the process id and set the default audio device.
                    this.AppAudioService.SetDefaultAudioDeviceForForegroundApp(AudioFlowType.Playback, settings.AudioDeviceId);
                }

                await this.ShowOkAsync();
            }
            catch (Exception ex)
            {
                _ = this.Connection.LogMessageAsync(ex.Message);
                await this.ShowAlertAsync();
            }
        }
    }
}
