namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;
    using SoundDeck.Plugin.Events;

    /// <summary>
    /// Provides an action that enables clearing of samples.
    /// </summary>
    [StreamDeckAction("Clear Sampler", UUID, "Images/SamplerClear/Action", Tooltip = "Activate sampler clearing.")]
    [StreamDeckActionState("Images/SamplerClear/Key0")]
    [StreamDeckActionState("Images/SamplerClear/Key1")]
    public class SamplerClearer : StreamDeckAction
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private static readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// The unique identifier for the action.
        /// </summary>
        public const string UUID = "com.geekyeggo.sounddeck.clearsample";

        /// <summary>
        /// The active state.
        /// </summary>
        private const int ACTIVE_STATE = 1;

        /// <summary>
        /// The inactive state
        /// </summary>
        private const int INACTIVE_STATE = 0;

        /// <summary>
        /// Gets a value indicating whether sample clearing is active.
        /// </summary>
        public static bool IsActive { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SamplerClearer"/> class.
        /// </summary>
        public SamplerClearer()
        {
            SamplerClearer.IsActiveChanged += this.SamplerClear_StateChanged;
        }

        /// <summary>
        /// Occurs when <see cref="SamplerClearer.IsActive"/> changes.
        /// </summary>
        private static event MultiStateChangedEventHandler IsActiveChanged;

        /// <summary>
        /// Sets <see cref="IsActive"/> and raises the <see cref="IsActiveChanged"/> event.
        /// </summary>
        /// <param name="value">The new value to set <see cref="IsActive"/>.</param>
        /// <param name="sender">The sender setting the value.</param>
        public static void SetIsActive(bool value, StreamDeckAction sender)
        {
            try
            {
                _syncRoot.Wait();

                if (SamplerClearer.IsActive != value)
                {
                    SamplerClearer.IsActive = value;
                    IsActiveChanged?.Invoke(sender, EventArgs.Empty);
                }
            }
            finally
            {
                _syncRoot.Release();
            }
        }

        /// <summary>
        /// Occurs when this instance is initialized.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs`1" /> instance containing the event data.</param>
        protected override void OnInit(ActionEventArgs<AppearancePayload> args)
        {
            base.OnInit(args);
            this.RefreshState();
        }

        /// <summary>
        /// Occurs when <see cref="KeyDown" /> is received for this instance.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs`1" /> instance containing the event data.</param>
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            await base.OnKeyDown(args);
            SetIsActive(args.Payload.State == INACTIVE_STATE, this);
        }

        /// <summary>
        /// Handles the <see cref="SamplerClearer.IsActiveChanged"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SamplerClear_StateChanged(StreamDeckAction sender, EventArgs args)
        {
            if (sender.Context != this.Context)
            {
                this.RefreshState();
            }
        }

        /// <summary>
        /// Refreshes the state of this action based on <see cref="SamplerClearer.IsActive"/>.
        /// </summary>
        private async void RefreshState()
        {
            await this.SetStateAsync(SamplerClearer.IsActive ? ACTIVE_STATE : INACTIVE_STATE)
                .ConfigureAwait(false);
        }
    }
}
