namespace SoundDeck.Core.Extensions
{
    using NAudio.CoreAudioApi;
    using SoundDeck.Core.Volume;

    /// <summary>
    /// Provides extension methods for <see cref="AudioEndpointVolume"/>.
    /// </summary>
    public static class AudioEndpointVolumeExtensions
    {
        /// <summary>
        /// Sets the audio of this instance based on the specified <paramref name="settings"/>.
        /// </summary>
        /// <param name="audio">This instance.</param>
        /// <param name="settings">The volume settings.</param>
        public static void Set(this AudioEndpointVolume audio, IVolumeSettings settings)
            => new AudioEndpointVolumeController(audio).Set(settings);
    }
}
