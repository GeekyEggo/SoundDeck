import React from "react";
import CaptureAudioSettings from "./captureAudioSettings";
import PlayAudioSettings from "./playAudioSettings";

class App extends React.Component {
    render() {
        switch (this.props.uuid) {
            case "com.geekyEggo.soundDeck.clipAudio":
                return <CaptureAudioSettings showDuration={true} />;

            case "com.geekyEggo.soundDeck.playAudio":
                return <PlayAudioSettings />;

            case "com.geekyEggo.soundDeck.recordAudio":
                return <CaptureAudioSettings />;

            default:
                return <div />
        }
    }
}

export default App;
