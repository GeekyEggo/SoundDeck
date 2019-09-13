namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Threading.Tasks;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;
    using SoundDeck.Core;

    /// <summary>
    /// Provides an action that can stop all audio currently being output by Sound Deck.
    /// </summary>
    [StreamDeckAction("Stop Audio", UUID, "Images/StopAudio/Action", Tooltip = "Stops all audio being played through Sound Deck.")]
    [StreamDeckActionState("Images/StopAudio/Key")]
    public class StopAudio : StreamDeckAction
    {
        /// <summary>
        /// The unique identifier for the action.
        /// </summary>
        public const string UUID = "com.geekyeggo.sounddeck.stopaudio";

        /// <summary>
        /// Initializes a new instance of the <see cref="StopAudio"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        public StopAudio(IAudioService audioService)
            : base()
        {
            this.AudioService = audioService;
        }

        /// <summary>
        /// Gets the audio service.
        /// </summary>
        public IAudioService AudioService { get; }

        /// <summary>
        /// Occurs when the user presses a key.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs" /> instance containing the event data.</param>
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            try
            {
                await base.OnKeyDown(args);
                this.AudioService.StopAll();
            }
            catch (Exception e)
            {
                await this.StreamDeck.LogMessageAsync(e.ToString());
                await this.ShowAlertAsync();
            }
        }
    }
}
