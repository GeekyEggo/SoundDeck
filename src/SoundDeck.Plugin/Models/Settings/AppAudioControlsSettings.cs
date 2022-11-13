namespace SoundDeck.Plugin.Models.Settings
{
    /// <summary>
    /// Provides settings for <see cref="Actions.AppAudioControls"/>.
    /// </summary>
    public class AppAudioControlSettings : ProcessSelectionCriteriaSettings
    {
        /// <summary>
        /// Gets or sets the action layout.
        /// </summary>
        public ActionLayout ActionLayout { get; set; } = ActionLayout.PlaybackAndVolume;

        /// <summary>
        /// Gets or sets the press action.
        /// </summary>
        public AudioControlAction PressAction { get; set; } = AudioControlAction.Track;

        /// <summary>
        /// Gets or sets the rotate action.
        /// </summary>
        public AudioControlAction RotateAction { get; set; } = AudioControlAction.Track;

        /// <summary>
        /// Gets or sets the volume value.
        /// </summary>
        public int VolumeValue { get; set; } = 5;
    }

    /// <summary>
    /// The possible layout.
    /// </summary>
    public enum ActionLayout
    {
        /// <summary>
        /// Playback and volume layout.
        /// </summary>
        PlaybackAndVolume = 0,

        /// <summary>
        /// Custom layout.
        /// </summary>
        Custom = 1
    }

    /// <summary>
    /// The enumeration of possible entities that can be controlled on the audio.
    /// </summary>
    public enum AudioControlAction
    {
        /// <summary>
        /// Controls the track, e.g. play/pause, skip/next.
        /// </summary>
        Track = 0,

        /// <summary>
        /// Controls the volume.
        /// </summary>
        Volume = 1
    }
}
