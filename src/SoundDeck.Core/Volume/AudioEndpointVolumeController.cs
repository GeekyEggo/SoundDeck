namespace SoundDeck.Core.Volume
{
    using NAudio.CoreAudioApi;

    /// <summary>
    /// Provides a <see cref="AudioEndpointVolume"/> capable of controlling <see cref="SimpleAudioVolume"/>.
    /// </summary>
    public class AudioEndpointVolumeController : VolumeController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioEndpointVolumeController"/> class.
        /// </summary>
        /// <param name="endpoint">The audio endpoint to control.</param>
        public AudioEndpointVolumeController(AudioEndpointVolume endpoint)
            : base()
        {
            this.Endpoint = endpoint;
        }

        /// <inheritdoc/>
        public override bool Mute
        {
            get => this.Endpoint.Mute;
            set => this.Endpoint.Mute = value;
        }

        /// <inheritdoc/>
        public override float Volume
        {
            get => this.Endpoint.MasterVolumeLevelScalar;
            set => this.Endpoint.MasterVolumeLevelScalar = value;
        }

        /// <summary>
        /// Gets the underlying <see cref="AudioEndpointVolume"/>.
        /// </summary>
        private AudioEndpointVolume Endpoint { get; }
    }
}
