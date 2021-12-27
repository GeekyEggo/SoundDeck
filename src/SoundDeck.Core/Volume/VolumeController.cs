namespace SoundDeck.Core.Volume
{
    using System;

    /// <summary>
    /// Provides information about, and allows control of, volume.
    /// </summary>
    public abstract class VolumeController
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is muted.
        /// </summary>
        public abstract bool Mute { get; set; }

        /// <summary>
        /// Gets or sets the volume.
        /// </summary>
        public abstract float Volume { get; set; }

        /// <summary>
        /// Sets the audio of this instance based on the specified <paramref name="settings"/>.
        /// </summary>
        /// <param name="settings">The volume settings.</param>
        public void Set(IVolumeSettings settings)
        {
            switch (settings.VolumeAction)
            {
                case VolumeAction.Mute:
                    this.Mute = true;
                    break;

                case VolumeAction.Unmute:
                    this.Mute = false;
                    break;

                case VolumeAction.ToggleMute:
                    this.Mute = !this.Mute;
                    break;

                case VolumeAction.Set:
                    this.Volume = Math.Max(0f, Math.Min(1f, settings.VolumeValue / 100f));
                    break;

                case VolumeAction.IncreaseBy:
                    this.Volume = Math.Min(1f, this.Volume + (settings.VolumeValue / 100f));
                    break;

                case VolumeAction.DecreaseBy:
                    this.Volume = Math.Max(0f, this.Volume - (settings.VolumeValue / 100f));
                    break;
            }
        }
    }
}
