namespace SoundDeck.Core.Interop.Helpers
{
    using System;

    /// <summary>
    /// Provides a factory for creating <see cref="IPolicyConfigClient"/>.
    /// </summary>
    public static class PolicyConfigClientFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="IPolicyConfigClient"/>
        /// </summary>
        /// <returns>The policy configuration client.</returns>
        /// <exception cref="EntryPointNotFoundException">Unable to instantiate policy configuration client.</exception>
        public static IPolicyConfigClient Create()
        {
            var client = new Interop.PolicyConfigClient();
            if (client is IPolicyConfigWin10 policyConfigWin10)
            {
                return new PolicyConfigClientWin10(policyConfigWin10);
            }
            else if (client is IPolicyConfig policyConfig)
            {
                return new PolicyConfigClient(policyConfig);
            }

            throw new EntryPointNotFoundException("Unable to instantiate policy configuration client.");
        }
    }
}
