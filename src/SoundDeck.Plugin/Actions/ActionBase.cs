namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Linq;
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
        /// Gets the audio devices capable of capturing audio.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public OptionsPayload GetCaptureAudioDevices()
        {
            var options = this.AudioService.Devices
                .Where(device => device.Enabled && device.AssignedDefault == DefaultAudioDeviceType.None)
                .GroupBy(device => device.Flow)
                .Select(g =>
                {
                    var children = g.Select(opt => new Option(opt.FriendlyName, opt.Id)).ToList();
                    return new Option(g.Key.ToString(), children);
                });

            return new OptionsPayload(options);
        }

        /// <summary>
        /// Gets the audio devices capable of having an application assigned to them.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public OptionsPayload GetAppAssignableAudioDevices()
            => this.GetPlaybackAudioDevicesInternal(device => device.AssignedDefault != DefaultAudioDeviceType.Communication);

        /// <summary>
        /// Gets the audio devices capable of playback.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public OptionsPayload GetPlaybackAudioDevices()
            => this.GetPlaybackAudioDevicesInternal();

        /// <summary>
        /// Gets the playback audio devices that fulfil the specified <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">The optional filter.</param>
        /// <returns>The payload containing the audio devices.</returns>
        private OptionsPayload GetPlaybackAudioDevicesInternal(Func<AudioDevice, bool> filter = null)
        {
            filter = filter ?? (_ => true);

            var options = this.AudioService.Devices
                .Where(d => d.Enabled && d.Flow == AudioFlowType.Playback)
                .Where(filter)
                .Select(d => new Option(d.FriendlyName, d.Id));

            return new OptionsPayload(options);
        }
    }
}
