namespace SoundDeck.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using NAudio.CoreAudioApi;
    using SoundDeck.Core.Interop;

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
        /// Gets the audio policy configuration facotry.
        /// </summary>
        private IAudioPolicyConfigFactory AudioPolicyConfig { get; } = AudioPolicyConfigFactory.Create();

        /// <summary>
        /// Gets the foreground application process identifier.
        /// </summary>
        /// <returns>The process identifier.</returns>
        public uint GetForegroundAppProcessId()
        {
            var hwnd = User32.GetForegroundWindow();
            User32.GetWindowThreadProcessId(hwnd, out var pid);

            return pid;
        }

        /// <summary>
        /// Gets the default audio device for the specified process.
        /// </summary>
        /// <param name="processId">The process identifier.</param>
        /// <param name="flow">The audio flow; either input or output.</param>
        /// <returns>The audio device; otherwise <c>null</c>.</returns>
        public string GetDefaultAudioDevice(uint processId, AudioFlowType flow)
        {
            try
            {
                var dataFlow = this.GetDataFlow(flow);
                this.AudioPolicyConfig.GetPersistedDefaultAudioEndpoint(processId, dataFlow, Role.Multimedia | Role.Console, out string deviceId);

                return this.ParseDeviceId(deviceId);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the default audio device for the specified process.
        /// </summary>
        /// <param name="processId">The process identifier.</param>
        /// <param name="flow">The audio flow; either input or output.</param>
        /// <param name="deviceId">The device identifier.</param>
        public void SetDefaultAudioDevice(uint processId, AudioFlowType flow, string deviceId)
        {
            var processName = Process.GetProcessById((int)processId).ProcessName;
            this.SetDefaultAudioDevice(processName, flow, deviceId);
        }

        /// <summary>
        /// Sets the default audio device for the specified process.
        /// </summary>
        /// <param name="processName">The process name.</param>
        /// <param name="audioFlow">The audio flow; either input or output.</param>
        /// <param name="deviceId">The device identifier.</param>
        public void SetDefaultAudioDevice(string processName, AudioFlowType audioFlow, string deviceId)
        {
            var flow = this.GetDataFlow(audioFlow);
            if (this.TryGetAudioSessionProcessId(processName, flow, out var audioSessionProcessId))
            {
                // Default to zero pointer; this will only change if an audio device has been specified.
                var hstring = IntPtr.Zero;
                if (!AudioDevices.Current.IsDefaultPlaybackDevice(deviceId))
                {
                    using (var device = AudioDevices.Current.GetDevice(deviceId))
                    {
                        var persistDeviceId = this.GenerateDeviceId(device.ID);
                        Combase.WindowsCreateString(persistDeviceId, (uint)persistDeviceId.Length, out hstring);
                    }
                }

                // Set the audio device for the process.
                this.AudioPolicyConfig.SetPersistedDefaultAudioEndpoint(audioSessionProcessId, flow, Role.Multimedia, hstring);
                this.AudioPolicyConfig.SetPersistedDefaultAudioEndpoint(audioSessionProcessId, flow, Role.Console, hstring);
            }
        }

        /// <summary>
        /// Tries the get audio session process identifier.
        /// </summary>
        /// <param name="processName">Name of the process.</param>
        /// <param name="flow">The flow.</param>
        /// <param name="audioSessionProcessId">The audio session process identifier.</param>
        /// <returns></returns>
        private bool TryGetAudioSessionProcessId(string processName, DataFlow flow, out uint audioSessionProcessId)
        {
            foreach (var audioSession in this.GetAudioSessions(flow))
            {
                audioSessionProcessId = audioSession.GetProcessID;
                if (Process.GetProcessById((int)audioSessionProcessId).ProcessName == processName)
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
