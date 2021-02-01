namespace SoundDeck.Core.Playback.Playlists
{
    using System;

    /// <summary>
    /// Provides information about a user defined playlist.
    /// </summary>
    public class UserDefinedPlaylist : IPlaylist
    {
        /// <summary>
        /// Private member field for <see cref="Files"/>.
        /// </summary>
        private AudioFileInfo[] _files;

        /// <summary>
        /// Private member field for <see cref="Order"/>.
        /// </summary>
        private PlaybackOrderType _order;

        /// <summary>
        /// Occurs when the playlist has changed.
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        public AudioFileInfo[] Files
        {
            get => this._files;
            set
            {
                void Set()
                {
                    this._files = value;
                    this.Changed?.Invoke(this, EventArgs.Empty);
                }

                // When both values are null, do nothing.
                if (this._files == null
                    && value == null)
                {
                    return;
                }

                // When the collection lengths differ, set the items again.
                if (this._files?.Length != value?.Length)
                {
                    Set();
                    return;
                }

                // Whilst the items are sequentially the same, set their volumes; otherwise simply reset the items.
                for (var i = 0; i < this._files.Length; i++)
                {
                    if (this._files[i].Path == value[i].Path)
                    {
                        this._files[i].Volume = value[i].Volume;
                    }
                    else
                    {
                        Set();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public PlaybackOrderType Order
        {
            get => this._order;
            set
            {
                if (this._order != value)
                {
                    this._order = value;
                    this.Changed?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
