namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using SharpDeck.Events.Received;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Models.Payloads;

    /// <summary>
    /// Provides a base class for actions that interact with applications.
    /// </summary>
    /// <typeparam name="TSettings">The type of the settings.</typeparam>
    public class AppActionBase<TSettings> : ActionBase<TSettings>
        where TSettings : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppActionBase{TSettings}"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        public AppActionBase(IAudioService audioService, IAppAudioService appAudioService)
            : base(audioService) => this.AppAudioService = appAudioService;

        /// <summary>
        /// Gets the application audio service.
        /// </summary>
        protected IAppAudioService AppAudioService { get; }

        /// <inheritdoc/>
        protected override async Task OnSendToPlugin(ActionEventArgs<JObject> args)
        {
            await base.OnSendToPlugin(args);

            var payload = args.Payload.ToObject<DataSourcePayload>();
            switch (payload.Event)
            {
                case "getAudioSessions":
                    await this.SendProcessOptions(payload.Event, this.GetAudioSessions());
                    break;
                case "getAudioSessionsOnly":
                    await this.SendProcessOptions(payload.Event, this.GetAudioSessions(), allowUserInput: false);
                    break;
                case "getMultimediaSessions":
                    await this.SendProcessOptions(payload.Event, await this.GetMultimediaSessions());
                    break;
            }
        }

        /// <summary>
        /// Gets the current active audio sessions.
        /// </summary>
        /// <returns>The current audio sessions.</returns>
        private IReadOnlyList<DataSourceItem> GetAudioSessions()
        {
            var sessions = new Dictionary<string, string>();
            foreach (var session in this.AppAudioService.GetAudioSessions())
            {
                if (session.GetProcessID == 0)
                {
                    continue;
                }

                var process = Process.GetProcessById((int)session.GetProcessID);
                if (!sessions.ContainsKey(process.ProcessName))
                {
                    try
                    {
                        sessions.Add(process.ProcessName, process.MainModule.FileVersionInfo.FileDescription);
                    }
                    catch (Win32Exception)
                    {
                        sessions.Add(process.ProcessName, process.ProcessName);
                    }
                }
            }

            return sessions
                .OrderBy(opt => opt.Key)
                .Select(opt => new DataSourceItem(opt.Key, opt.Value))
                .ToArray();
        }

        /// <summary>
        /// Gets the current active multimedia sessions.
        /// </summary>
        /// <returns>The current multimedia sessions.</returns>
        private async Task<IReadOnlyList<DataSourceItem>> GetMultimediaSessions()
        {
            var sessions = new Dictionary<string, string>();
            foreach (var session in await this.AppAudioService.GetMultimediaSessionsAsync())
            {
                if (!sessions.ContainsKey(session.SourceAppUserModelId))
                {
                    AppInfoUtils.TryGet(session.SourceAppUserModelId, out var appInfo);
                    sessions.Add(session.SourceAppUserModelId, appInfo?.DisplayInfo?.DisplayName ?? session.SourceAppUserModelId);
                }
            }

            return sessions
                .OrderBy(opt => opt.Value)
                .Select(opt => new DataSourceItem(opt.Key, opt.Value))
                .ToArray();
        }

        /// <summary>
        /// Sends the process options to the property inspector.
        /// </summary>
        /// <param name="eventName">Name of the event that requested the data source.</param>
        /// <param name="sessions">The sessions.</param>
        /// <param name="allowUserInput">Determines whether "Foreground", and "By Name" should be displayed.</param>
        private async Task SendProcessOptions(string eventName, IReadOnlyList<DataSourceItem> sessions, bool allowUserInput = true)
        {
            var items = new List<DataSourceItem>();

            // Add the default items.
            if (allowUserInput)
            {
                items.Add(new DataSourceItem("0", "Foreground (Active)"));
                items.Add(new DataSourceItem("1", "By Name"));
            }

            // Add the active sessions if we have any.
            if (sessions.Count > 0)
            {
                if (allowUserInput)
                {
                    items.Add(new DataSourceItem("Apps", sessions));
                }
                else
                {
                    items.AddRange(sessions);
                }
            }

            // Return the items.
            var response = new DataSourceResponse(eventName, items);
            await this.SendToPropertyInspectorAsync(response);
        }
    }
}
