namespace SoundDeck.Core.Playback
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides a collection of <see cref="IAudioPlayer"/>; players are removed from the collection when they're disposed.
    /// </summary>
    public class AudioPlayerCollection
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private static readonly object _syncRoot = new object();

        /// <summary>
        /// Gets the players.
        /// </summary>
        private List<IAudioPlayer> Players { get; } = new List<IAudioPlayer>();

        /// <summary>
        /// Adds the specified player to the collection.
        /// </summary>
        /// <param name="player">The player.</param>
        public void Add(IAudioPlayer player)
        {
            lock (_syncRoot)
            {
                player.Disposed += this.Player_Disposed;
                this.Players.Add(player);
            }
        }

        /// <summary>
        /// Stops all players within the collection.
        /// </summary>
        public void StopAll()
        {
            lock (_syncRoot)
            {
                this.Players.ForEach(p => p.Stop());
            }
        }

        /// <summary>
        /// Handles the <see cref="IAudioPlayer.Disposed"/> event for players within <see cref="Players"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Player_Disposed(object sender, EventArgs e)
        {
            if (sender is IAudioPlayer player)
            {
                lock (_syncRoot)
                {
                    player.Disposed -= this.Player_Disposed;
                    this.Players.Remove(player);
                }
            }
        }
    }
}
