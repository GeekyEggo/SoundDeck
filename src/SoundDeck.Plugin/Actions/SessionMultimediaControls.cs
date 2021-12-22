namespace SoundDeck.Plugin.Actions
{
    using System.Threading.Tasks;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides an action that is capable of controlling media for a specific app.
    /// </summary>
    [StreamDeckAction("com.geekyeggo.sounddeck.sessionmultimediacontrols")]
    public class SessionMultimediaControls : StreamDeckAction<SessionMultimediaControlsSettings>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionMultimediaControls"/> class.
        /// </summary>
        /// <param name="multimediaControlService">The multimedia control service.</param>
        public SessionMultimediaControls(IMultimediaControlsService multimediaControlsService)
           : base()
        {
            this.MultimediaControlsService = multimediaControlsService;
        }

        /// <summary>
        /// Gets the multimedia controls service.
        /// </summary>
        private IMultimediaControlsService MultimediaControlsService { get; }

        /// <summary>
        /// Occurs when <see cref="SharpDeck.Connectivity.IStreamDeckConnection.KeyDown" /> is received for this instance.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs`1" /> instance containing the event data.</param>
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            try
            {
                var settings = args.Payload.GetSettings<SessionMultimediaControlsSettings>();

                await this.MultimediaControlsService.TryControlAsync(settings.SearchCriteria, settings.Action);
                await this.ShowOkAsync();
            }
            catch
            {
                await this.ShowAlertAsync();
            }
        }
    }
}
