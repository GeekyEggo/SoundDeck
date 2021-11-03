namespace SoundDeck.Core.Interop.Helpers
{
    /// <summary>
    /// Provides methods for creating instances of <see cref="IAudioPolicyConfigFactory"/>.
    /// </summary>
    public class AudioPolicyConfigFactory
    {
        /// <summary>
        /// Creates a new <see cref="IAudioPolicyConfigFactory"/>.
        /// </summary>
        /// <returns>The <see cref="IAudioPolicyConfigFactory"/>.</returns>
        public static IAudioPolicyConfigFactory Create()
        {
            try
            {
                return new AudioPolicyConfigFactory21H2();
            }
            catch
            {
                return new AudioPolicyConfigFactoryDownlevel();
            }
        }
    }
}
