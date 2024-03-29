namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides an action for setting the default audio device for a process.
    /// </summary>
    [StreamDeckAction("com.geekyeggo.sounddeck.setappaudiodevice")]
    public class SetAppAudioDevice : AppActionBase<SetAppAudioDeviceSettings>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetAppAudioDevice" /> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="appAudioService">The application audio service.</param>
        public SetAppAudioDevice(IAudioService audioService, IAppAudioService appAudioService)
            : base(audioService, appAudioService)
        {
        }

        /// <summary>
        /// Occurs when <see cref="IStreamDeckConnection.KeyDown" /> is received for this instance.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs" /> instance containing the event data.</param>
        /// <returns>The task of handling the event.</returns>
        protected async override Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            var settings = args.Payload.GetSettings<SetAppAudioDeviceSettings>();

            try
            {
                this.AppAudioService.SetDefaultAudioDevice(settings, settings.AudioDeviceId);
                await this.ShowOkAsync();
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, $"Failed to set app audio device; AudioDeviceId=\"{settings.AudioDeviceId}\", ProcessSelectionType=\"{settings.ProcessSelectionType}\", ProcessName=\"{settings.ProcessName}\".");
                await this.ShowAlertAsync();
            }
        }
    }
}
