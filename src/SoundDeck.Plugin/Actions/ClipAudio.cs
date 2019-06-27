namespace SoundDeck.Plugin.Actions
{
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;
    using SoundDeck.Core;
    using SoundDeck.Core.Capture;
    using SoundDeck.Plugin.Models.Settings;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides capturing of an audio buffer, similar to an instat replay.
    /// </summary>
    [StreamDeckAction("Clip Audio", UUID, "Images/ClipAudio/Action", Tooltip = "Clip those last precious seconds of audio; you'll never miss a funny moment again.")]
    [StreamDeckActionState("Images/ClipAudio/Key")]
    public class ClipAudio : BaseCaptureAction<ClipAudioSettings, IAudioBuffer>
    {
        /// <summary>
        /// The unique identifier for the action.
        /// </summary>
        public const string UUID = "com.geekyEggo.soundDeck.clipAudio";

        /// <summary>
        /// Initializes a new instance of the <see cref="ClipAudio"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        public ClipAudio(IAudioService audioService, ActionEventArgs<AppearancePayload> args)
            : base(audioService)
        {
            this.CaptureDevice = this.GetCaptureDevice(args.Payload.GetSettings<ClipAudioSettings>());
        }

        /// <summary>
        /// Gets the capture device, for the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The capture device.</returns>
        protected sealed override IAudioBuffer GetCaptureDevice(ClipAudioSettings settings)
        {
            return !string.IsNullOrWhiteSpace(settings?.AudioDeviceId)
                ? this.AudioService.GetAudioBuffer(settings.AudioDeviceId, settings.Duration)
                : null;
        }

        /// <summary>
        /// Raises the <see cref="E:SharpDeck.StreamDeckActionEventReceiver.DidReceiveSettings" /> event.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        /// <returns>The task of updating the state of the object based on the settings.</returns>
        protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, ClipAudioSettings settings)
        {
            await base.OnDidReceiveSettings(args, settings);
            if (this.CaptureDevice != null)
            {
                this.CaptureDevice.BufferDuration = settings.Duration;
            }
        }

        /// <summary>
        /// Occurs when the user presses a key.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        /// <returns>The task of saving the buffer to a file.</returns>
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            try
            {
                var settings = args.Payload.GetSettings<ClipAudioSettings>();
                if (this.CaptureDevice != null)
                {
                    var path = await this.CaptureDevice.SaveAsync(settings);

                    await this.StreamDeck.LogMessageAsync($"Saved captured from device {settings.AudioDeviceId} to {path}");
                    await this.ShowOkAsync();
                }
            }
            catch (Exception ex)
            {
                await this.StreamDeck.LogMessageAsync(ex.Message);
                await this.ShowAlertAsync();
            }
        }
    }
}
