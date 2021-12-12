namespace SoundDeck.Core.Playback.Players
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides a collection of <see cref="IStopper"/> that allows for management of playback from any audio produced by Sound Deck.
    /// </summary>
    public class AudioPlaybackManager
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private static readonly object _syncRoot = new object();

        /// <summary>
        /// Gets the players.
        /// </summary>
        private List<IStopper> Players { get; } = new List<IStopper>();

        /// <summary>
        /// Adds the specified player to the collection.
        /// </summary>
        /// <param name="player">The player.</param>
        public void Add(IStopper player)
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
            if (sender is IStopper player)
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
