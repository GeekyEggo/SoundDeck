namespace SoundDeck.Core.Interop
{
    /// <summary>
    /// Provides methods for creating instances of <see cref="IAudioPolicyConfigFactory"/>.
    /// </summary>
    public class AudioPolicyConfigFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="IAudioPolicyConfigFactory"/>
        /// </summary>
        /// <returns>The new <see cref="IAudioPolicyConfigFactory"/>.</returns>
        public static IAudioPolicyConfigFactory Create()
        {
            var iid = typeof(IAudioPolicyConfigFactory).GUID;
            Combase.RoGetActivationFactory("Windows.Media.Internal.AudioPolicyConfig", ref iid, out object factory);

            return (IAudioPolicyConfigFactory)factory;
        }
    }
}
