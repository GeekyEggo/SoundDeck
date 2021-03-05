namespace SoundDeck.Plugin.Extensions
{
    using System;
    using SharpDeck.Enums;
    using SoundDeck.Core.Playback;
    using SoundDeck.Core.Playback.Playlists;
    using SoundDeck.Plugin.Contracts;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides extension methods for a <see cref="IPlayAudioAction"/>, used to assist with construction and settings of actions capable of playing audio.
    /// </summary>
    public static class PlayAudioActionExtensions
    {
        /// <summary>
        /// Sets the <see cref="IPlaylistController"/> for this instance.
        /// </summary>
        /// <param name="action">This instance.</param>
        /// <param name="settings">The settings.</param>
        public static void SetPlaylistController(this IPlayAudioAction action, IPlayAudioSettings settings)
        {
            // Ensure we have a playlist controller, and that the device is correct.
            if (action.PlaylistController == null
                || action.PlaylistController.Action != settings.Action)
            {
                // Ensure we keep the playlist before disposing.
                var playlist = action?.PlaylistController?.Playlist ?? new AudioFileCollection();
                action.PlaylistController?.Dispose();

                // Create the new controller, and assign its playlist
                action.PlaylistController = action.AudioService.CreatePlaylistController(settings.PlaybackAudioDeviceId, settings.Action);
                action.PlaylistController.Playlist = playlist;

                action.AddTimeChangedHandler();
            }
            else
            {
                action.PlaylistController.AudioPlayer.DeviceId = settings.PlaybackAudioDeviceId;
            }

            action.PlaylistController.Order = settings.Order;
        }

        /// <summary>
        /// Constructs an event handler for <see cref="IAudioPlayer.TimeChanged"/>.
        /// </summary>
        /// <param name="action">The action.</param>
        private static void AddTimeChangedHandler(this IPlayAudioAction action)
        {
            async void handler(object obj, PlaybackTimeEventArgs e)
            {
                string title = null;
                if (e.Current != TimeSpan.Zero)
                {
                    var remaining = e.Total.Subtract(e.Current);
                    title = remaining.TotalSeconds > 0.1f ? remaining.ToString("mm':'ss") : null;
                }

                try
                {
                    await action.SetTitleAsync(title, TargetType.Both);
                }
                catch (ObjectDisposedException)
                {
                    action.PlaylistController.AudioPlayer.TimeChanged -= handler;
                }
            }

            action.PlaylistController.AudioPlayer.Disposed += (_, __) => action.PlaylistController.AudioPlayer.TimeChanged -= handler;
            action.PlaylistController.AudioPlayer.TimeChanged += handler;
        }
    }
}
