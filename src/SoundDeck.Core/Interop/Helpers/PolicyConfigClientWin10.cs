namespace SoundDeck.Core.Interop.Helpers
{
    using System;
    using System.Runtime.InteropServices;
    using NAudio.CoreAudioApi;

    /// <summary>
    /// Provides a Windows 10 implementation of <see cref="IPolicyConfigClient"/>.
    /// </summary>
    public class PolicyConfigClientWin10 : IPolicyConfigClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyConfigClientWin10"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public PolicyConfigClientWin10(IPolicyConfigWin10 client)
            => this.Client = client;

        /// <summary>
        /// Gets the underlying client.
        /// </summary>
        private IPolicyConfigWin10 Client { get; }

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
