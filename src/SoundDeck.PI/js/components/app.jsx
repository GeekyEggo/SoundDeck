import React from "react";
import AppMultimediaControlsSettings from "./appMultimediaControlsSettings";
import CaptureAudioSettings from "./captureAudioSettings";
import PlayAudioSettings from "./playAudioSettings";
import SamplerSettings from "./samplerSettings";
import SetAppAudioDeviceSettings from "./setAppAudioDeviceSettings";
import SetAppVolumeSettings from "./setAppVolumeSettings";
import SetAudioDeviceVolumeSettings from "./setAudioDeviceVolumeSettings";

class App extends React.Component {
    render() {
        switch (this.props.uuid) {
            case "com.geekyeggo.sounddeck.appmultimediacontrols":
                return <AppMultimediaControlsSettings />;

            case "com.geekyeggo.sounddeck.clipaudio":
                return <CaptureAudioSettings showDuration={true} />;

            case "com.geekyeggo.sounddeck.playaudio":
                return <PlayAudioSettings />;

            case "com.geekyeggo.sounddeck.recordaudio":
                return <CaptureAudioSettings />;

            case "com.geekyeggo.sounddeck.sampler":
                return <SamplerSettings />;

            case "com.geekyeggo.sounddeck.setappaudiodevice":
                return <SetAppAudioDeviceSettings />;

            case "com.geekyeggo.sounddeck.setappvolume":
                return <SetAppVolumeSettings />;

            case "com.geekyeggo.sounddeck.setaudiodevicevolume":
                return <SetAudioDeviceVolumeSettings />;

            default:
                return <div />;
        }
    }
}

export default App;
