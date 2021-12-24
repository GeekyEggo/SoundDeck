namespace SoundDeck.Core.Volume
{
    /// <summary>
    /// Provides information about a volume adjustment.
    /// </summary>
    public interface IVolumeSettings
    {
        /// <summary>
        /// Gets the action to apply.
        /// </summary>
        public VolumeAction VolumeAction { get; }

        /// <summary>
        /// Gets the value that supports <see cref="VolumeAction.Set"/>, <see cref="VolumeAction.IncreaseBy"/>, and <see cref="VolumeAction.DecreaseBy"/>.
        /// </summary>
        public int VolumeValue { get; }
    }
}
