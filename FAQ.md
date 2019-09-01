# Frequently Asked Questions
If you have a general question about Sound Deck, please contact me on [![Twitter](https://i.imgur.com/wWzX9uB.png)Twitter](https://twitter.com/geekyeggo).


## Contents
* [What operating systems are supported?](#what-operating-systems-are-supported)
* [I see an exclamation mark when I press an action](#i-see-an-exclamation-mark-when-i-press-an-action)
* [Clicking the directory button doesn't do anything](#clicking-the-directory-button-doesnt-do-anything)
* [Audio clips are really quiet](#audio-clips-are-really-quiet)
* [How do I submit an issue?](#how-do-i-submit-an-issue)

## What operating systems are supported?
Currently Sound Deck supports Windows 10, and requires [Microsoft .NET Framework](https://dotnet.microsoft.com/download/dotnet-framework) runtime.

## I see an exclamation mark when I press an action
This indicates an error has occurred with the plugin; restarting the Stream Deck application usually resolve most errors. If the problem persists, please consider submitting an [issue].

## Clicking the directory button doesn't do anything
The button _should_ open a dialog that lets you choose a folder; if it appears nothing has happened, the dialog might just be hiding behind the Stream Deck window.

## Audio clips are really quiet
The volume of audio clips are currently normalized within Sound Deck; I intend to make this configurable within future releases; if you'd like to see this sooner, please let me know.

## How do I submit an issue?
You can submit an issue on ![issue-icon] GitHub, [here](https://github.com/geekyeggo/sounddeck/issues). Please include any log files, as they are incredibly helpful when figuring out what went wrong. Logs can be found in `%APPDATA%\Elgato\StreamDeck\logs`, and will be named `com.geekyeggo.sounddeck#.log`, where `#` is a number (the latest log file is enough).

[issue-icon]: https://i.imgur.com/9I6NRUm.png
