namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Core.Sessions;
    using SoundDeck.Core.Volume;
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// Provides settings for the <see cref="SetAppAudioDevice"/> action.
    /// </summary>
    public class SetAppVolumeSettings : IProcessSelectionCriteria, IVolumeSettings
    {
        /// <inheritdoc/>
        public VolumeAction VolumeAction { get; set; }

        /// <inheritdoc/>
        public int VolumeValue { get; set; }

        /// <inheritdoc/>
        public string ProcessName { get; set; }

        /// <inheritdoc/>
        public ProcessSelectionType ProcessSelectionType { get; set; }
    }
}
