namespace SoundDeck.Plugin.Extensions
{
    using System;
    using System.Threading.Tasks;
    using SharpDeck.Enums;
    using SoundDeck.Core.Playback;
    using SoundDeck.Plugin.Actions;
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
        /// <param name="setTitleAsync">The delegate used to set the title of the action asynchronously.</param>
        public static void SetPlayer(this IPlayAudioAction action, IPlayAudioSettings settings, Func<string, TargetType, Task> setTitleAsync)
        {
            var deviceId = string.IsNullOrWhiteSpace(settings.AudioDeviceId) ? action.AudioService.Devices.DefaultPlaybackDevice?.Id : settings.AudioDeviceId;

            if (action.Player?.DeviceId != deviceId
                || action.Player?.Action != settings.Action)
            {
                action.Player?.Dispose();
                action.Player = action.AudioService.GetPlaylistPlayer(deviceId, settings.Action, action.Playlist);
                action.Player.TimeChanged += GetTimeChangedHandler(setTitleAsync);
            }
        }

        /// <summary>
        /// Constructs an event handler for <see cref="IAudioPlayer.TimeChanged"/>.
        /// </summary>
        /// <param name="setTitleAsync">The delegate used to set the title of the action asynchronously.</param>
        /// <returns>The event handler.</returns>
        private static EventHandler<PlaybackTimeEventArgs> GetTimeChangedHandler(Func<string, TargetType, Task> setTitleAsync)
        {
            return async (obj, e) =>
            {
                string getTime(PlaybackTimeEventArgs time)
                {
                    if (time.Current == TimeSpan.Zero)
                    {
                        return null;
                    }

                    var remaining = time.Total.Subtract(time.Current);
                    return remaining.TotalSeconds > 0.1f ? remaining.ToString("mm':'ss") : null;
                }

                await setTitleAsync(getTime(e), TargetType.Both);
            };
        }
    }
}
