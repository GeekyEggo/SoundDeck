namespace SoundDeck.Core.Interop
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Provides interoperability for WinRT.
    /// </summary>
    public static class WinRT
    {
        /// <summary>
        /// Gets the activation factory for the specified runtime class.
        /// </summary>
        /// <param name="activatableClassId">The ID of the activatable class.</param>
        /// <param name="iid">The reference ID of the interface.</param>
        /// <param name="factory">The activation factory.</param>
        [DllImport("api-ms-win-core-winrt-l1-1-0.dll")]
        public static extern void RoGetActivationFactory(
            [MarshalAs(UnmanagedType.HString)] string activatableClassId,
            [In] ref Guid iid,
            [Out, MarshalAs(UnmanagedType.IInspectable)] out object factory);
    }
}
