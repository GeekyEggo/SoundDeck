namespace SoundDeck.Plugin.Events
{
    using System;
    using SharpDeck;

    /// <summary>
    /// Provides an event handler invoked when the state of an action changes.
    /// </summary>
    /// <param name="sender">The <see cref="StreamDeckAction" /> instance containing the event data.</param>
    /// <param name="args">The arguments, always <see cref="EventArgs.Empty"/>.</param>
    public delegate void MultiStateChangedEventHandler(StreamDeckAction sender, EventArgs args);
}
