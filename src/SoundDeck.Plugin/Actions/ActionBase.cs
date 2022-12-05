namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using NAudio.CoreAudioApi;
    using Newtonsoft.Json.Linq;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.PropertyInspectors;
    using SharpDeck.PropertyInspectors.Payloads;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Models.Payloads;

    /// <summary>
    /// Provides a base action.
    /// </summary>
    /// <typeparam name="TSettings">The type of the settings.</typeparam>
    /// <seealso cref="StreamDeckAction{TSettings}" />
    public abstract class ActionBase<TSettings> : StreamDeckAction<TSettings>
        where TSettings : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionBase{TSettings}"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        public ActionBase(IAudioService audioService)
            => this.AudioService = audioService;

        /// <summary>
        /// Gets the audio service.
        /// </summary>
        public IAudioService AudioService { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is an encoder.
        /// </summary>
        protected bool IsEncoder { get; set; } = false;

        /// <summary>
        /// Gets the audio devices capable of capturing audio.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public DataSourceItem[] GetAudioDevices()
            => this.GetAudioDevices(_ => true);

        /// <summary>
        /// Gets the audio devices that are capable of being assigned as a the default audio device.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public DataSourceItem[] GetDefaultAssignableAudioDevices()
            => this.GetAudioDevices(device => !device.IsDynamic);

        /// <summary>
        /// Gets the audio devices capable of playback.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public Option[] GetPlaybackAudioDevices()
        {
            return this.AudioService.Devices
                .Where(d => d.Flow == DataFlow.Render)
                .Select(d => new Option(d.FriendlyName, d.Key))
                .ToArray();
        }

        /// <inheritdoc/>
        protected override async Task OnSendToPlugin(ActionEventArgs<JObject> args)
        {
            await base.OnSendToPlugin(args);

            var payload = args.Payload.ToObject<DataSourcePayload>();
            switch (payload.Event)
            {
                case "getAppAssignableAudioDevices":
                    await this.SendToPropertyInspectorAsync(new DataSourceResponse(payload.Event, this.GetAudioDevices(device => device.Role != Role.Communications)));
                    break;
                case "getAudioDevices":
                    await this.SendToPropertyInspectorAsync(new DataSourceResponse(payload.Event, this.GetAudioDevices()));
                    break;
            }
        }

        /// <inheritdoc/>
        protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
        {
            await base.OnWillAppear(args);
            this.IsEncoder = args.Payload.Controller is Controller.Encoder;
        }

        /// <summary>
        /// Gets the audio devices that match the specified <paramref name="filter"/>; otherwise all.
        /// </summary>
        /// <param name="filter">The optional filter.</param>
        /// <returns>The audio devices. </returns>
        private DataSourceItem[] GetAudioDevices(Func<IAudioDevice, bool> filter)
        {
            return this.AudioService.Devices
                .Where(filter)
                .GroupBy(device => device.Flow)
                .Select(group =>
                {
                    var children = group
                        .OrderByDescending(x => x.IsDynamic)
                        .ThenBy(x => x.FriendlyName)
                        .Select(opt => new DataSourceItem(opt.Key, opt.FriendlyName))
                        .ToArray();

                    return new DataSourceItem(group.Key == DataFlow.Render ? "Playback" : "Recording", children);
                }).ToArray();
        }
    }
}
