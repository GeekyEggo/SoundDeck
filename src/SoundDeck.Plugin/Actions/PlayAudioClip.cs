namespace SoundDeck.Plugin.Actions
{
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;
    using SoundDeck.Plugin.Models.Settings;
    using System.Threading.Tasks;

    [StreamDeckAction("Play Audio", UUID, "Images/Action", Tooltip = "Play an audio clip.")]
    [StreamDeckActionState("Images/Action")]
    public class PlayAudioClip  : StreamDeckAction<PlayAudioClipSettings>
    {
        /// <summary>
        /// The unique identifier for the action.
        /// </summary>
        public const string UUID = "com.geekyEggo.soundDeck.playAudioClip";

        /// <summary>
        /// Occurs when the user presses a key.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args)
            => base.OnKeyDown(args);
    }
}
