import React from "react";
import AudioDevices from "./audioDeviceSelect";
import client from "../common/streamDeckClient";
import Duration from "./durationSelect";
import FolderPicker from "../common/components/folderPicker"
import PropertyInspectorWrapper from "../common/components/propertyInspectorWrapper";

class CaptureAudioBufferSettings extends React.Component {
    render() {
        return (
            <PropertyInspectorWrapper>
                <AudioDevices valuePath="audioDeviceId" />
                <Duration valuePath="clipDuration" />
                <FolderPicker label="Output Path" pluginUri="GetOutputPath" valuePath="outputPath" />
            </PropertyInspectorWrapper>
        );
    }
}

export default CaptureAudioBufferSettings
