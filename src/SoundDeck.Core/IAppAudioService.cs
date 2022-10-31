namespace SoundDeck.Core
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NAudio.CoreAudioApi;
    using SoundDeck.Core.Sessions;
    using SoundDeck.Core.Volume;
    using Windows.Media.Control;

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
        /// Sets the volume of an application based on the specified <paramref name="settings"/>.
        /// </summary>
        /// <param name="settings">The settings that determine the application and the volume adjustment.</param>
        void SetVolume<T>(T settings)
            where T : IProcessSelectionCriteria, IVolumeSettings;

        /// <summary>
        /// Attempts to control a session that matches the specified <paramref name="criteria"/>.
        /// </summary>
        /// <param name="criteria">The process selection criteria that determines which process to update.</param>
        /// <param name="action">The multimedia action to apply.</param>
        /// <returns>The task of controlling the multimedia.</returns>
        Task ControlAsync(IProcessSelectionCriteria criteria, MultimediaAction action);

        /// <summary>
        /// Gets the all active audio sessions audio sessions.
        /// </summary>
        /// <returns>The active audio sessions.</returns>
        IEnumerable<AudioSessionControl> GetAudioSessions();

        /// <summary>
        /// Gets the all active multimedia session asynchronously.
        /// </summary>
        /// <returns>The active multimedia sessions.</returns>
        Task<IReadOnlyList<GlobalSystemMediaTransportControlsSession>> GetMultimediaSessionAsync();
    }
}
