namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Core.Playback;

    /// <summary>
    /// Provides settings for <see cref="Actions.PlayAudio"/>
    /// </summary>
    public class PlayAudioSettings : IPlaylistOptions
    {
        /// <summary>
        /// Gets or sets the audio device identifier to capture.
        /// </summary>
        public string AudioDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the type of the action that occurs upon the button being pressed.
        /// </summary>
        public PlaylistPlayerActionType Action { get; set; } = PlaylistPlayerActionType.PlayNext;

        /// <summary>
        /// Gets or sets the audio files to play.
        /// </summary>
        public PlaylistFile[] Files { get; set; }

        /// <summary>
        /// Gets or sets the playback order.
        /// </summary>
        public PlaylistOrderType Order { get; set; } = PlaylistOrderType.Sequential;
    }
}
