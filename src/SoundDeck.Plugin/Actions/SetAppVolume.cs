﻿namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides an action that is capable of changing the volume of an application.
    /// </summary>
    [StreamDeckAction("com.geekyeggo.sounddeck.setappvolume")]
    public class SetAppVolume : AppActionBase<SetAppVolumeSettings>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetAppVolume"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="appAudioService">The application audio service.</param>
        public SetAppVolume(IAudioService audioService, IAppAudioService appAudioService)
            : base(audioService, appAudioService)
        {
        }

        /// <inheritdoc/>
        protected async override Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            var settings = args.Payload.GetSettings<SetAppVolumeSettings>();

            try
            {
                this.AppAudioService.SetVolume(settings);
                await this.ShowOkAsync();
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, $"Failed to set app audio volume; Action=\"{settings.VolumeAction}\", Value=\"{settings.VolumeValue}\", ProcessSelectionType=\"{settings.ProcessSelectionType}\", ProcessName=\"{settings.ProcessName}\".");
                await this.ShowAlertAsync();
            }
        }
    }
}
