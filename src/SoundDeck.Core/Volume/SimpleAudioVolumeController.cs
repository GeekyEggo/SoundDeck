namespace SoundDeck.Core.Volume
{
    using NAudio.CoreAudioApi;

    /// <summary>
    /// Provides a <see cref="VolumeController"/> capable of controlling <see cref="SimpleAudioVolume"/>.
    /// </summary>
    public class SimpleAudioVolumeController : VolumeController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleAudioVolumeController"/> class.
        /// </summary>
        /// <param name="audio">The audio to control.</param>
        public SimpleAudioVolumeController(SimpleAudioVolume audio)
            : base()
        {
            this.Audio = audio;
        }

        /// <inheritdoc/>
        public override bool Mute
        {
            get => this.Audio.Mute;
            set => this.Audio.Mute = value;
        }

        /// <inheritdoc/>
        public override float Volume
        {
            get => this.Audio.Volume;
            set => this.Audio.Volume = value;
        }

        /// <summary>
        /// Gets the underlying <see cref="SimpleAudioVolume"/>.
        /// </summary>
        private SimpleAudioVolume Audio { get; }
    }
}
