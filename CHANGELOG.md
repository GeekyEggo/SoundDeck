# Change Log

## vNext

#### ⭐ Added
- Long-pressing "Play Audio" now resets the current audio to the first item in the playlist.
- Support for AIFF audio files.
- Support for OGG audio files.
- Support for Stream Deck audio files.

#### 🐞 Fixed
- Fixed an issue whereby "Set App Audio Device" would fail if there was a missing Windows component (added fallback).

#### ♻ Changed
- Updated NAudio dependency.

## 2.0.0

#### ⭐ Added
- "Sampler" action.
  - Press-and-hold to record a sample.
  - Press again to playback sample.
  - Record any audio device (capture and playback).
  - Save as WAV, or encode to MP3, automatic volume normalization.
- "Set App Audio Device" action.
  - Change the audio device for the foreground window.
  - Or, change the audio device for a specific application.
  - Great for Wave:1 and Wave:3 users!
- "Stop Audio" action.
  - Stops all audio playback from any "Play Audio" or "Sampler" action.
- "Play Audio" action enhancements.
  - Volume control! Individually control the volume of your audio clips.
  - New action types:
    - Play All / Stop; plays the playlist from the next track, or stops playback if a track is playing.
    - Loop All / Stop; loops the playlist from the next track, or stops playback if a track is playing.
    - Loop All / Stop (Reset); loops the playlist from the first track, or stops playback if a track is playing.

#### 🐞 Fixed
- Greatly reduced CPU usage when playing audio files.
- Fixed audio time display.
- Fixed an issue when moving actions around could cause incorrect playback.

#### ♻ Changed
- Removed drag-handle within Play Audio settings when order was sequential.

## 1.0.0

#### ⭐ Added
- "Clip Audio" action.
  - Quickly record the last *x* seconds of audio.
  - Supports any audio device (capture and playback).
  - Save as WAV, or encode to MP3, automatic volume normalization.
- "Play Audio" action.
  - Play audio tracks to any playback device.
  - Create playlists of tracks to play, in random or order.
  - Configure different play actions.
    - Play / Next.
    - Play / Stop.
    - Loop / Stop.
- "Record Audio" action.
  - Record any audio device (capture and playback).
  - Start/stop recording with button feedback.
  - Save as WAV, or encode to MP3, automatic volume normalization.
