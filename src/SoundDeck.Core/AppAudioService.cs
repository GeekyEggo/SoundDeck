namespace SoundDeck.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using NAudio.CoreAudioApi;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Core.Interop;
    using SoundDeck.Core.Interop.Helpers;
    using SoundDeck.Core.Sessions;
    using SoundDeck.Core.Volume;
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
        public string GetDefaultAudioDevice(uint processId, DataFlow flow)
        {
            try
            {
                this.AudioPolicyConfig.GetPersistedDefaultAudioEndpoint(processId, flow, Role.Multimedia | Role.Console, out var deviceId);
                return this.ParseDeviceId(deviceId);
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public void SetDefaultAudioDevice(IProcessSelectionCriteria criteria, string deviceKey)
        {
            var dataFlow = this.GetDataFlow(deviceKey);
            var predicate = criteria.ToPredicate();

            foreach (var audioSession in this.GetAudioSessions().Where(predicate.IsMatch))
            {
                // Default to zero pointer; this will only change if an audio device has been specified.
                var hstring = IntPtr.Zero;
                var device = AudioDevices.Current.GetDeviceByKey(deviceKey);
                if (!device.IsDynamic)
                {
                    var persistDeviceId = this.GenerateDeviceId(device.Id, dataFlow);
                    Combase.WindowsCreateString(persistDeviceId, (uint)persistDeviceId.Length, out hstring);
                }

                // Set the audio device for the process.
                var audioSessionProcessId = audioSession.GetProcessID;
                this.AudioPolicyConfig.SetPersistedDefaultAudioEndpoint(audioSessionProcessId, dataFlow, Role.Console, hstring);
                this.AudioPolicyConfig.SetPersistedDefaultAudioEndpoint(audioSessionProcessId, dataFlow, Role.Multimedia, hstring);
                this.AudioPolicyConfig.SetPersistedDefaultAudioEndpoint(audioSessionProcessId, dataFlow, Role.Communications, hstring);
            }
        }

        /// <inheritdoc/>
        public void SetVolume<T>(T settings)
            where T : IProcessSelectionCriteria, IVolumeSettings
        {
            var predicate = settings.ToPredicate();
            foreach (var audioSession in this.GetAudioSessions().Where(predicate.IsMatch))
            {
                audioSession.SimpleAudioVolume.Set(settings);
            }
        }

        /// <inheritdoc/>
        public async Task ControlAsync(IProcessSelectionCriteria criteria, MultimediaAction action)
        {
            var predicate = criteria.ToPredicate();
            var sessions = await this.GetMultimediaSessionAsync();

            foreach (var session in sessions.Where(predicate.IsMatch))
            {
                await (action switch
                {
                    MultimediaAction.Play => session.TryPlayAsync(),
                    MultimediaAction.Pause => session.TryPauseAsync(),
                    MultimediaAction.Stop => session.TryStopAsync(),
                    MultimediaAction.SkipPrevious => session.TrySkipPreviousAsync(),
                    MultimediaAction.SkipNext => session.TrySkipNextAsync(),
                    _ => session.TryTogglePlayPauseAsync()
                });
            }
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<GlobalSystemMediaTransportControlsSession>> GetMultimediaSessionAsync()
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

                return this._manager.GetSessions();
            }
            finally
            {
                _syncRoot.Release();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<AudioSessionControl> GetAudioSessions()
        {
            using (var deviceEnumerator = new MMDeviceEnumerator())
            {
                var sessions = deviceEnumerator
                    .EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active)
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
        /// Gets the data flow from the specified <paramref name="deviceKey"/>.
        /// </summary>
        /// <param name="deviceKey">The audio device key.</param>
        /// <returns>The interop data flow.</returns>
        private DataFlow GetDataFlow(string deviceKey)
        {
            var device = AudioDevices.Current.GetDeviceByKey(deviceKey);
            if (device == null)
            {
                throw new KeyNotFoundException($"Unable to find audio device with key {deviceKey}.");
            }

            return device.Flow;
        }

        /// <summary>
        /// Generates the device identifier that can be used to set the persisted default audio endpoint for a process.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="flow">The flow.</param>
        /// <returns>The device identifier.</returns>
        private string GenerateDeviceId(string deviceId, DataFlow flow)
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
    }
}
