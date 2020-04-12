[![Twitter icon](https://img.shields.io/badge/GeekyEggo--brightgreen?style=social&logo=twitter)](https://www.twitter.com/geekyeggo) [![version-icon]](https://github.com/GeekyEggo/SoundDeck) 

# <img src="./assets/sound-deck.svg" width="25" alt="Sound Deck Logo" /> Sound Deck

Sound Deck is a plugin for the Elgato Stream Deck designed to provide advanced audio clip control and capture on **any audio device**. Each of Sound Deck's actions allow you to choose which audio device they're associated with, meaning you can record one device, and play clips on another!

[<img src="./assets/elgato.svg" width="18" alt="Stream Deck Logo" /> Stream Deck](https://www.elgato.com/en/gaming/stream-deck) + [<img src="./assets/go-xlr.svg" width="18" alt="GoXLR Logo" /> GoXLR](https://www.tc-helicon.com/broadcast) + <img src="./assets/sound-deck.svg" width="18" alt="Sound Deck Logo" /> Sound Deck = :heart:

## Contents

1. Actions
   1. [Clip Audio](#-clip-audio)
   1. [Play Audio](#-play-audio)
   1. [Record Audio](#-record-audio)
   1. [Stop Audio](#-stop-audio)
1. [F.A.Q.](#frequently-asked-questions)
1. [Licence](#Licence)

## <img src="./assets/clip.svg" width="20" alt="Clip Audio Action" /> Clip Audio

Think Twitch clips, but audio only, simple! The Clip Audio action lets you record the last _x_ seconds of audio for any of your computer's audio devices. After activating the action, the audio is clipped, and saved to the <code>Output Path</code> as an MP3/WAV file. Tip: this works great for recording silly moments in party chat!

| Option | Description |
--- | ---
<code>Audio Device</code> | The audio device to create clips from; this can be either an input (e.g. microphone) or output (e.g. speaker, headphone, etc.) device connected to your computer.
`Duration` | The length of the audio clip; can be 15 / 30 / 60 / 90 / 120 seconds.
<code>Output Path</code> | A directory path where audio clips should be saved to.
<code>Encode to MP3</code> | When checked, audio clips are encoded to MP3; otherwise audio clips are saved as WAV.
<code>Normalize Volume</code> | When checked, volume levels within the audio clip are normalized. This can reduce excessive peeking, but might reduce overall volume in some scenarios.

## <img src="./assets/play.svg" width="18" alt="Play Audio Action" /> Play Audio

Like the default _play audio_ action, but with more options and hype! The Play Audio action lets you play a list of audio files in the order you want, on the audio device you want. The action also lets you control how clips are played, via the `Action` option.

| Option | Description |
--- | ---
<code>Audio Device</code> | The audio device to play clips on; this can be any output (e.g. speaker, headphone, etc.) device connected to your computer.
`Action` | Tells the plugin what to do when the button is pressed.<br />`Play / Next` plays the next track, stopping the current track if one is playing.<br />`Play / Stop` plays the next track, or stops the current track if one is playing.<br />`Play All / Stop` plays the playlist from the next track, or stops playback if a track is playing. *(v1.1.0)*<br />`Loop / Stop` loops  the next track, or stops the current track if one is playing.<br />`Loop All / Stop` loops the playlist from the next track, or stops playback if a track is playing. *(v1.1.0)*<br />`Loop All / Stop (Reset)` loops the playlist from the first track, or stops playback if a track is playing. *(v1.1.0)*
`Order` | Defines the order the audio files are played in; can be either `Random` or `Sequential`. Please note, when `Sequential` is selected, files can be re-ordered manually.
`Files` | The playlist of audio files to play. Files can be added by selecting <code>Add file...</code>, removed by pressing the X, or re-ordered (when `Order` is `Sequential`) by dragging the bullet point up/down.

## <img src="./assets/record.svg" width="20" alt="Record Audio Action" /> Record Audio

The Record Audio action lets you record an audio device for any duration of time. Press once to start recording, press again to stop recording, easy! After recording, audio clips are saved to the <code>Output Path</code> as an MP3/WAV file.

| Option | Description |
--- | ---
<code>Audio Device</code> | The audio device to listen to; this can be either an input (e.g. microphone) or output (e.g. speaker, headphone, etc.) device connected to your computer.
<code>Output Path</code> | A directory path where audio clips should be saved to.
<code>Encode to MP3</code> | When checked, audio clips are encoded to MP3; otherwise audio clips are saved as WAV.
<code>Normalize Volume</code> | When checked, volume levels within the audio clip are normalized. This can reduce excessive peeking, but might reduce overall volume in some scenarios.

## <img src="./assets/stop.svg" width="20" alt="Record Audio Action" /> Stop Audio

The Stop Audio action lets you quickly and easily stop all audio currently being played through Sound Deck. This can be particularly useful if a Play Audio action is being played from a Multi-Action. *(v1.1.0)*

*There are no options associated with the Stop Audio action.*

## Frequently Asked Questions

Please see [Frequently Asked Questions](FAQ.md) for issues or requests; for general queries, please [contact me](https://twitter.com/geekyeggo) on <img src="./assets/twitter.svg" width="18" alt="Twitter" /> Twitter.

## Licence

Sound Deck is licenced under [The MIT License (MIT)](LICENSE.md) and is not officially associated with Elgato or TC-Helicon Gaming.

Stream Deck and GoXLR are trademarks or registered trademarks of [Elgato](https://www.elgato.com/en) and [TC-Helicon Gaming](https://www.tc-helicon.com) respectively.

[version-icon]: https://img.shields.io/badge/Sound%20Deck-1.0.0-informational?style=flat&logo=data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAyODcuMzUgMjcwLjc4Ij4KICAgIDxkZWZzPgogICAgICAgIDxzdHlsZT4KICAgICAgICAgICAgLnNwZWFrZXIgeyBmaWxsOiAjZmZmZmZmOyB9CiAgICAgICAgICAgIC5zdGFyIHsgZmlsbDogI2ZmZmZmZjsgfQogICAgICAgIDwvc3R5bGU+CiAgICA8L2RlZnM+CiAgICA8dGl0bGU+U291bmQgRGVjayBMb2dvPC90aXRsZT4KICAgIDxnPgogICAgICAgIDxwYXRoIGNsYXNzPSJzdGFyIiBkPSJNMjI0LjMzLDE0MS41NmwxMS4zMywzNC44NmE3LjY5LDcuNjksMCwwLDAsNy4zMiw1LjMyaDM2LjY1YTcuNyw3LjcsMCwwLDEsNC41MywxMy45MkwyNTQuNSwyMTcuMjFhNy43LDcuNywwLDAsMC0yLjc5LDguNkwyNjMsMjYwLjY3YTcuNyw3LjcsMCwwLDEtMTEuODQsOC42MWwtMjkuNjUtMjEuNTVhNy43LDcuNywwLDAsMC05LDBsLTI5LjY2LDIxLjU1QTcuNyw3LjcsMCwwLDEsMTcxLDI2MC42N2wxMS4zMy0zNC44NmE3LjY5LDcuNjksMCwwLDAtMi44LTguNmwtMjkuNjUtMjEuNTVhNy42OSw3LjY5LDAsMCwxLDQuNTItMTMuOTJIMTkxYTcuNjksNy42OSwwLDAsMCw3LjMyLTUuMzJsMTEuMzMtMzQuODZBNy43LDcuNywwLDAsMSwyMjQuMzMsMTQxLjU2WiIvPgogICAgICAgIDxwYXRoIGNsYXNzPSJzcGVha2VyIiBkPSJNMjAyLjQ5LDMwLjQ0bDYuODQtMTEuMzFBMy4zNywzLjM3LDAsMCwxLDIxNCwxOGMxNC4wNiw4LjY2LDcyLjgyLDUwLjYxLDQ4Ljk0LDEzNC40MS00LjQzLDEwLjItOS43Niw4LjQyLTExLjA5LDQuODhzLTQuMTYtMTEuMTMtNC44Ny0xMy43NWMtLjU0LTItMS4zMy00LC44OC0xNi44NSwyLjE4LTEyLjY0LDQuNzktNTYuMTctNDQuNDUtOTEuNzVBMy4zOSwzLjM5LDAsMCwxLDIwMi40OSwzMC40NFoiLz4KICAgICAgICA8cGF0aCBjbGFzcz0ic3BlYWtlciIgZD0iTTIwNi43MywxMTNzMS43Mi0yMy4zNi0yNi4xOS00MS45MWEzLjEsMy4xLDAsMCwxLS45LTQuMjZjMy41NC01LjQ5LDYuNTQtMTAsOC4zLTEyLjU3YTMuMDcsMy4wNywwLDAsMSw0LjIzLS44NmM4LjU5LDUuNjEsMzUuNjgsMjYsMzQuODQsNTkuNiIvPgogICAgICAgIDxwYXRoIGNsYXNzPSJzcGVha2VyIiBkPSJNMTY0LDExNi42Yy0uMjgsMS40Mi0xLjUzLDUuMi03LjA2LDkuMTJhMy4wOSwzLjA5LDAsMCwwLS44NCw0LjJjMy43Niw1Ljg2LDYuMzgsOS43OSw3Ljg2LDEyYTMuMDksMy4wOSwwLDAsMCwzLjkxLDFjNS4xOS0yLjYsMTYuODMtMTAuMjYsMTYuODMtMjYuOTJTMTczLjA5LDkxLjY5LDE2Ny45LDg5LjFhMy4wOSwzLjA5LDAsMCwwLTMuOTEsMWMtMS40OCwyLjE4LTQuMSw2LjExLTcuODYsMTJhMy4wOSwzLjA5LDAsMCwwLC44NCw0LjJjNS41MywzLjkyLDYuNzgsNy43LDcuMDYsOS4xMkEyLjg4LDIuODgsMCwwLDEsMTY0LDExNi42WiIvPgogICAgICAgIDxwYXRoIGNsYXNzPSJzcGVha2VyIiBkPSJNNDcsMTE3Ljc0djM3YTkuNjMsOS42MywwLDAsMCwyLjU4LDYuNTZsNjYuMzMsNzEuMTNhOS42Miw5LjYyLDAsMCwwLDE2LjY2LTYuNTZWOS42NWE5LjYyLDkuNjIsMCwwLDAtMTYuNjYtNi41N0w0OS41OCw3NC4yMUE5LjY1LDkuNjUsMCwwLDAsNDcsODAuNzhaIi8+CiAgICAgICAgPHBhdGggY2xhc3M9InNwZWFrZXIiIGQ9Ik0yOC44MiwxMTcuNzRWNzcuODdIMTguMzVBMTguMzYsMTguMzYsMCwwLDAsMCw5Ni4yM3Y0M2ExOC4zNiwxOC4zNiwwLDAsMCwxOC4zNSwxOC4zNUgyOC44MloiLz4KICAgIDwvZz4KPC9zdmc+Cg==
