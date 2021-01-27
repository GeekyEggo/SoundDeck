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
    [StreamDeckAction("com.geekyeggo.sounddeck.clearsample")]
    [StreamDeckActionState("Images/SamplerClear/Key0")]
    [StreamDeckActionState("Images/SamplerClear/Key1")]
    public class SamplerClearer : StreamDeckAction
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private static readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

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
        /// Occurs when <see cref="IStreamDeckConnection.WillAppear" /> is received for this instance.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs`1" /> instance containing the event data.</param>
        /// <returns>The task of handling the event.</returns>
        protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
        {
            try
            {
                await _syncRoot.WaitAsync();

                await base.OnWillAppear(args);
                SamplerClearer.IsActiveChanged += this.SamplerClear_StateChanged;

                this.RefreshState();
            }
            finally
            {
                _syncRoot.Release();
            }
        }

        /// <summary>
        /// Occurs when <see cref="IStreamDeckConnection.WillDisappear" /> is received for this instance.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs`1" /> instance containing the event data.</param>
        protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
        {
            try
            {
                await _syncRoot.WaitAsync();

                await base.OnWillDisappear(args);
                SamplerClearer.IsActiveChanged -= this.SamplerClear_StateChanged;
            }
            finally
            {
                _syncRoot.Release();
            }
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
