namespace SoundDeck.Plugin.Actions
{
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;
    using SharpDeck.PropertyInspectors;
    using SharpDeck.PropertyInspectors.Payloads;
    using SoundDeck.Core;
    using SoundDeck.Core.Enums;
    using SoundDeck.Core.Playback;
    using SoundDeck.Plugin.Models.Settings;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides an Elgato Stream Deck action for playing an audio clip.
    /// </summary>
    [StreamDeckAction("Play Audio", UUID, "Images/PlayAudio/Action", Tooltip = "Play an audio clip, or three... or more, in the order your heart desires.")]
    [StreamDeckActionState("Images/PlayAudio/Key")]
    public class PlayAudio  : StreamDeckAction<PlayAudioSettings>
    {
        /// <summary>
        /// The unique identifier for the action.
        /// </summary>
        public const string UUID = "com.geekyEggo.soundDeck.playAudio";

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayAudio"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="args">The <see cref="ActionEventArgs{AppearancePayload}"/> instance containing the event data.</param>
        public PlayAudio(IAudioService audioService, ActionEventArgs<AppearancePayload> args)
        {
            this.AudioService = audioService;

            var settings = args.Payload.GetSettings<PlayAudioSettings>();
            this.Playback = new AudioPlaybackCollection(settings);
            this.SetPlayer(settings.AudioDeviceId);
        }

        /// <summary>
        /// Gets the audio service.
        /// </summary>
        private IAudioService AudioService { get; }

        /// <summary>
        /// Gets the playback collection.
        /// </summary>
        private AudioPlaybackCollection Playback { get; }

        /// <summary>
        /// Provides an entry point for the property inspector, which can be used to get the audio devices available on the system.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public Task<OptionsPayload> GetAudioDevices()
        {
            var options = this.AudioService.Devices
                .Where(d => d.Enabled && d.Flow == AudioFlowType.Playback)
                .Select(d => new Option(d.FriendlyName, d.Id));

            return Task.FromResult(new OptionsPayload(options));
        }

        /// <summary>
        /// Raises the <see cref="E:SharpDeck.StreamDeckActionEventReceiver.DidReceiveSettings" /> event.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        /// <returns>The task of updating the state of the object based on the settings.</returns>
        protected override Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args)
        {
            var settings = args.Payload.GetSettings<PlayAudioSettings>();

            this.SetPlayer(settings.AudioDeviceId);
            return this.Playback.SetOptionsAsync(settings);
        }

        /// <summary>
        /// Occurs when the user presses a key.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            try
            {
                await base.OnKeyDown(args);
                await this.Playback.NextAsync();
            }
            catch (Exception e)
            {
                await this.StreamDeck.LogMessageAsync(e.ToString());
                await this.ShowAlertAsync();
            }
        }

        /// <summary>
        /// Handles the <see cref="IAudioPlayer.TimeChanged"/> event of <see cref="AudioPlaybackCollection.Player"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PlaybackTimeEventArgs" /> instance containing the event data.</param>
        private async void Player_TimeChanged(object sender, PlaybackTimeEventArgs e)
        {
            static string getTime(PlaybackTimeEventArgs time)
            {
                if (time.Current == TimeSpan.Zero)
                {
                    return null;
                }

                var remaining = time.Total.Subtract(time.Current);
                return remaining.TotalSeconds > 0.5f ? remaining.ToString("mm':'ss") : null;
            }

            await this.SetTitleAsync(getTime(e));
        }

        /// <summary>
        /// Sets the <see cref="IAudioPlayer"/> of <see cref="Playback"/>.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        private void SetPlayer(string deviceId)
        {
            // dispose of the current player, if the device identifiers differ
            if (this.Playback.Player?.DeviceId != deviceId)
            {
                if (this.Playback.Player != null)
                {
                    this.Playback.Player.TimeChanged -= this.Player_TimeChanged;
                }

                this.Playback.Player?.Dispose();
                this.Playback.Player = null;
            }

            // attempt to re-set the audio player
            if (this.Playback.Player == null && !string.IsNullOrWhiteSpace(deviceId))
            {
                this.Playback.Player = this.AudioService.GetAudioPlayer(deviceId);
                if (this.Playback.Player != null)
                {
                    this.Playback.Player.TimeChanged += this.Player_TimeChanged;
                }
            }
        }
    }
}
