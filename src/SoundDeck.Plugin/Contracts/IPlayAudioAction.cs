namespace SoundDeck.Plugin.Contracts
{
    using System.Threading.Tasks;
    using SharpDeck.Enums;
    using SoundDeck.Core;
    using SoundDeck.Core.Playback;

    /// <summary>
    /// Provides information about an action used to play audio.
    /// </summary>
    public interface IPlayAudioAction
    {
        /// <summary>
        /// Gets the audio service.
        /// </summary>
        IAudioService AudioService { get; }

        /// <summary>
        /// Gets or sets the playlist controller.
        /// </summary>
        IPlaylistController PlaylistController { get; set; }

        /// <summary>
        /// Dynamically change the title of an instance of an action.
        /// </summary>
        /// <param name="title">The title to display. If no title is passed, the title is reset to the default title from the manifest.</param>
        /// <param name="target">Specify if you want to display the title on the hardware and software, only on the hardware, or only on the software.</param>
        /// <param name="state">A 0-based integer value representing the state of an action with multiple states. This is an optional parameter. If not specified, the title is set to all states.</param>
        Task SetTitleAsync(string title = "", TargetType target = TargetType.Both, int? state = null);
    }
}
