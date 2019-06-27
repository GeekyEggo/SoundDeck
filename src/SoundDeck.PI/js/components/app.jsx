import React from "react";
import ClipAudioSettings from "./clipAudioSettings";
import PlayAudioSettings from "./playAudioSettings";

class App extends React.Component {
    render() {
        switch (this.props.uuid) {
            case "com.geekyEggo.soundDeck.clipAudio":
                return <ClipAudioSettings />;

            case "com.geekyEggo.soundDeck.playAudio":
                return <PlayAudioSettings />;

            default:
                return <div />
        }
    }
}

export default App;
