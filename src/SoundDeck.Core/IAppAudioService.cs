namespace SoundDeck.Core
{
    using System.Threading.Tasks;
    using NAudio.CoreAudioApi;
    using SoundDeck.Core.Sessions;

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
        string GetDefaultAudioDevice(uint processId, DataFlow flow);

        /// <summary>
        /// Sets the default audio device of an application that matches the specified <paramref name="criteria"/>.
        /// </summary>
        /// <param name="criteria">The process selection criteria that determines which process to update.</param>
        /// <param name="flow">The audio flow; either input or output.</param>
        /// <param name="deviceKey">The device key.</param>
        void SetDefaultAudioDevice(IProcessSelectionCriteria criteria, string deviceKey);

        /// <summary>
        /// Sets the volume of an application that matches the specified <paramref name="criteria"/>.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <param name="action">The action.</param>
        /// <param name="value">The value.</param>
        void SetVolume(IProcessSelectionCriteria criteria, VolumeAction action, int value);

        /// <summary>
        /// Attempts to control a session that matches the specified <paramref name="criteria"/>.
        /// </summary>
        /// <param name="criteria">The process selection criteria that determines which process to update.</param>
        /// <param name="action">The multimedia action to apply.</param>
        /// <returns>The task of controlling the multimedia.</returns>
        Task ControlAsync(IProcessSelectionCriteria criteria, MultimediaAction action);
    }
}
