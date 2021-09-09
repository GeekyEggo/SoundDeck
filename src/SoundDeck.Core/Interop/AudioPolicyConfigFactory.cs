using System;

namespace SoundDeck.Core.Interop
{
    /// <summary>
    /// Provides methods for creating instances of <see cref="IAudioPolicyConfigFactory"/>.
    /// </summary>
    public class AudioPolicyConfigFactory
    {
        private delegate void GetActivationFactoryDelegate(string activatableClassId, ref Guid iid, out object factory);

        /// <summary>
        /// Creates a new instance of <see cref="IAudioPolicyConfigFactory"/> using combase.
        /// </summary>
        /// <returns>The new <see cref="IAudioPolicyConfigFactory"/>.</returns>
        public static IAudioPolicyConfigFactory CreateFromCombase()
            => Create(Combase.RoGetActivationFactory);

        /// <summary>
        /// Creates a new instance of <see cref="IAudioPolicyConfigFactory"/> using api-ms-win-core-winrt-l1-1-0 library.
        /// </summary>
        /// <returns>The new <see cref="IAudioPolicyConfigFactory"/>.</returns>
        public static IAudioPolicyConfigFactory CreateFromWinRT()
            => Create(WinRT.RoGetActivationFactory);

        /// <summary>
        /// Creates a new instance of <see cref="IAudioPolicyConfigFactory"/> using the specified <paramref name="getActivationFactory"/>
        /// </summary>
        /// <param name="getActivationFactory">The delegate responsible for creating the factory.</param>
        /// <returns>The <see cref="IAudioPolicyConfigFactory"/>.</returns>
        private static IAudioPolicyConfigFactory Create(GetActivationFactoryDelegate getActivationFactory)
        {
            var iid = typeof(IAudioPolicyConfigFactory).GUID;
            getActivationFactory("Windows.Media.Internal.AudioPolicyConfig", ref iid, out var factory);

            return (IAudioPolicyConfigFactory)factory;
        }
    }
}
