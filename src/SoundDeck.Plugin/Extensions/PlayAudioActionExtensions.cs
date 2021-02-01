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
            // Ensure we have a playlist controller, and that the device is correct.
            var deviceId = string.IsNullOrWhiteSpace(settings.PlaybackAudioDeviceId) ? action.AudioService.Devices.DefaultPlaybackDevice?.Id : settings.PlaybackAudioDeviceId;
            if (action.PlaybackController == null
                || action.PlaybackController.Action != settings.Action)
            {
                action.PlaybackController?.Dispose();
                action.PlaybackController = action.AudioService.CreatePlaylistController(deviceId, settings.Action);
                action.AddTimeChangedHandler();
            }
            else
            {
                action.PlaybackController.AudioPlayer.DeviceId = deviceId;
            }

            // Ensure the order, and the files align.
            action.PlaybackController.Playlist.Order = settings.Order;
            action.PlaybackController.Playlist.Files = settings.Files;
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
                    action.PlaybackController.AudioPlayer.TimeChanged -= handler;
                }
            }

            action.PlaybackController.AudioPlayer.Disposed += (_, __) => action.PlaybackController.AudioPlayer.TimeChanged -= handler;
            action.PlaybackController.AudioPlayer.TimeChanged += handler;
        }
    }
}
