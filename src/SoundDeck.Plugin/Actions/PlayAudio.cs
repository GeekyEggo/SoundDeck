namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading.Tasks;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;
    using SharpDeck.PropertyInspectors;
    using SoundDeck.Core;
    using SoundDeck.Core.Playback;
    using SoundDeck.Core.Playback.Playlists;
    using SoundDeck.Plugin.Contracts;
    using SoundDeck.Plugin.Extensions;
    using SoundDeck.Plugin.Models;
    using SoundDeck.Plugin.Models.Payloads;
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
        /// Gets or sets the playback controller.
        /// </summary>
        public IPlaylistController PlaylistController { get; set; }

        /// <summary>
        /// Gets or sets the volume tester.
        /// </summary>
        private VolumeTester VolumeTester { get; set; }

        /// <summary>
        /// Adds the files to the playlist.
        /// </summary>
        /// <param name="payload">The payload containing the files.</param>
        [PropertyInspectorMethod]
        public void AddFiles(AddPlaylistFilesPayload payload)
            => this.WhenUserDefinedPlaylist(p => p.AddRange(payload.Files));

        /// <summary>
        /// Moves the file within the playlist.
        /// </summary>
        /// <param name="payload">The payload containing the old and new indexes.</param>
        [PropertyInspectorMethod]
        public void MoveFile(MovePlaylistFilePayload payload)
            => this.WhenUserDefinedPlaylist(p => p.Move(payload.OldIndex, payload.NewIndex));

        /// <summary>
        /// Deletes the file from the playlist.
        /// </summary>
        /// <param name="payload">The payload containing the index of the file to delete.</param>
        [PropertyInspectorMethod]
        public void RemoveFile(RemovePlaylistFilePayload payload)
            => this.WhenUserDefinedPlaylist(p => p.RemoveAt(payload.Index));

        /// <summary>
        /// Sets the volume of the audio clip whos volume is being tested.
        /// </summary>
        /// <param name="payload">The file.</param>
        [PropertyInspectorMethod]
        public void SetVolume(AdjustPlaylistFileVolumePayload payload)
        {
            lock (_syncRoot)
            {
                // When the current item is being played, adjust the audio player volume.
                if (this.PlaylistController?.Enumerator?.CurrentIndex == payload?.Index)
                {
                    this.PlaylistController.AudioPlayer.Volume = payload.Volume;
                }

                // When the volume tester is being played, adjust the audio player volume.
                if (this.VolumeTester?.Index == payload?.Index)
                {
                    this.VolumeTester.Player.Volume = payload.Volume;
                }

                // Set the volume on the playlist item, and persist it.
                this.PlaylistController.Playlist[payload.Index].Volume = payload.Volume;
                this.SavePlaylist();
            }
        }

        /// <summary>
        /// Tests the volume of the specified audio file by playing it.
        /// </summary>
        /// <param name="payload">The payload.</param>
        [PropertyInspectorMethod]
        public void TestVolume(AdjustPlaylistFileVolumePayload payload)
        {
            lock (_syncRoot)
            {
                // Ensure the volume tester exists and has the correct device identifier.
                var deviceId = this.PlaylistController.AudioPlayer.DeviceId;
                if (this.VolumeTester == null)
                {
                    this.VolumeTester = new VolumeTester
                    {
                        Player = this.AudioService.GetAudioPlayer(deviceId)
                    };
                }

                // Stop any current volume tests.
                this.VolumeTester.Player.Stop();

                // Set the current state of the volume tester, and then play the clip at the desired volume.
                this.VolumeTester.Index = payload.Index;
                this.VolumeTester.Player.DeviceId = deviceId;
                _ = this.VolumeTester.Player.PlayAsync(payload);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            this.VolumeTester?.Dispose();
            this.PlaylistController?.Dispose();
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
            this.SetPlaylistController(settings);
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

            // Construct the playlist.
            var playlist = new AudioFileCollection(settings.Files);
            playlist.CollectionChanged += (_, e) => this.SavePlaylist(e);

            // Set the playback, and its playlist
            this.SetPlaylistController(settings);
            this.PlaylistController.Playlist = playlist;
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
                await this.PlaylistController.NextAsync();
            }
            catch (Exception e)
            {
                await this.StreamDeck.LogMessageAsync(e.ToString());
                await this.ShowAlertAsync();
            }
        }

        /// <summary>
        /// Saves the playlist defined within the <see cref="PlaylistController" /> to the settings of this action.
        /// </summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private async void SavePlaylist(NotifyCollectionChangedEventArgs e = null)
        {
            // When an item was removed, ensure we stop any audio players that were playing it.
            if (e?.Action == NotifyCollectionChangedAction.Remove)
            {
                // The main player.
                if (e.OldStartingIndex == this.PlaylistController?.Enumerator?.CurrentIndex)
                {
                    this.PlaylistController?.AudioPlayer?.Stop();
                }

                // The volume tester.
                if (e.OldStartingIndex == this.VolumeTester?.Index)
                {
                    this.PlaylistController?.AudioPlayer.Stop();
                }
            }

            // Save the playlist to the action settings.
            var settings = await this.GetSettingsAsync()
                .ConfigureAwait(false);

            settings.Files = this.PlaylistController.Playlist.ToArray();

            await this.SetSettingsAsync(settings)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Provides a helper method for executing the specified <paramref name="action"/> when the underlying <see cref="IPlaylist"/> is under-defined.
        /// </summary>
        /// <param name="action">The action.</param>
        private void WhenUserDefinedPlaylist(Action<AudioFileCollection> action)
        {
            if (this.PlaylistController.Playlist is AudioFileCollection playlist)
            {
                action(playlist);
            }
        }
    }
}
