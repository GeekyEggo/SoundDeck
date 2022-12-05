namespace SoundDeck.Core
{
    /// <summary>
    /// Provides a delegate that has a typed sender and arguments.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender.</typeparam>
    /// <typeparam name="TArgs">The type of the arguments.</typeparam>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The arguments.</param>
    public delegate void EventHandler<TSender, TArgs>(TSender sender, TArgs args);
}
