namespace SoundDeck.Core.Interop.Helpers
{
    using System;
    using NAudio.CoreAudioApi;

    /// <summary>
    /// Provides methods for interacting with a audio policy configuration.
    /// </summary>
    public interface IPolicyConfigClient : IDisposable
    {
        /// <summary>
        /// Sets the default endpoint.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="eRole">The role.</param>
        void SetDefaultEndpoint(string deviceId, Role eRole);
    }
}
