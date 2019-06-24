import React from "react";
import { PropertyInspectorWrapper, Select } from "react-sharpdeck";
import FilesPicker from "./filesPicker";

class PlayAudioClipSettings extends React.Component {
    render() {
        return (
            <PropertyInspectorWrapper>
                <Select label="Audio Device" dataSourceUri="GetAudioDevices" valuePath="audioDeviceId" />
                <FilesPicker label="Files" valuePath="files" accept="audio/mpeg,audio/wav" buttonLabel="Add file..." />
            </PropertyInspectorWrapper>
        );
    }
}

export default PlayAudioClipSettings
