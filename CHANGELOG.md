# Change Log

## v3.7.2

### ⭐ Added

- New action, "App Media Controls", control playback of a specific applications, i.e. Chrome, Spotify, VLC etc. ([#63](https://github.com/GeekyEggo/SoundDeck/issues/63))
- New action, "Set App Volume", set the volume of an individual applications ([#63](https://github.com/GeekyEggo/SoundDeck/issues/63)).
- New action, "Set Default Audio Device" ([#64](https://github.com/GeekyEggo/SoundDeck/issues/64)).
- New action, "Set Audio Device Volume" ([#64](https://github.com/GeekyEggo/SoundDeck/issues/64)).
- "Set App Audio Device" action now supports input devices ([#63](https://github.com/GeekyEggo/SoundDeck/issues/63)). 
- Added option to select default audio device for recording actions ([#54](https://github.com/GeekyEggo/SoundDeck/issues/54)).
- Added "Play / Overlap" option to playback actions ([#60](https://github.com/GeekyEggo/SoundDeck/issues/60)).
- Added playback volume control to "Sampler" action ([#61](https://github.com/GeekyEggo/SoundDeck/issues/61)).

### 🐞 Fixed

- Fixed an error that would cause playback to stop when switching profiles ([#65](https://github.com/GeekyEggo/SoundDeck/issues/65)).

### ♻ Changed

- Switched to GNU General Public License v3 (GPL-3) license.

## v3.0.2

### 🐞 Fixed

- Fixed an error that prevented users from opening the folder picker.

## v3.0.1

### 🐞 Fixed

- Fixed an error that could be encountered during initial setup of "Clip Audio" actions ([#55](https://github.com/GeekyEggo/SoundDeck/issues/55)).

## v3.0.0

### ⭐ Added

- Actions now support "Default" device as an audio device option; currently supported by ([#28](https://github.com/GeekyEggo/SoundDeck/issues/28)).
  - "Play Audio".
  - "Sampler".
  - "Set App Audio Device".
- Added long-press support for "Play Audio" actions; now resets the current audio to the first item in the playlist ([#30](https://github.com/GeekyEggo/SoundDeck/issues/30)).
- Added support for more audio formats ([#33](https://github.com/GeekyEggo/SoundDeck/issues/33)).
  - AIFF.
  - OGG.

### 🐞 Fixed

- Fixed an issue that caused "Set App Audio Device" to fail on Windows 11 ([#52](https://github.com/GeekyEggo/SoundDeck/issues/52)).

### ♻ Changed

- Updated NAudio dependency ([#27](https://github.com/GeekyEggo/SoundDeck/issues/27)).
- Logging moved to `%APPDATA%\Elgato\StreamDeck\Plugins\com.geekyeggo.sounddeck.sdPlugin\logs` ([#32](https://github.com/GeekyEggo/SoundDeck/issues/32)).
- Reduced memory usage of "Clip Audio" by approx ~80% ([#49](https://github.com/GeekyEggo/SoundDeck/issues/49)).
- Updated to .NET Framework 4.8.

## 2.0.0

### ⭐ Added

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

### 🐞 Fixed

- Greatly reduced CPU usage when playing audio files.
- Fixed audio time display.
- Fixed an issue when moving actions around could cause incorrect playback.

### ♻ Changed

- Removed drag-handle within Play Audio settings when order was sequential.

## 1.0.0

### ⭐ Added

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
