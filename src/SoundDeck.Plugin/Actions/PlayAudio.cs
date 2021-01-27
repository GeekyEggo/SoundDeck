namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Threading.Tasks;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;
    using SharpDeck.PropertyInspectors;
    using SoundDeck.Core;
    using SoundDeck.Core.Playback;
    using SoundDeck.Plugin.Contracts;
    using SoundDeck.Plugin.Extensions;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides an Elgato Stream Deck action for playing an audio clip.
    /// </summary>
    [StreamDeckAction("com.geekyeggo.sounddeck.playaudio")]
    [StreamDeckActionState("Images/PlayAudio/Key")]
    public class PlayAudio : ActionBase<PlayAudioSettings>, IPlayAudioAction
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private static readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayAudio"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="args">The <see cref="ActionEventArgs{AppearancePayload}"/> instance containing the event data.</param>
        public PlayAudio(IAudioService audioService)
            : base(audioService)
        {
        }

        /// <summary>
        /// Gets or sets the player.
        /// </summary>
        public IPlaylistPlayer Player { get; set; }

        /// <summary>
        /// Gets or sets the playlist.
        /// </summary>
        public Playlist Playlist { get; set; }

        /// <summary>
        /// Gets or sets the player used to test the volume.
        /// </summary>
        private IAudioFilePlayer VolumeTestPlayer { get; set; }

        /// <summary>
        /// Sets the volume of the audio clip whos volume is being tested.
        /// </summary>
        /// <param name="file">The file.</param>
        [PropertyInspectorMethod]
        public void SetTestVolume(AudioFileInfo file)
        {
            lock (_syncRoot)
            {
                if (this.Player?.FileName == file.Path)
                {
                    this.Player.Volume = file.Volume;
                }

                if (this.VolumeTestPlayer?.FileName == file.Path)
                {
                    this.VolumeTestPlayer.Volume = file.Volume;
                }
            }
        }

        /// <summary>
        /// Tests the volume of the specified audio file by playing it.
        /// </summary>
        /// <param name="file">The file.</param>
        [PropertyInspectorMethod]
        public void TestVolume(AudioFileInfo file)
        {
            lock (_syncRoot)
            {
                this.VolumeTestPlayer?.Dispose();

                this.VolumeTestPlayer = this.AudioService.GetAudioPlayer(this.Player.DeviceId);
                _ = this.VolumeTestPlayer.PlayAsync(file);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            this.VolumeTestPlayer?.Dispose();
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
            this.SetPlayerSettings(settings);
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
            this.SetPlayerSettings(settings);
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
