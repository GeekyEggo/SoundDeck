namespace SoundDeck.Core
{
    using SoundDeck.Core.Enums;

    /// <summary>
    /// Provides a service for controlling and interacting with the audio device of an application.
    /// </summary>
    public interface IAppAudioService
    {
        /// <summary>
        /// Gets the foreground application process identifier.
        /// </summary>
        /// <returns>The process identifier.</returns>
        uint GetForegroundAppProcessId();

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
        /// <param name="deviceId">The device identifier.</param>
        void SetDefaultAudioDevice(uint processId, AudioFlowType flow, string deviceId);

        /// <summary>
        /// Sets the default audio device for the specified process.
        /// </summary>
        /// <param name="processName">The process name.</param>
        /// <param name="flow">The audio flow; either input or output.</param>
        /// <param name="deviceId">The device identifier.</param>
        void SetDefaultAudioDevice(string processName, AudioFlowType flow, string deviceId);
    }
}
