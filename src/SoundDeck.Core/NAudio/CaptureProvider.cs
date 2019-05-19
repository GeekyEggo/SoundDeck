using NAudio.CoreAudioApi;
using System.Collections.Generic;

namespace SoundDeck.Core.NAudio
{
    public class CaptureProvider : ICaptureProvider
    {
        public IAudioCapture GetCapture(string deviceId)
            => new WasapiAudioCapture(deviceId);

        public IEnumerable<CaptureDevice> GetDevices()
        {
            var iterator = new MMDeviceEnumerator();
            foreach (var device in iterator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active))
            {
                yield return new CaptureDevice
                {
                    Flow = device.DataFlow == DataFlow.Capture ? AudioFlow.In : AudioFlow.Out,
                    FriendlyName = device.FriendlyName,
                    Id = device.ID
                };
            }
        }
    }
}
