namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SoundDeck.Core;
    using SoundDeck.Core.Capture;
    using SoundDeck.Plugin.Models.Settings;
    using SoundDeck.Plugin.Models.UI;

    /// <summary>
    /// Provides capturing of an audio buffer, similar to an instat replay.
    /// </summary>
    [StreamDeckAction("com.geekyeggo.sounddeck.clipaudio")]
    public class ClipAudio : CaptureActionBase<ClipAudioSettings, IAudioBuffer>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClipAudio"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="audioBufferService">The audio buffer service.</param>
        /// <param name="folderBrowserDialogProvider">The folder browser dialog.</param>
        public ClipAudio(IAudioService audioService, IAudioBufferService audioBufferService, IFolderBrowserDialogProvider folderBrowserDialogProvider)
            : base(audioService, folderBrowserDialogProvider)
        {
            this.AudioBufferService = audioBufferService;
        }

        /// <summary>
        /// Gets the audio buffer service.
        /// </summary>
        public IAudioBufferService AudioBufferService { get; }

        /// <summary>
        /// Gets the capture device, for the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The capture device.</returns>
        protected sealed override IAudioBuffer GetCaptureDevice(ClipAudioSettings settings)
        {
            return !string.IsNullOrWhiteSpace(settings?.CaptureAudioDeviceId)
                ? this.AudioBufferService.GetAudioBuffer(settings.CaptureAudioDeviceId, settings.Duration)
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
                if (this.CaptureDevice == null)
                {
                    throw new NullReferenceException($"Unable to capture audio for {args.Context}; the capture device is null.");
                }

                var settings = args.Payload.GetSettings<ClipAudioSettings>();
                if (this.CaptureDevice != null)
                {
                    var path = await this.CaptureDevice.SaveAsync(settings);

                    this.Logger.LogInformation($"Saved captured from device \"{settings.CaptureAudioDeviceId}\" to \"{path}\".");
                    await this.ShowOkAsync();
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, $"Failed to capture audio.");
                await this.ShowAlertAsync();
            }
        }
    }
}
