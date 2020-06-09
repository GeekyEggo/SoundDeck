import React from "react";
import { Checkbox, FolderPicker, PropertyInspectorWrapper, Select } from "react-sharpdeck";

class CaptureAudioSettings extends React.Component {
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
                <Select label="Audio Device" dataSourceUri="GetCaptureAudioDevices" valuePath="audioDeviceId" />
                {this.props.showDuration &&
                    <Select label="Duration" options={this.durationOptions} valuePath="duration" />
                }
                <FolderPicker label="Output Path" pluginUri="GetOutputPath" valuePath="outputPath" />
                <Checkbox label="Encode to MP3" valuePath="encodeToMP3" defaultValue={false} id="encodeToMP3" />
                <Checkbox label="Normalize Volume" valuePath="normalizeVolume" defaultValue={true} id="normalizeVolume" />
            </PropertyInspectorWrapper>
        );
    }
}

CaptureAudioSettings.defaultProps = {
    showDuration: false
};

export default CaptureAudioSettings
