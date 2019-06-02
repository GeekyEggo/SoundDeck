namespace SoundDeck.Plugin.Models.Payloads
{
    using SharpDeck.PropertyInspectors;
    using SoundDeck.Core;

    public class AudioDevicesPayload : PropertyInspectorPayload
    {
        public AudioDevice[] Devices { get; set; }
    }
}
