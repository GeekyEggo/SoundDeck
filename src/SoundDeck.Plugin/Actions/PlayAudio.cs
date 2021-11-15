namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.PropertyInspectors;
    using SoundDeck.Core;
    using SoundDeck.Core.Playback;
    using SoundDeck.Core.Playback.Playlists;
    using SoundDeck.Plugin.Contracts;
    using SoundDeck.Plugin.Extensions;
    using SoundDeck.Plugin.Models;
    using SoundDeck.Plugin.Models.Payloads;
    using SoundDeck.Plugin.Models.Settings;
    using SoundDeck.Plugin.Models.UI;

    /// <summary>
    /// Provides an Elgato Stream Deck action for playing an audio clip.
    /// </summary>
    [StreamDeckAction("com.geekyeggo.sounddeck.playaudio")]
    public class PlayAudio : ActionBase<PlayAudioSettings>, IPlayAudioAction
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private static readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayAudio" /> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="fileDialogProvider">The file dialog provider.</param>
        public PlayAudio(IAudioService audioService, IFileDialogProvider fileDialogProvider)
            : base(audioService)
        {
            this.FileDialogProvider = fileDialogProvider;
        }

        /// <summary>
        /// Gets or sets the playback controller.
        /// </summary>
        public IPlaylistController PlaylistController { get; set; }

        /// <summary>
        /// Gets the file dialog provider.
        /// </summary>
        public IFileDialogProvider FileDialogProvider { get; }

        /// <summary>
        /// Gets or sets the volume tester.
        /// </summary>
        private VolumeTester VolumeTester { get; set; }

        /// <summary>
        /// Prompts the user to add files to the playlist.
        /// </summary>
        [PropertyInspectorMethod]
        public void AddFiles()
        {
            var filter =
                "All audio|*.aiff;*.aif;*.mp3;*.mpga;*.oga;*.ogg;*.opus;*.streamDeckAudio;*.wav|" +
                "AIFF/Mac audio|*.aiff;*.aif|" +
                "MP3 audio|*.mp3;*.mpga|" +
                "Ogg audio|*.oga;*.ogg;*.opus|" +
                /*"Stream Deck audio|*.streamDeckAudio|" +*/
                "WAV audio|*.wav";

            var files = this.FileDialogProvider.ShowOpenDialog("Add Files to Playlist", filter);
            if (files.Length > 0)
            {
                this.WhenUserDefinedPlaylist(p => p.AddRange(files));
            }
        }

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
                // Update the volume of anything playing this file.
                this.PlaylistController.TrySetVolume(payload);
                this.VolumeTester.TrySetVolume(payload);

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
                    this.VolumeTester = new VolumeTester(this.AudioService.GetAudioPlayer(deviceId));
                }

                _ = this.VolumeTester.PlayAsync(payload, deviceId);
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
        /// Occurs when <see cref="IStreamDeckConnection.KeyDown"/> is held down for <see cref="StreamDeckAction.LongPressInterval"/>.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs{KeyPayload}"/> instance containing the event data.</param>
        /// <returns>The task of handling the event.</returns>
        protected override async Task OnKeyLongPress(ActionEventArgs<KeyPayload> args)
        {
            try
            {
                await base.OnKeyLongPress(args);

                this.PlaylistController.AudioPlayer?.Stop();
                this.PlaylistController.Reset();
                await this.PlaylistController.NextAsync();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Play audio failed on long key press.");
                await this.ShowAlertAsync();
            }
        }

        /// <summary>
        /// Occurs when <see cref="IStreamDeckConnection.KeyDown" /> is released before <see cref="StreamDeckAction.LongPressInterval" />.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs{KeyPayload}" /> instance containing the event data.</param>
        /// <returns>The task of handling the event.</returns>
        protected override async Task OnKeyPress(ActionEventArgs<KeyPayload> args)
        {
            try
            {
                await base.OnKeyPress(args);
                await this.PlaylistController.NextAsync();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Play audio failed on key press.");
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
                this.PlaylistController.TryStop(e.OldStartingIndex);
                this.VolumeTester.TryStop(e.OldStartingIndex);
            }

            await this.UpdateSettingsAsync(s => s.Files = this.PlaylistController.Playlist.ToArray())
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
