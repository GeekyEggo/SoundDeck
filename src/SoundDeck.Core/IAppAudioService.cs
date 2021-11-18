namespace SoundDeck.Core
{
    using SoundDeck.Core.Interop;

    /// <summary>
    /// Provides a service for controlling and interacting with the audio device of an application.
    /// </summary>
    public interface IAppAudioService
    {
        /// <summary>
        /// Gets the default audio device for the specified process.
        /// </summary>
        /// <param name="processId">The process identifier.</param>
        /// <param name="flow">The audio flow; either input or output.</param>
        /// <returns>The audio device; otherwise <c>null</c>.</returns>
        string GetDefaultAudioDevice(uint processId, AudioFlowType flow);

        /// <summary>
        /// Sets the default audio device for the specified process.
        /// </summary>
        /// <param name="processId">The process identifier.</param>
        /// <param name="flow">The audio flow; either input or output.</param>
        /// <param name="deviceKey">The device key.</param>
        void SetDefaultAudioDevice(uint processId, AudioFlowType flow, string deviceKey);

        /// <summary>
        /// Sets the default audio device for the specified process.
        /// </summary>
        /// <param name="processName">The process name.</param>
        /// <param name="flow">The audio flow; either input or output.</param>
        /// <param name="deviceKey">The device key.</param>
        void SetDefaultAudioDevice(string processName, AudioFlowType flow, string deviceKey);

        /// <summary>
        /// Sets the default audio device for the foreground application.
        /// </summary>
        /// <param name="flow">The audio flow; either input or output.</param>
        /// <param name="deviceKey">The device key.</param>
        void SetDefaultAudioDeviceForForegroundApp(AudioFlowType flow, string deviceKey);
    }
}
