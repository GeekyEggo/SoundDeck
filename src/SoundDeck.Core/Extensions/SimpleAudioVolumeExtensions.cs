namespace SoundDeck.Core.Extensions
{
    using NAudio.CoreAudioApi;
    using SoundDeck.Core.Volume;

    /// <summary>
    /// Provides extension methods for <see cref="SimpleAudioVolume"/>
    /// </summary>
    public static class SimpleAudioVolumeExtensions
    {
        /// <summary>
        /// Sets the audio of this instance based on the specified <paramref name="settings"/>.
        /// </summary>
        /// <param name="audio">This instance.</param>
        /// <param name="settings">The volume settings.</param>
        public static void Set(this SimpleAudioVolume audio, IVolumeSettings settings)
            => new SimpleAudioVolumeController(audio).Set(settings);
    }
}
