namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Core.Sessions;
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// Provides settings for the <see cref="SetAppAudioDevice"/> action.
    /// </summary>
    public class SetAppVolumeSettings : IProcessSelectionCriteria
    {
        /// <summary>
        /// Gets or sets the action to apply to the volume.
        /// </summary>
        public VolumeAction Action { get; set; }

        /// <summary>
        /// Gets or sets the value that accompanies <see cref="VolumeAction.Set"/>, <see cref="VolumeAction.IncreaseBy"/>, and <see cref="VolumeAction.DecreaseBy"/>.
        /// </summary>
        public int ActionValue { get; set; }

        /// <inheritdoc/>
        public string ProcessName { get; set; }

        /// <inheritdoc/>
        public ProcessSelectionType ProcessSelectionType { get; set; }
    }
}
