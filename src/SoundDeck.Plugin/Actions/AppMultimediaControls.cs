namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::Windows.ApplicationModel;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Models.Payloads;
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
           : base(audioService) => this.AppAudioService = appAudioService;

        /// <summary>
        /// Gets the application audio service.
        /// </summary>
        private IAppAudioService AppAudioService { get; }

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

        /// <inheritdoc/>
        protected override async Task OnSendToPlugin(ActionEventArgs<JObject> args)
        {
            await base.OnSendToPlugin(args);

            var payload = args.Payload.ToObject<DataSourcePayload>();
            if (payload.Event == "getMultimediaSessions")
            {
                await this.SendProcessOptions(payload.Event, "Active Apps", await this.GetMultimediaSessions());
            }
        }

        /// <summary>
        /// Gets the current active multimedia sessions.
        /// </summary>
        /// <returns>The current multimedia sessions.</returns>
        private async Task<IReadOnlyList<DataSourceItem>> GetMultimediaSessions()
        {
            var sessions = new Dictionary<string, string>();
            foreach (var session in await this.AppAudioService.GetMultimediaSessionAsync())
            {
                if (!sessions.ContainsKey(session.SourceAppUserModelId))
                {
                    try
                    {
                        sessions.Add(session.SourceAppUserModelId, AppInfo.GetFromAppUserModelId(session.SourceAppUserModelId).DisplayInfo.DisplayName);
                    }
                    catch
                    {
                        sessions.Add(session.SourceAppUserModelId, session.SourceAppUserModelId);
                    }
                }
            }

            return sessions
                .OrderBy(opt => opt.Value)
                .Select(opt => new DataSourceItem(opt.Key, opt.Value))
                .ToArray();
        }
    }
}
