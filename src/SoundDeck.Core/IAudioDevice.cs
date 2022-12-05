namespace SoundDeck.Core
{
    using System;
    using NAudio.CoreAudioApi;

    /// <summary>
    /// Provides information about an audio device.
    /// </summary>
    public interface IAudioDevice
    {
        /// <summary>
        /// Occurs when the underlying device changes.
        /// </summary>
        event EventHandler DeviceChanged;

        /// <summary>
        /// Occurs when the volume has changed.
        /// </summary>
        event EventHandler<IAudioDevice, AudioVolumeNotificationData> VolumeChanged;

        /// <summary>
        /// Gets the name friendly name associated with underlying audio device.
        /// </summary>
        string DeviceName { get; }

        /// <summary>
        /// Gets the friendly name of the audio device.
        /// </summary>
        string FriendlyName { get; }

        /// <summary>
        /// Gets the flow of the audio.
        /// </summary>
        DataFlow Flow { get; }

        /// <summary>
        /// Gets the interop identifier of the audio device.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets a value indicating whether this instance represents a dynamic audio device identifier.
        /// </summary>
        bool IsDynamic { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is muted.
        /// </summary>
        bool IsMuted { get; }

        /// <summary>
        /// Gets the unique key that represents the audio device.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the role that this audio device is default for.
        /// </summary>
        Role? Role { get; }

        /// <summary>
        /// Gets the volume.
        /// </summary>
        float Volume { get; }

        /// <summary>
        /// Gets the multimedia device that this instance represents.
        /// </summary>
        /// <returns>The multimedia device.</returns>
        MMDevice GetMMDevice();
    }
}
