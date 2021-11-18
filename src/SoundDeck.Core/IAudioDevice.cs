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
        /// Occurs when the underlying <see cref="IAudioDevice.Id"/> changes.
        /// </summary>
        event EventHandler IdChanged;

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
        /// Gets a value indicating whether this instance represents a read-only identifier.
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets the unique key that represents the audio device.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the role that this audio device is default for.
        /// </summary>
        Role? Role { get; }

        /// <summary>
        /// Gets the multimedia device that this instance represents.
        /// </summary>
        /// <returns>The multimedia device.</returns>
        MMDevice GetMMDevice();
    }
}
