[![version-icon]](https://github.com/GeekyEggo/SoundDeck) [![twitter-icon]](https://www.twitter.com/geekyeggo)

# Sound Deck

Sound Deck is a plugin for the Elgato Stream Deck designed to provide advanced audio clip control and capture on **any audio device**. Each of Sound Deck's actions allow you to choose which audio device they're associated with, meaning you can record one device, and play clips on another!

[Stream Deck](https://www.elgato.com/en/gaming/stream-deck) + Sound Deck + [GoXLR](https://www.tc-helicon.com/broadcast) = :heart:

## Contents
1. [Actions](#actions)
   1. [Clip Audio](#-clip-audio)
   1. [Play Audio](#-play-audio)
   1. [Record Audio](#-record-audio)
1. [F.A.Q.](#frequently-asked-questions)
1. [Licence](#Licence)

## Actions
### ![Clip Audio action icon](src/SoundDeck.Plugin/Images/ClipAudio/Action-Black.png) Clip Audio
### ![Play Audio action icon](src/SoundDeck.Plugin/Images/PlayAudio/Action-Black.png) Play Audio
### ![Record Audio action icon](src/SoundDeck.Plugin/Images/RecordAudio/Action-Black.png) Record Audio

## Frequently Asked Questions
* [What operating systems are supported?](#)
* [I see an exclamation mark when I press an action](#)
* [Clicking the directory button doesn't do anything](#)
* [Audio clips are really quiet](#)

#### _What operating systems are supported?_
Currently Sound Deck supports Windows 10, and requires [Microsoft .NET Framework](https://dotnet.microsoft.com/download/dotnet-framework) runtime.

#### _I see an exclamation mark when I press an action_
This indicates an error has occurred with the plugin; restarting the Stream Deck application usually resolve most errors. If the problem persists, please consider submitting an [issue](https://github.com/geekyeggo/sounddeck/issues), including any log files is really helpful. Logs can be found in `%APPDATA%\Elgato\StreamDeck\logs`, and will be named `com.geekyeggo.sounddeck#.log`.

#### _Clicking the directory button doesn't do anything_
The button _should_ open a dialog that lets you choose a folder; if it appears nothing has happened, the dialog might just be hiding behind the Stream Deck window.

#### _Audio clips are really quiet_
The volume of audio clips are currently normalized within Sound Deck; I intend to make this configurable within future releases; if you'd like to see this sooner, please let me know.

## Licence
Sound Deck is licenced under [The MIT License (MIT)](LICENSE.md) and is not officially associated with Elgato or TC-Helicon Gaming.

Stream Deck and GoXLR are trademarks or registered trademarks of [Elgato](https://www.elgato.com/en) and [TC-Helicon Gaming](https://www.tc-helicon.com) respectively.

[twitter-icon]: https://img.shields.io/badge/GeekyEggo--brightgreen?style=social&logo=twitter
[version-icon]: https://img.shields.io/badge/Sound%20Deck-1.0.0-informational?style=flat&logo=data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAYAAAAfSC3RAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAFiSURBVDhPldJNKARxGMfxmbF2V0jhQC5yIBfyUjM5SSk37koOUzhQuDg5KCcOHBxccJOD0iblIgcyW44KB0QoSVx2tetlfH+anfawDp76NM9M8/z/z//FNPLC8zy912ECKcw7jvOZTCbjvu9X8P6GDN+MsDAoqsEYprCKGXyhE5PYQYJB3i0KIijhQyWGoNlKkQt1EMcBhtFimqZlkXSjFx2YhlrKhQZswAiyOEc/YiqcwyDUZjXyoxlVOA3yE7QjrkK1+FfUowea6Rg3KEdUhQ/wUSjucIhbaJ36/xlZJYvYxGMgDDbhDHuktRiF2l5Hqsh13WuSK+iMPtAK7eol1EkfBtCFMkTRFJ4jh2xyPtog7eA4tnAEzdQGDZaG1rmgVn/Dtm2N/oQ1LOEC+9jGKzRJDCtIhIUKrtI3j3ssY4MOVFAMFWlnM4ggHbZaKLhRWpOu2gt2ofU2Ylbr4vnfMIwfeq5ld+cnpg0AAAAASUVORK5CYII=