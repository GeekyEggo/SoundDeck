namespace SoundDeck.Core.Interop.Helpers
{
    using System;
    using NAudio.CoreAudioApi;

    /// <summary>
    /// Provides an implementation of <see cref="IAudioPolicyConfigFactory"/> for Windows pre 21H2.
    /// </summary>
    public class AudioPolicyConfigFactoryDownlevel : IAudioPolicyConfigFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioPolicyConfigFactoryDownlevel"/> class.
        /// </summary>
        public AudioPolicyConfigFactoryDownlevel()
        {
            var iid = typeof(IAudioPolicyConfigFactoryDownlevel).GUID;
            Combase.RoGetActivationFactory("Windows.Media.Internal.AudioPolicyConfig", ref iid, out object factory);
            this.Factory = (IAudioPolicyConfigFactoryDownlevel)factory;
        }

        /// <summary>
        /// Gets the underlying factory.
        /// </summary>
        private IAudioPolicyConfigFactoryDownlevel Factory { get; }

        /// <inheritdoc/>
        public HRESULT ClearAllPersistedApplicationDefaultEndpoints()
            => this.Factory.ClearAllPersistedApplicationDefaultEndpoints();

        /// <inheritdoc/>
        public HRESULT GetPersistedDefaultAudioEndpoint(uint processId, DataFlow flow, Role role, out string deviceId)
            => this.Factory.GetPersistedDefaultAudioEndpoint(processId, flow, role, out deviceId);

        /// <inheritdoc/>
        public HRESULT SetPersistedDefaultAudioEndpoint(uint processId, DataFlow flow, Role role, IntPtr deviceId)
            => this.Factory.SetPersistedDefaultAudioEndpoint(processId, flow, role, deviceId);
    }
}
