import React from "react";
import CaptureAudioSettings from "./captureAudioSettings";
import PlayAudioSettings from "./playAudioSettings";
import SamplerSettings from "./samplerSettings";
import SetDefaultAudioDeviceSettings from "./setDefaultAudioDeviceSettings";

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

            case "com.geekyeggo.sounddeck.setdefaultaudiodevice":
                return <SetDefaultAudioDeviceSettings />;

            default:
                return <div />;
        }
    }
}

export default App;
