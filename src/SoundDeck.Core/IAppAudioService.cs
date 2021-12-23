namespace SoundDeck.Core
{
    using System.Threading.Tasks;
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

        /// <summary>
        /// Attempts to control a session with the specified matching <paramref name="searchCriteria"/>.
        /// </summary>
        /// <param name="searchCriteria">The search criteria of the session to match.</param>
        /// <param name="action">The multimedia action to apply.</param>
        /// <returns>The task of controlling the multimedia.</returns>
        Task TryControlAsync(string searchCriteria, MultimediaAction action);
    }
}
