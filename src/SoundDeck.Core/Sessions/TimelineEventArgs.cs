namespace SoundDeck.Core.Sessions
{
    using System;
    using Windows.Media.Control;

    /// <summary>
    /// Provides information about a timeline event.
    /// </summary>
    public class TimelineEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the empty <see cref="TimelineEventArgs"/>.
        /// </summary>
        public static new TimelineEventArgs Empty { get; } = new TimelineEventArgs(TimeSpan.Zero, TimeSpan.Zero);

        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineEventArgs"/> class.
        /// </summary>
        /// <param name="timeline">The timeline.</param>
        public TimelineEventArgs(GlobalSystemMediaTransportControlsSessionTimelineProperties timeline)
            : this(timeline.Position, timeline.EndTime)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineEventArgs"/> class.
        /// </summary>
        /// <param name="position">The current position.</param>
        /// <param name="endTime">The end time.</param>
        public TimelineEventArgs(TimeSpan position, TimeSpan endTime)
        {
            this.EndTime = endTime;
            this.Position = position;
        }

        /// <summary>
        /// Gets the end time.
        /// </summary>
        public TimeSpan EndTime { get; }

        /// <summary>
        /// Gets the current position.
        /// </summary>
        public TimeSpan Position { get; }
    }
}
