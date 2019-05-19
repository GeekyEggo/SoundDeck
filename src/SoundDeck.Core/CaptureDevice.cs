namespace SoundDeck.Core
{
    public class CaptureDevice
    {
        public string Id { get; set; }
        public string FriendlyName { get; set; }
        public AudioFlow Flow { get; set; }
    }
}