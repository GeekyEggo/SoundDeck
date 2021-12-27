import React from "react";
import { PropertyInspectorWrapper, Select } from "react-sharpdeck";

class SetDefaultAudioDeviceSettings extends React.Component {
    render() {
        const roles = [
            { label: "Default", value: "0" },
            { label: "Communication", value: "2" }
        ];

        return (
            <PropertyInspectorWrapper>
                <Select label="Audio Device" dataSourceUri="GetDefaultAssignableAudioDevices" valuePath="audioDeviceId" defaultValue="PLAYBACK_DEFAULT" />
                <Select label="Role" options={roles} valuePath="role" defaultValue="0" />
            </PropertyInspectorWrapper>
        );
    }
}

export default SetDefaultAudioDeviceSettings;
