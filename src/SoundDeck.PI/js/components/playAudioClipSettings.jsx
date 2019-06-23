import React from "react";
import { Checkbox, FolderPicker, PropertyInspectorWrapper, Select } from "react-sharpdeck";

class PlayAudioClipSettings extends React.Component {
    render() {
        return (
            <PropertyInspectorWrapper>
                <Select label="Audio Device" dataSourceUri="GetAudioDevices" valuePath="audioDeviceId" />
            </PropertyInspectorWrapper>
        );
    }
}

export default PlayAudioClipSettings
