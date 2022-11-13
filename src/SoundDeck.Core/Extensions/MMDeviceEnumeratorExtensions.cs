namespace SoundDeck.Core.Extensions
{
    using System.Collections.Generic;
    using NAudio.CoreAudioApi;

    /// <summary>
    /// Provides extension methods for <see cref="MMDeviceEnumerator"/>.
    /// </summary>
    public static class MMDeviceEnumeratorExtensions
    {
        /// <summary>
        /// Gets the <see cref="AudioSessionControl"/> associated with the <see cref="AudioSessionManager"/> for each device.
        /// </summary>
        /// <param name="deviceEnumerator">The <see cref="MMDeviceEnumerator"/>.</param>
        /// <param name="dataFlow">The <see cref="DataFlow"/> to filter by.</param>
        /// <param name="deviceState">The <see cref="DeviceState"/> to filter by.</param>
        /// <returns>The collection of <see cref="AudioSessionControl"/>.</returns>
        public static IEnumerable<AudioSessionControl> GetAudioSessions(this MMDeviceEnumerator deviceEnumerator, DataFlow dataFlow = DataFlow.All, DeviceState deviceState = DeviceState.Active)
        {
            foreach (var device in deviceEnumerator.EnumerateAudioEndPoints(dataFlow, deviceState))
            {
                if (device.AudioSessionManager == null)
                {
                    continue;
                }

                for (var i = 0; i < device.AudioSessionManager.Sessions.Count; i++)
                {
                    yield return device.AudioSessionManager.Sessions[i];
                }
            }
        }
    }
}
