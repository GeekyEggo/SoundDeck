namespace SoundDeck.Plugin.Actions
{
    using System.Linq;
    using System.Threading.Tasks;
    using SharpDeck;
    using SharpDeck.PropertyInspectors;
    using SharpDeck.PropertyInspectors.Payloads;
    using SoundDeck.Core;
    using SoundDeck.Core.Interop;

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
        {
            this.AudioService = audioService;
        }

        /// <summary>
        /// Gets the audio service.
        /// </summary>
        public IAudioService AudioService { get; }

        /// <summary>
        /// Provides an entry point for the property inspector, which can be used to get the audio devices available on the system.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public Task<OptionsPayload> GetCaptureAudioDevices()
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
        /// Provides an entry point for the property inspector, which can be used to get the audio devices available on the system.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public Task<OptionsPayload> GetPlaybackAudioDevices()
        {
            var options = this.AudioService.Devices
                .Where(d => d.Enabled && d.Flow == AudioFlowType.Playback)
                .Select(d => new Option(d.FriendlyName, d.Id));

            return Task.FromResult(new OptionsPayload(options));
        }
    }
}
