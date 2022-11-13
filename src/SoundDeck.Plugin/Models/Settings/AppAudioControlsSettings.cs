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
        public PressAction PressAction { get; set; } = PressAction.PlayPause;

        /// <summary>
        /// Gets or sets the rotate action.
        /// </summary>
        public RotateAction RotateAction { get; set; } = RotateAction.Track;
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
    /// The action that occurs when pressing.
    /// </summary>
    public enum PressAction
    {
        /// <summary>
        /// Toggle play/pause.
        /// </summary>
        PlayPause = 0,

        /// <summary>
        /// Toggle mute.
        /// </summary>
        ToggleMute = 1
    }

    /// <summary>
    /// The action that occurs when rotating.
    /// </summary>
    public enum RotateAction
    {
        /// <summary>
        /// Track control, e.g. previous/next.
        /// </summary>
        Track = 0,

        /// <summary>
        /// Volume control.
        /// </summary>
        Volume = 1
    }
}
