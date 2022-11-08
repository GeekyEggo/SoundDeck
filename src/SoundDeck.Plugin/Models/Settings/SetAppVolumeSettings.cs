namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Core.Volume;
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// Provides settings for the <see cref="SetAppAudioDevice"/> action.
    /// </summary>
    public class SetAppVolumeSettings : ProcessSelectionCriteriaSettings, IVolumeSettings
    {
        /// <inheritdoc/>
        public VolumeAction VolumeAction { get; set; }

        /// <inheritdoc/>
        public int VolumeValue { get; set; } = 100;
    }
}
