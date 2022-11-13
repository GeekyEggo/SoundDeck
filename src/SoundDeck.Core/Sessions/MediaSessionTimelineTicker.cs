namespace SoundDeck.Core.Sessions
{
    using System;
    using System.Threading;
    using Windows.Media.Control;

    /// <summary>
    /// Provides an artificial wrapper around the <see cref="GlobalSystemMediaTransportControlsSession.TimelinePropertiesChanged"/> event that allows for more "responsive" feedback.
    /// </summary>
    public sealed class MediaSessionTimelineTicker : IDisposable
    {
        /// <summary>
        /// The default timer tick used by <see cref="ArtificialSynchronizationTimer"/>.
        /// </summary>
        private readonly TimeSpan DEFAULT_TIMER_TICK = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Private backing field for <see cref="IsActive"/>.
        /// </summary>
        private bool _isActive = false;

        /// <summary>
        /// Private backing field for <see cref="EnableRaisingEvents"/>.
        /// </summary>
        private bool _enableRaisingEvents = true;

        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaSessionTimelineTicker"/> class.
        /// </summary>
        /// <param name="session">The session to monitor.</param>
        public MediaSessionTimelineTicker(GlobalSystemMediaTransportControlsSession session)
        {
            this.Session = session;
            this.Session.PlaybackInfoChanged += this.OnPlaybackInfoChanged;

            this.ArtificialSynchronizationTimer = new Timer(this.OnArtificiallySynchronization, this, Timeout.Infinite, Timeout.Infinite);
            this.RefreshIsActive();
        }

        /// <summary>
        /// Occurs when the <see cref="GlobalSystemMediaTransportControlsSession.TimelinePropertiesChanged"/> occurs, artificially.
        /// </summary>
        public event EventHandler<TimelineEventArgs> TimelineChanged;

        /// <summary>
        /// Gets or sets a value indicating whether this instance can raise events.
        /// </summary>
        public bool EnableRaisingEvents
        {
            get => this._enableRaisingEvents;
            set
            {
                this._enableRaisingEvents = value;
                this.RefreshIsActive();
            }
        }

        /// <summary>
        /// Gets the timer responsible for artificially synchronizing the timeline.
        /// </summary>
        private Timer ArtificialSynchronizationTimer { get; }

        /// <summary>
        /// Gets or sets the information from the last timeline event.
        /// </summary>
        private TimelineEventArgs LastTimelineProperties { get; set; }

        /// <summary>
        /// Gets or sets the time the last synchronization occurred.
        /// </summary>
        private DateTime LastTimelineSync { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is actively monitoring the timeline.
        /// </summary>
        private bool IsActive
        {
            get => this._isActive;
            set
            {
                if (this._isActive == value)
                {
                    return;
                }

                this._isActive = value;
                if (value)
                {
                    this.SynchronizeTimeline();

                    this.Session.TimelinePropertiesChanged += this.OnTimelinePropertiesChanged;
                    this.ArtificialSynchronizationTimer.Change(DEFAULT_TIMER_TICK, DEFAULT_TIMER_TICK);
                }
                else
                {
                    this.Session.TimelinePropertiesChanged -= this.OnTimelinePropertiesChanged;
                    this.ArtificialSynchronizationTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
        }

        /// <summary>
        /// Gets the session.
        /// </summary>
        private GlobalSystemMediaTransportControlsSession Session { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.ArtificialSynchronizationTimer.Dispose();
            this.Session.PlaybackInfoChanged -= this.OnPlaybackInfoChanged;
            this.Session.TimelinePropertiesChanged -= this.OnTimelinePropertiesChanged;

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Artificially increments the position of the timeline based on <see cref="LastTimelineProperties"/> and <see cref="LastTimelineSync"/>.
        /// </summary>
        /// <param name="state">The state.</param>
        private void OnArtificiallySynchronization(object state)
        {
            var ticker = (MediaSessionTimelineTicker)state;
            if (!ticker.IsActive)
            {
                return;
            }

            ticker._syncRoot.Wait();
            try
            {
                if (!ticker.IsActive)
                {
                    return;
                }

                var diff = DateTime.UtcNow.Subtract(ticker.LastTimelineSync);
                var position = ticker.LastTimelineProperties.Position.Add(diff);

                ticker.TimelineChanged?.Invoke(ticker, new TimelineEventArgs(position, ticker.LastTimelineProperties.EndTime));
            }
            finally
            {
                ticker._syncRoot.Release();
            }
        }

        /// <summary>
        /// Called when <see cref="GlobalSystemMediaTransportControlsSession.PlaybackInfoChanged"/> occurs, updating the active state of timeline monitoring.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="PlaybackInfoChangedEventArgs"/> instance containing the event data.</param>
        private void OnPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
            this._syncRoot.Wait();
            try
            {
                this.RefreshIsActive();
            }
            finally
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Called when <see cref="GlobalSystemMediaTransportControlsSession.TimelinePropertiesChanged"/> occurs.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="TimelinePropertiesChangedEventArgs"/> instance containing the event data.</param>
        private void OnTimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession sender, TimelinePropertiesChangedEventArgs args)
        {
            this._syncRoot.Wait();
            try
            {
                this.SynchronizeTimeline();
            }
            finally
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Synchronizes the <see cref="LastTimelineProperties"/> and <see cref="LastTimelineSync"/> with the <see cref="Session"/>.
        /// </summary>
        private void SynchronizeTimeline()
        {
            this.LastTimelineProperties = new TimelineEventArgs(this.Session.GetTimelineProperties());
            this.LastTimelineSync = DateTime.UtcNow;

            this.TimelineChanged?.Invoke(this, this.LastTimelineProperties);
        }

        /// <summary>
        /// Sets <see cref="IsActive"/> based on the state of playback, and whether a timeline is available.
        /// </summary>
        private void RefreshIsActive()
            => this.IsActive = this.EnableRaisingEvents
                && this.Session.GetTimelineProperties().EndTime > TimeSpan.Zero
                && this.Session.GetPlaybackInfo().PlaybackStatus
                    is GlobalSystemMediaTransportControlsSessionPlaybackStatus.Changing
                    or GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
    }
}
