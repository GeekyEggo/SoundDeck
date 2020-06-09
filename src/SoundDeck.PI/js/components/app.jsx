import React from "react";
import CaptureAudioSettings from "./captureAudioSettings";
import PlayAudioSettings from "./playAudioSettings";
import SamplerSettings from "./samplerSettings";

class App extends React.Component {
    render() {
        console.log(this.props.uuid);

        switch (this.props.uuid) {
            case "com.geekyeggo.sounddeck.clipaudio":
                return <CaptureAudioSettings showDuration={true} />;

            case "com.geekyeggo.sounddeck.playaudio":
                return <PlayAudioSettings />;

            case "com.geekyeggo.sounddeck.recordaudio":
                return <CaptureAudioSettings />;

            case "com.geekyeggo.sounddeck.sampler":
                return <SamplerSettings />;

            default:
                return <div />
        }
    }
}

export default App;
