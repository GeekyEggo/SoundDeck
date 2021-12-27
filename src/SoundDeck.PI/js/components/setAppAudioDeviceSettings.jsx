import React from "react";
import { PropertyInspectorWrapper, Select } from "react-sharpdeck";
import ProcessSelector from "./ProcessSelector";

class SetAppAudioDeviceSettings extends React.Component {
    render() {
        return (
            <PropertyInspectorWrapper>
                <ProcessSelector />
                <Select label="Audio Device" dataSourceUri="GetAppAssignableAudioDevices" valuePath="audioDeviceId" defaultValue="PLAYBACK_DEFAULT" />
            </PropertyInspectorWrapper>
        );
    }
}

export default SetAppAudioDeviceSettings;
