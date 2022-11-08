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
    /// Provides an action that is capable of controlling media for a specific app.
    /// </summary>
    [StreamDeckAction("com.geekyeggo.sounddeck.appmultimediacontrols")]
    public class AppMultimediaControls : AppActionBase<AppMultimediaControlsSettings>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppMultimediaControls" /> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="appAudioService">The application audio service.</param>
        public AppMultimediaControls(IAudioService audioService, IAppAudioService appAudioService)
           : base(audioService, appAudioService)
        {
        }

        /// <summary>
        /// Occurs when <see cref="SharpDeck.Connectivity.IStreamDeckConnection.KeyDown" /> is received for this instance.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs`1" /> instance containing the event data.</param>
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            var settings = args.Payload.GetSettings<AppMultimediaControlsSettings>();

            try
            {
                await this.AppAudioService.ControlAsync(settings, settings.Action);
                await this.ShowOkAsync();
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, $"Failed to control application's multimedia; Action=\"{settings.Action}\", ProcessSelectionType=\"{settings.ProcessSelectionType}\", ProcessName=\"{settings.ProcessName}\".");
                await this.ShowAlertAsync();
            }
        }
    }
}
