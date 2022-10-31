namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Models.Payloads;
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
            : base(audioService) => this.AppAudioService = appAudioService;

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

        /// <inheritdoc/>
        protected override async Task OnSendToPlugin(ActionEventArgs<JObject> args)
        {
            _ = base.OnSendToPlugin(args);

            var payload = args.Payload.ToObject<DataSourcePayload>();
            if (payload.Event == "getAudioSessions")
            {
                await this.SendProcessOptions(payload.Event, "Apps", this.GetAudioSessions());
            }
        }

        /// <summary>
        /// Gets the current active audio sessions.
        /// </summary>
        /// <returns>The current audio sessions.</returns>
        private IReadOnlyList<DataSourceItem> GetAudioSessions()
        {
            var sessions = new HashSet<string>();
            foreach (var session in this.AppAudioService.GetAudioSessions())
            {
                if (session.GetProcessID == 0)
                {
                    continue;
                }

                var process = Process.GetProcessById((int)session.GetProcessID);
                if (!sessions.Contains(process.ProcessName))
                {
                    sessions.Add(process.ProcessName);
                }
            }

            return sessions
                .OrderBy(opt => opt)
                .Select(opt => new DataSourceItem(opt, opt))
                .ToArray();
        }
    }
}
