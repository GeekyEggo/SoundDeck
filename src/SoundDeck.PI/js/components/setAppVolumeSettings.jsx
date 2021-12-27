import React from "react";
import { PropertyInspectorWrapper, Range, Select, store, TextField } from "react-sharpdeck";
import ProcessSelector from "./processSelector";
import VolumeAdjustment from "./volumeAdjustment";

class SetAppVolumeSettings extends React.Component {
    render() {
        return (
            <PropertyInspectorWrapper>
                <ProcessSelector />
                <VolumeAdjustment />
            </PropertyInspectorWrapper>
        );
    }
}

export default SetAppVolumeSettings;
