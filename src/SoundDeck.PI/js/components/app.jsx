import React from "react";
import CaptureAudioSettings from "./captureAudioSettings";
import PlayAudioSettings from "./playAudioSettings";
import SamplerSettings from "./samplerSettings";
import SessionMultimediaControlsSettings from "./sessionMultimediaControlsSettings";
import SetAppAudioDeviceSettings from "./setAppAudioDeviceSettings"

class App extends React.Component {
    render() {
        switch (this.props.uuid) {
            case "com.geekyeggo.sounddeck.clipaudio":
                return <CaptureAudioSettings showDuration={true} />;

            case "com.geekyeggo.sounddeck.playaudio":
                return <PlayAudioSettings />;

            case "com.geekyeggo.sounddeck.recordaudio":
                return <CaptureAudioSettings />;

            case "com.geekyeggo.sounddeck.sampler":
                return <SamplerSettings />;

            case "com.geekyeggo.sounddeck.sessionmultimediacontrols":
                return <SessionMultimediaControlsSettings />;

            case "com.geekyeggo.sounddeck.setappaudiodevice":
                return <SetAppAudioDeviceSettings />

            default:
                return <div />
        }
    }
}

export default App;
