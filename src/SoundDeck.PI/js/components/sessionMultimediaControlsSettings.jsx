import React from "react";
import { PropertyInspectorWrapper, Select, store, TextField } from "react-sharpdeck";

class SessionMultimediaControlsSettings extends React.Component {
    constructor(props) {
        super(props);

        this.multimediaAction = [
            { label: "Previous", value: "1" },
            { label: "Play / Pause", value: "3" },
            { label: "Next", value: "0" },
            { label: "Stop", value: "2" }
        ]
    }

    render() {
        return (
            <PropertyInspectorWrapper>
                <TextField label="Name" valuePath="searchCriteria" />
                <Select label="Action" options={this.multimediaAction} valuePath="action" />
            </PropertyInspectorWrapper>
        );
    }
}

export default SessionMultimediaControlsSettings;
