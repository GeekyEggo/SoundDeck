namespace SoundDeck.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using NAudio.CoreAudioApi;
    using SoundDeck.Core.Comparers;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Core.Interop;
    using SoundDeck.Core.Interop.Helpers;
    using Windows.ApplicationModel;
    using Windows.Media.Control;

    /// <summary>
    /// Provides a service for controlling and interacting with the audio device of an application.
    /// </summary>
    public class AppAudioService : IAppAudioService
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private static readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// The session manager.
        /// </summary>
        private GlobalSystemMediaTransportControlsSessionManager _manager = null;

        /// <summary>
        /// The device interface string represents audio playback.
        /// </summary>
        private const string DEVINTERFACE_AUDIO_RENDER = "#{e6327cad-dcec-4949-ae8a-991e976a79d2}";

        /// <summary>
        /// The device interface string represents audio recording.
        /// </summary>
        private const string DEVINTERFACE_AUDIO_CAPTURE = "#{2eef81be-33fa-4800-9670-1cd474972c3f}";

        /// <summary>
        /// The MMDevice API token.
        /// </summary>
        private const string MMDEVAPI_TOKEN = @"\\?\SWD#MMDEVAPI#";

        /// <summary>
        /// Initializes a new instance of the <see cref="AppAudioService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public AppAudioService(ILogger<AppAudioService> logger)
        {
            try
            {
                this.AudioPolicyConfig = AudioPolicyConfigFactory.Create();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to instantiate audio policy config factory.");
                throw;
            }
        }

        /// <summary>
        /// Gets the audio policy configuration facotry.
        /// </summary>
        private IAudioPolicyConfigFactory AudioPolicyConfig { get; }

        /// <inheritdoc/>
        public string GetDefaultAudioDevice(uint processId, AudioFlowType flow)
        {
            try
            {
                var dataFlow = this.GetDataFlow(flow);
                this.AudioPolicyConfig.GetPersistedDefaultAudioEndpoint(processId, dataFlow, Role.Multimedia | Role.Console, out var deviceId);

                return this.ParseDeviceId(deviceId);
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public void SetDefaultAudioDevice(string processName, AudioFlowType flow, string deviceKey)
            => this.SetDefaultAudioDevice(new ProcessNamePredicate(processName), flow, deviceKey);

        /// <inheritdoc/>
        public void SetDefaultAudioDeviceForForegroundApp(AudioFlowType flow, string deviceKey)
        {
            var hwnd = User32.GetForegroundWindow();
            User32.GetWindowThreadProcessId(hwnd, out var processId);

            this.SetDefaultAudioDevice(new IdentifiedProcessPredicate(processId), flow, deviceKey);
        }

        /// <inheritdoc/>
        public async Task TryControlAsync(string processName, MultimediaAction action)
        {
            var manager = await this.GetManagerAsync();
            foreach (var session in manager.GetSessions().Where(s => IsMatch(s, processName)))
            {
                switch (action)
                {
                    case MultimediaAction.SkipNext:
                        await session.TrySkipNextAsync();
                        break;

                    case MultimediaAction.SkipPrevious:
                        await session.TrySkipPreviousAsync();
                        break;

                    case MultimediaAction.Stop:
                        await session.TryStopAsync();
                        break;

                    case MultimediaAction.TogglePlayPause:
                        await session.TryTogglePlayPauseAsync();
                        break;
                }
            }

            bool IsMatch(GlobalSystemMediaTransportControlsSession session, string searchCriteria)
            {
                try
                {
                    if (session.SourceAppUserModelId.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    var appInfo = AppInfo.GetFromAppUserModelId(session.SourceAppUserModelId);
                    return appInfo.DisplayInfo.DisplayName.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Sets the default audio device for the first process that matches the specified <paramref name="processPredicate"/>.
        /// </summary>
        /// <param name="processPredicate">The process predicate to match against.</param>
        /// <param name="flow">The audio flow; either input or output.</param>
        /// <param name="deviceKey">The device key.</param>
        private void SetDefaultAudioDevice(IProcessPredicate processPredicate, AudioFlowType flow, string deviceKey)
        {
            var dataFlow = this.GetDataFlow(flow);
            foreach (var audioSession in this.GetAudioSessions(dataFlow))
            {
                var audioSessionProcessId = audioSession.GetProcessID;
                if (processPredicate.IsMatch(audioSessionProcessId))
                {
                    // Default to zero pointer; this will only change if an audio device has been specified.
                    var hstring = IntPtr.Zero;
                    var device = AudioDevices.Current.GetDeviceByKey(deviceKey);
                    if (device.IsReadOnly)
                    {
                        var persistDeviceId = this.GenerateDeviceId(device.Id);
                        Combase.WindowsCreateString(persistDeviceId, (uint)persistDeviceId.Length, out hstring);
                    }

                    // Set the audio device for the process.
                    this.AudioPolicyConfig.SetPersistedDefaultAudioEndpoint(audioSessionProcessId, dataFlow, Role.Multimedia, hstring);
                    this.AudioPolicyConfig.SetPersistedDefaultAudioEndpoint(audioSessionProcessId, dataFlow, Role.Console, hstring);
                }
            }
        }

        /// <summary>
        /// Gets the all active audio sessions audio sessions.
        /// </summary>
        /// <param name="flow">The audio data flow.</param>
        /// <returns>The active audio sessions.</returns>
        private IEnumerable<AudioSessionControl> GetAudioSessions(DataFlow flow)
        {
            using (var deviceEnumerator = new MMDeviceEnumerator())
            {
                var sessions = deviceEnumerator
                    .EnumerateAudioEndPoints(flow, DeviceState.Active)
                    .Select(d => d.AudioSessionManager.Sessions);

                foreach (var session in sessions)
                {
                    for (var i = 0; i < session.Count; i++)
                    {
                        yield return session[i];
                    }
                }
            }
        }

        /// <summary>
        /// Gets the data flow from the specified <see cref="AudioFlowType"/>.
        /// </summary>
        /// <param name="flow">The flow.</param>
        /// <returns>The interop data flow.</returns>
        private DataFlow GetDataFlow(AudioFlowType flow)
            => flow == AudioFlowType.Playback ? DataFlow.Render : DataFlow.Capture;

        /// <summary>
        /// Generates the device identifier that can be used to set the persisted default audio endpoint for a process.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="flow">The flow.</param>
        /// <returns>The device identifier.</returns>
        private string GenerateDeviceId(string deviceId, DataFlow flow = DataFlow.Render)
            => $"{MMDEVAPI_TOKEN}{deviceId}{(flow == DataFlow.Render ? DEVINTERFACE_AUDIO_RENDER : DEVINTERFACE_AUDIO_CAPTURE)}";

        /// <summary>
        /// Parses the root device identifier from a string that represents the default device for an application process.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>The device identifier.</returns>
        private string ParseDeviceId(string deviceId)
        {
            if (deviceId.StartsWith(MMDEVAPI_TOKEN)) deviceId = deviceId.Remove(0, MMDEVAPI_TOKEN.Length);
            if (deviceId.EndsWith(DEVINTERFACE_AUDIO_RENDER)) deviceId = deviceId.Remove(deviceId.Length - DEVINTERFACE_AUDIO_RENDER.Length);
            if (deviceId.EndsWith(DEVINTERFACE_AUDIO_CAPTURE)) deviceId = deviceId.Remove(deviceId.Length - DEVINTERFACE_AUDIO_CAPTURE.Length);

            return deviceId;
        }

        /// <summary>
        /// Gets the session manager asynchronously.
        /// </summary>
        /// <returns>The session manager.</returns>
        private async Task<GlobalSystemMediaTransportControlsSessionManager> GetManagerAsync()
        {
            try
            {
                await _syncRoot.WaitAsync();

                if (this._manager == null)
                {
                    this._manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                    if (this._manager == null)
                    {
                        throw new NullReferenceException("Failed to get session manager.");
                    }
                }

                return this._manager;
            }
            finally
            {
                _syncRoot.Release();
            }
        }
    }
}
