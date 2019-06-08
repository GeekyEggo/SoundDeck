import React from "react";
import FolderPicker from "../common/components/folderPicker"
import PropertyInspectorWrapper from "../common/components/propertyInspectorWrapper";
import Select from "../common/components/select";

class CaptureAudioBufferSettings extends React.Component {
    constructor (props) {
        super(props);

        this.durationOptions = [
            { label: "15 seconds", value: "15" },
            { label: "30 seconds", value: "30" },
            { label: "60 seconds", value: "60" },
            { label: "90 seconds", value: "90" },
            { label: "120 seconds", value: "120" }
        ]
    }

    render() {
        return (
            <PropertyInspectorWrapper>
                <Select label="Audio Device" dataSourceUri="GetAudioDevices" valuePath="audioDeviceId" />
                <Select label="Duration" options={this.durationOptions} valuePath="clipDuration" />
                <FolderPicker label="Output Path" pluginUri="GetOutputPath" valuePath="outputPath" />
            </PropertyInspectorWrapper>
        );
    }
}

export default CaptureAudioBufferSettings
