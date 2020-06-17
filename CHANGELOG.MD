# Change Log

## Unreleased

**Added**
- Sampler Action
  - Press-and-hold to record a sample.
  - Press again to playback sample.
  - Record any audio device (capture and playback).
  - Save as WAV, or encode to MP3, automatic volume normalization.
- Stop Audio
  - Stops all audio playback from any *Play Audio* or *Sampler* action.
- Added new action types to Play Audio action.
  - Play All / Stop; plays the playlist from the next track, or stops playback if a track is playing.
  - Loop All / Stop; loops the playlist from the next track, or stops playback if a track is playing.
  - Loop All / Stop (Reset); loops the playlist from the first track, or stops playback if a track is playing.

**Fixed**
- Greatly reduced CPU usage when playing audio files.
- Fixed an issue when moving actions around could cause incorrect playback.

**Changed**
- Removed drag-handle within Play Audio settings when order was sequential.

## 1.0.0

**Added**
- Clip Audio Action
  - Quickly record the last *x* seconds of audio.
  - Supports any audio device (capture and playback).
  - Save as WAV, or encode to MP3, automatic volume normalization.
- Play Audio Action
  - Play audio tracks to any playback device.
  - Create playlists of tracks to play, in random or order.
  - Configure different play actions
    - Play / Next
    - Play / Stop
    - Loop / Stop
- Record Audio Action
  - Record any audio device (capture and playback).
  - Start/stop recording with button feedback.
  - Save as WAV, or encode to MP3, automatic volume normalization.
