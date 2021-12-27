namespace SoundDeck.Core.Interop.Helpers
{
    using System;
    using System.Runtime.InteropServices;
    using NAudio.CoreAudioApi;

    /// <summary>
    /// Provides a default implementation of <see cref="IPolicyConfigClient"/>.
    /// </summary>
    public sealed class PolicyConfigClient : IPolicyConfigClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyConfigClientWin10"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public PolicyConfigClient(IPolicyConfig client)
            => this.Client = client;

        /// <summary>
        /// Gets the underlying client.
        /// </summary>
        private IPolicyConfig Client { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Marshal.IsComObject(this.Client))
            {
                Marshal.FinalReleaseComObject(this.Client);
            }

            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public void SetDefaultEndpoint(string deviceId, Role role)
            => this.Client.SetDefaultEndpoint(deviceId, role);
    }
}
