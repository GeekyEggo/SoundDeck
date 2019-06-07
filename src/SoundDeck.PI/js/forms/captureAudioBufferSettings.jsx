import React from "react";
import AudioDevices from "../components/audioDeviceSelect";
import client from "../common/streamDeckClient";
import Duration from "../components/durationSelect";
import FolderPicker from "../common/components/folderPicker"

class CaptureAudioBufferSettings extends React.Component {
    render() {
        return (
            <div className="sdpi-wrapper">
                <AudioDevices valuePath="audioDeviceId" />
                <Duration valuePath="clipDuration" />
                <FolderPicker label="Output Path" pluginUri="GetOutputPath" valuePath="outputPath" />
            </div>
        );
    }
}

export default CaptureAudioBufferSettings
