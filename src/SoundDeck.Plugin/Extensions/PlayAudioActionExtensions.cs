namespace SoundDeck.Plugin.Extensions
{
    using System;
    using SharpDeck.Enums;
    using SoundDeck.Core.Playback;
    using SoundDeck.Plugin.Contracts;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides extension methods for a <see cref="IPlayAudioAction"/>, used to assist with construction and settings of actions capable of playing audio.
    /// </summary>
    public static class PlayAudioActionExtensions
    {
        /// <summary>
        /// Sets the <see cref="IAudioPlayer"/> for this instance.
        /// </summary>
        /// <param name="action">This instance.</param>
        /// <param name="settings">The settings.</param>
        public static void SetPlayerSettings(this IPlayAudioAction action, IPlayAudioSettings settings)
        {
            action.SetPlaylist(settings);
            action.SetPlayer(settings);
        }

        /// <summary>
        /// Sets the playlist based on its current state.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="setting">The setting.</param>
        private static void SetPlaylist(this IPlayAudioAction action, IPlayAudioSettings settings)
        {
            // update the playlist
            if (action.Playlist == null)
            {
                action.Playlist = new Playlist(settings);
            }
            else
            {
                action.Playlist.SetOptions(settings);
            }
        }

        /// <summary>
        /// Sets the player of the action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="settings">The settings.</param>
        private static void SetPlayer(this IPlayAudioAction action, IPlayAudioSettings settings)
        {
            // update the player
            var deviceId = string.IsNullOrWhiteSpace(settings.PlaybackAudioDeviceId) ? action.AudioService.Devices.DefaultPlaybackDevice?.Id : settings.PlaybackAudioDeviceId;
            if (action.Player?.DeviceId != deviceId
                || action.Player?.Action != settings.Action)
            {
                action.Player?.Dispose();
                action.Player = action.AudioService.GetPlaylistPlayer(deviceId, settings.Action, action.Playlist);
                action.AddTimeChangedHandler();
            }
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
                    action.Player.TimeChanged -= handler;
                }
            }

            action.Player.Disposed += (_, __) => action.Player.TimeChanged -= handler;
            action.Player.TimeChanged += handler;
        }
    }
}
