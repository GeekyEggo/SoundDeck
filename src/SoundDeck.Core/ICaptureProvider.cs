using System.Collections.Generic;

namespace SoundDeck.Core
{
    public interface ICaptureProvider
    {
        IAudioCapture GetCapture(string deviceId);
        IEnumerable<CaptureDevice> GetDevices();
    }
}
