# Set App Audio Device CLI

The [SetAppAudioDevice.exe](https://github.com/GeekyEggo/SoundDeck/raw/main/tools/SetAppAudioDevice/dist/SetAppAudioDevice.exe) is a stand-alone command-line executable for the "Set App Audio Device" Stream Deck action.

## Usage
```
SetAppAudioDevice.exe
    [-p <process>]
    [-d <device>]
```

* `-p` - Optional; the **process** name, e.g. "spotify"; when not specified, the foreground application is used.
* `-d` - Optional; the **device** name, e.g. "music"; when not specified, the default playback device is used.
