[![Twitter icon](https://img.shields.io/badge/GeekyEggo--brightgreen?style=social&logo=twitter)](https://www.twitter.com/geekyeggo) [![version-icon]](https://github.com/GeekyEggo/SoundDeck) 

# <img src="./assets/sound-deck.svg" width="25" alt="Sound Deck Logo" /> Sound Deck

Sound Deck is a plugin for the Elgato Stream Deck designed to provide advanced audio clip control and capture on **any audio device**. Each of Sound Deck's actions allow you to choose which audio device they're associated with, meaning you can record one device, and play clips on another!

[![goxlr](assets/elgato.png) Stream Deck](https://www.elgato.com/en/gaming/stream-deck) + [![goxlr](assets/goxlr.png) GoXLR](https://www.tc-helicon.com/broadcast) + <img src="./assets/sound-deck.svg" width="16" alt="Sound Deck Logo" /> Sound Deck = :heart:

## Contents

1. Actions
   1. [Clip Audio](#-clip-audio)
   1. [Play Audio](#-play-audio)
   1. [Record Audio](#-record-audio)
1. [F.A.Q.](#frequently-asked-questions)
1. [Licence](#Licence)

## ![Clip Audio action icon](assets/clip-audio.png) Clip Audio

Think Twitch clips, but audio only, simple! The Clip Audio action lets you record the last _x_ seconds of audio for any of your computer's audio devices. After activating the action, the audio is clipped, and saved to the <code>Output&nbsp;Path</code> as an MP3/WAV file. Tip: this works great for recording silly moments in party chat!

| Option | Description |
--- | ---
<code>Audio&nbsp;Device</code> | The audio device to create clips from; this can be either an input (e.g. microphone) or output (e.g. speaker, headphone, etc.) device connected to your computer.
`Duration` | The length of the audio clip; can be 15 / 30 / 60 / 90 / 120 seconds.
<code>Output&nbsp;Path</code> | A directory path where audio clips should be saved to.
<code>Encode&nbsp;to&nbsp;MP3</code> | When checked, audio clips are encoded to MP3; otherwise audio clips are saved as WAV.
<code>Normalize&nbsp;Volume</code> | When checked, volume levels within the audio clip are normalized. This can reduce excessive peeking, but might reduce overall volume in some scenarios.

## ![Play Audio action icon](assets/play-audio.png) Play Audio

Like the default _play audio_ action, but with more options and hype! The Play Audio action lets you play a list of audio files in the order you want, on the audio device you want. The action also lets you control how clips are played, via the `Action` option.

| Option | Description |
--- | ---
<code>Audio&nbsp;Device</code> | The audio device to play clips on; this can be any output (e.g. speaker, headphone, etc.) device connected to your computer.
`Action` | Tells the plugin what to do when the button is pressed.<br />`Play / Next` - Plays the next clip; stopping the current clip if there is one.<br />`Play / Stop` - Plays the next clip when one isn't already playing; otherwise the current clip is stopped.<br />`Loop / Stop` - Loops the next clip when one isn't already playing; otherwise the current clip is stopped
`Order` | Defines the order the audio files are played in; can be either `Random` or `Sequential`. Please note, when `Sequential` is selected, files can be re-ordered manually.
`Files` | The playlist of audio files to play. Files can be added by selecting <code>Add&nbsp;file...</code>, removed by pressing the X, or re-ordered (when `Order` is `Sequential`) by dragging the bullet point up/down.

## ![Record Audio action icon](assets/record-audio.png) Record Audio

The Record Audio action lets you record an audio device for any duration of time. Press once to start recording, press again to stop recording, easy! After recording, audio clips are saved to the <code>Output&nbsp;Path</code> as an MP3/WAV file.

| Option | Description |
--- | ---
<code>Audio&nbsp;Device</code> | The audio device to listen to; this can be either an input (e.g. microphone) or output (e.g. speaker, headphone, etc.) device connected to your computer.
<code>Output&nbsp;Path</code> | A directory path where audio clips should be saved to.
<code>Encode&nbsp;to&nbsp;MP3</code> | When checked, audio clips are encoded to MP3; otherwise audio clips are saved as WAV.
<code>Normalize&nbsp;Volume</code> | When checked, volume levels within the audio clip are normalized. This can reduce excessive peeking, but might reduce overall volume in some scenarios.

## Frequently Asked Questions

Please see [Frequently Asked Questions](FAQ.md) for issues or requests; for general queries, please [contact me](https://twitter.com/geekyeggo) on ![Twitter](https://i.imgur.com/wWzX9uB.png) Twitter.

## Licence

Sound Deck is licenced under [The MIT License (MIT)](LICENSE.md) and is not officially associated with Elgato or TC-Helicon Gaming.

Stream Deck and GoXLR are trademarks or registered trademarks of [Elgato](https://www.elgato.com/en) and [TC-Helicon Gaming](https://www.tc-helicon.com) respectively.

[version-icon]: https://img.shields.io/badge/Sound%20Deck-1.0.0-informational?style=flat&logo=data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAYAAAAfSC3RAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAFiSURBVDhPldJNKARxGMfxmbF2V0jhQC5yIBfyUjM5SSk37koOUzhQuDg5KCcOHBxccJOD0iblIgcyW44KB0QoSVx2tetlfH+anfawDp76NM9M8/z/z//FNPLC8zy912ECKcw7jvOZTCbjvu9X8P6GDN+MsDAoqsEYprCKGXyhE5PYQYJB3i0KIijhQyWGoNlKkQt1EMcBhtFimqZlkXSjFx2YhlrKhQZswAiyOEc/YiqcwyDUZjXyoxlVOA3yE7QjrkK1+FfUowea6Rg3KEdUhQ/wUSjucIhbaJ36/xlZJYvYxGMgDDbhDHuktRiF2l5Hqsh13WuSK+iMPtAK7eol1EkfBtCFMkTRFJ4jh2xyPtog7eA4tnAEzdQGDZaG1rmgVn/Dtm2N/oQ1LOEC+9jGKzRJDCtIhIUKrtI3j3ssY4MOVFAMFWlnM4ggHbZaKLhRWpOu2gt2ofU2Ylbr4vnfMIwfeq5ld+cnpg0AAAAASUVORK5CYII=
