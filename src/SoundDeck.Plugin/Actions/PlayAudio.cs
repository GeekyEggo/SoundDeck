namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;
    using SharpDeck.PropertyInspectors;
    using SharpDeck.PropertyInspectors.Payloads;
    using SoundDeck.Core;
    using SoundDeck.Core.Enums;
    using SoundDeck.Core.Playback;
    using SoundDeck.Plugin.Extensions;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides an Elgato Stream Deck action for playing an audio clip.
    /// </summary>
    [StreamDeckAction("Play Audio", UUID, "Images/PlayAudio/Action", Tooltip = "Play an audio clip, or three... or more, in the order your heart desires.")]
    [StreamDeckActionState("Images/PlayAudio/Key")]
    public class PlayAudio  : StreamDeckAction<PlayAudioSettings>, IPlayAudioAction
    {
        /// <summary>
        /// The unique identifier for the action.
        /// </summary>
        public const string UUID = "com.geekyeggo.sounddeck.playaudio";

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayAudio"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="args">The <see cref="ActionEventArgs{AppearancePayload}"/> instance containing the event data.</param>
        public PlayAudio(IAudioService audioService)
        {
            this.AudioService = audioService;
        }

        /// <summary>
        /// Gets the audio service.
        /// </summary>
        public IAudioService AudioService { get; }

        /// <summary>
        /// Gets or sets the player.
        /// </summary>
        public IPlaylistPlayer Player { get; set; }

        /// <summary>
        /// Gets or sets the playlist.
        /// </summary>
        public Playlist Playlist { get; set; }

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
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            this.Player?.Dispose();
            this.SetTitleAsync();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Handles the <see cref="DidReceiveSettings" /> event.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs`1" /> instance containing the event data.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>The task of updating the state of the object based on the settings.</returns>
        protected override Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, PlayAudioSettings settings)
        {
            this.SetPlayer(settings, this.SetTitleAsync);
            this.Playlist.SetOptions(settings);

            return base.OnDidReceiveSettings(args, settings);
        }

        /// <summary>
        /// Occurs when this instance is initialized.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        /// <param name="settings">The settings.</param>
        protected override void OnInit(ActionEventArgs<AppearancePayload> args, PlayAudioSettings settings)
        {
            base.OnInit(args, settings);

            this.Playlist = new Playlist(settings);
            this.SetPlayer(settings, this.SetTitleAsync);
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
                await this.Player.NextAsync();
            }
            catch (Exception e)
            {
                await this.StreamDeck.LogMessageAsync(e.ToString());
                await this.ShowAlertAsync();
            }
        }
    }
}
