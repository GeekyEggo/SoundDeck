namespace SoundDeck.Core.Interop.Helpers
{
    using System;
    using NAudio.CoreAudioApi;

    /// <summary>
    /// Provides an implementation of <see cref="IAudioPolicyConfigFactory"/> for Windows 21H2.
    /// </summary>
    public class AudioPolicyConfigFactory21H2 : IAudioPolicyConfigFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioPolicyConfigFactory21H2"/> class.
        /// </summary>
        public AudioPolicyConfigFactory21H2()
        {
            var iid = typeof(IAudioPolicyConfigFactory21H2).GUID;
            Combase.RoGetActivationFactory("Windows.Media.Internal.AudioPolicyConfig", ref iid, out object factory);
            this.Factory = (IAudioPolicyConfigFactory21H2)factory;
        }

        /// <summary>
        /// Gets the underlying factory.
        /// </summary>
        private IAudioPolicyConfigFactory21H2 Factory { get; }

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
