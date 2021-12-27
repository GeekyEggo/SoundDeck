namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Linq;
    using NAudio.CoreAudioApi;
    using SharpDeck;
    using SharpDeck.PropertyInspectors;
    using SharpDeck.PropertyInspectors.Payloads;
    using SoundDeck.Core;

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
        /// Gets the audio devices capable of capturing audio.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public OptionsPayload GetAudioDevices()
            => this.GetAudioDevices(_ => true);

        /// <summary>
        /// Gets the audio devices capable of having an application assigned to them.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public OptionsPayload GetAppAssignableAudioDevices()
            => this.GetAudioDevices(device => device.Role != Role.Communications);

        /// <summary>
        /// Gets the audio devices that are capable of being assigned as a the default audio device.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public OptionsPayload GetDefaultAssignableAudioDevices()
            => this.GetAudioDevices(device => !device.IsDynamic);

        /// <summary>
        /// Gets the audio devices capable of playback.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public OptionsPayload GetPlaybackAudioDevices()
        {
            var options = this.AudioService.Devices
                .Where(d => d.Flow == DataFlow.Render)
                .Select(d => new Option(d.FriendlyName, d.Key));

            return new OptionsPayload(options);
        }

        /// <summary>
        /// Gets the audio devices that match the specified <paramref name="filter"/>; otherwise all.
        /// </summary>
        /// <param name="filter">The optional filter.</param>
        /// <returns>The audio devices. </returns>
        private OptionsPayload GetAudioDevices(Func<IAudioDevice, bool> filter)
        {
            var options = this.AudioService.Devices
                .Where(filter)
                .GroupBy(device => device.Flow)
                .Select(g =>
                {
                    var children = g.Select(opt => new Option(opt.FriendlyName, opt.Key)).ToList();
                    return new Option(g.Key == DataFlow.Render ? "Playback" : "Recording", children);
                });

            return new OptionsPayload(options);
        }
    }
}
