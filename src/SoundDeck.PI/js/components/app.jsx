import React from "react";
import CaptureAudioBufferSettings from "./captureAudioBufferSettings";
import PlayAudioClipSettings from "./playAudioClipSettings";

class App extends React.Component {
    render() {
        switch (this.props.uuid) {
            case "com.geekyEggo.soundDeck.captureAudioBuffer":
                return <CaptureAudioBufferSettings />;

            case "com.geekyEggo.soundDeck.playAudioClip":
                return <PlayAudioClipSettings />;

            default:
                return <div />
        }
    }
}

export default App;
