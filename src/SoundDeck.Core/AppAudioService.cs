namespace SoundDeck.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using NAudio.CoreAudioApi;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Core.Interop;
    using SoundDeck.Core.Interop.Helpers;

    /// <summary>
    /// Provides a service for controlling and interacting with the audio device of an application.
    /// </summary>
    public class AppAudioService : IAppAudioService
    {
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
        public void SetDefaultAudioDevice(uint processId, AudioFlowType flow, string deviceKey)
        {
            var processName = Process.GetProcessById((int)processId).ProcessName;
            this.SetDefaultAudioDevice(processName, flow, deviceKey);
        }

        //// <inheritdoc/>
        public void SetDefaultAudioDevice(string processName, AudioFlowType flow, string deviceKey)
        {
            var dataFlow = this.GetDataFlow(flow);
            if (this.TryGetAudioSessionProcessId(processName, dataFlow, out var audioSessionProcessId))
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

        /// <inheritdoc/>
        public void SetDefaultAudioDeviceForForegroundApp(AudioFlowType flow, string deviceKey)
        {
            var hwnd = User32.GetForegroundWindow();
            User32.GetWindowThreadProcessId(hwnd, out var pid);

            this.SetDefaultAudioDevice(pid, flow, deviceKey);
        }

        /// <summary>
        /// Tries the get audio session process identifier.
        /// </summary>
        /// <param name="processName">Name of the process.</param>
        /// <param name="flow">The flow.</param>
        /// <param name="audioSessionProcessId">The audio session process identifier.</param>
        /// <returns><c>true</c> when the audio session was retrieved for the <paramref name="processName"/>; otherwise <c>false</c>.</returns>
        private bool TryGetAudioSessionProcessId(string processName, DataFlow flow, out uint audioSessionProcessId)
        {
            const string DEFAULT_PROCESS_EXTENSION = ".exe";
            foreach (var audioSession in this.GetAudioSessions(flow))
            {
                audioSessionProcessId = audioSession.GetProcessID;

                // Ensure both the process name we're looking for, and the audio session process name, don't end with ".exe".
                var audioSessionProcessName = Process.GetProcessById((int)audioSessionProcessId).ProcessName.TrimEnd(DEFAULT_PROCESS_EXTENSION, StringComparison.OrdinalIgnoreCase);
                processName = processName.TrimEnd(DEFAULT_PROCESS_EXTENSION, StringComparison.OrdinalIgnoreCase);

                // When there is a case insensitive match, we're good!
                if (audioSessionProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            audioSessionProcessId = 0;
            return false;
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
    }
}
