namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Threading.Tasks;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;
    using SoundDeck.Core;
    using SoundDeck.Core.Enums;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides an action for setting the default audio device for a process.
    /// </summary>
    [StreamDeckAction("Set App Output", "com.geekyeggo.sounddeck.setappaudio", "Images/SetAppAudio/Action", Tooltip = "Set the default audio device for an application.")]
    [StreamDeckActionState("Images/SetAppAudio/Key")]
    public class SetAppAudio : ActionBase<SetAppAudioSettings>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetAppAudio" /> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="appAudioService">The application audio service.</param>
        public SetAppAudio(IAudioService audioService, IAppAudioService appAudioService)
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
                var settings = args.Payload.GetSettings<SetAppAudioSettings>();

                // Ensure we have an audio device.
                if (string.IsNullOrWhiteSpace(settings.AudioDeviceId))
                {
                    throw new ArgumentNullException($"Cannot set default audio device for app: The audio device has not been specified.");
                }

                if (settings.ProcessSelectionType == ProcessSelectionType.ByName)
                {
                    // When the process selection type is by name, validate we have a name and then set the default audio device.
                    if (string.IsNullOrWhiteSpace(settings.ProcessName))
                    {
                        throw new ArgumentNullException($"Cannot set default audio device for app: The process name has not been specified.");
                    }

                    this.AppAudioService.SetDefaultAudioDevice(settings.ProcessName, AudioFlowType.Playback, settings.AudioDeviceId);
                }
                else
                {
                    // The process selection type is foreground, so select the process id and set the default audio device.
                    var processId = this.AppAudioService.GetForegroundAppProcessId();
                    this.AppAudioService.SetDefaultAudioDevice(processId, AudioFlowType.Playback, settings.AudioDeviceId);
                }

                await this.ShowOkAsync();
            }
            catch (Exception ex)
            {
                _ = this.StreamDeck.LogMessageAsync(ex.Message);
                await this.ShowAlertAsync();
            }
        }
    }
}
