import React from "react";
import { PropertyInspectorWrapper, Range, Select, store, TextField } from "react-sharpdeck";
import ProcessSelector from "./processSelector";
import VolumeAdjustment from "./volumeAdjustment";

class SetAudioDeviceVolumeSettings extends React.Component {
    render() {
        return (
            <PropertyInspectorWrapper>
                <Select label="Audio Device" dataSourceUri="GetAudioDevices" valuePath="audioDeviceId" defaultValue="PLAYBACK_DEFAULT" />
                <VolumeAdjustment />
            </PropertyInspectorWrapper>
        );
    }
}

export default SetAudioDeviceVolumeSettings;
