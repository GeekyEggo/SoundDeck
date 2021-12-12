namespace SoundDeck.Core.Playback
{
    using System;
    using SoundDeck.Core.Playback.Readers;

    /// <summary>
    /// Provides event arguments for the time of a <see cref="AudioPlayer"/> changing.
    /// </summary>
    public class PlaybackTimeEventArgs : EventArgs
    {
        /// <summary>
        /// A <see cref="PlaybackTimeEventArgs"/> where <see cref="Current"/> and <see cref="Total"/> are both <see cref="TimeSpan.Zero"/>;
        /// </summary>
        public static readonly PlaybackTimeEventArgs Zero = new PlaybackTimeEventArgs(TimeSpan.Zero, TimeSpan.Zero);

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackTimeEventArgs"/> class.
        /// </summary>
        /// <param name="current">The current time.</param>
        /// <param name="total">The total time.</param>
        internal PlaybackTimeEventArgs(TimeSpan current, TimeSpan total)
        {
            this.Current = current;
            this.Total = total;
        }

        /// <summary>
        /// Gets the current time.
        /// </summary>
        public TimeSpan Current { get; }

        /// <summary>
        /// Gets the total time.
        /// </summary>
        public TimeSpan Total { get; }

        /// <summary>
        /// Gets the event arguments from a <see cref="IAudioFileReader"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The event arguments.</returns>
        internal static PlaybackTimeEventArgs FromReader(IAudioFileReader reader)
        {
            try
            {
                return new PlaybackTimeEventArgs(reader.CurrentTime, reader.TotalTime);
            }
            catch
            {
                return PlaybackTimeEventArgs.Zero;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is PlaybackTimeEventArgs other)
            {
                return this.Current.TotalSeconds == other.Current.TotalSeconds
                    && this.Total.TotalSeconds == other.Total.TotalSeconds;
            }

            return this == null && obj == null;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
            => base.GetHashCode();
    }
}
