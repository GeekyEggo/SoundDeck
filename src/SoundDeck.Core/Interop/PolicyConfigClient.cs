namespace SoundDeck.Core.Interop
{
    using System;
    using System.Runtime.InteropServices;
    using SoundDeck.Core.Interop.Helpers;

    /// <summary>
    /// Helper class for instantiating <see cref="IPolicyConfigClient"/>.
    /// </summary>
    [ComImport]
    [Guid(ComGuid.POLICY_CONFIG_CLIENT)]
    public class PolicyConfigClient
    {
    }
}
