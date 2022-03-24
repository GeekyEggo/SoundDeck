import React from "react";
import { PropertyInspectorWrapper, Select, } from "react-sharpdeck";
import ProcessSelector from "./processSelector";

class AppMultimediaControlsSettings extends React.Component {
    constructor(props) {
        super(props);

        this.multimediaActions = [
            { label: "Play / Pause", value: "0" },
            { label: "Play", value: "1" },
            { label: "Pause", value: "2" },
            { label: "Stop", value: "3" },
            { label: "Skip Previous", value: "4" },
            { label: "Skip Next", value: "5" }
        ]
    }

    render() {
        return (
            <PropertyInspectorWrapper>
                <ProcessSelector />
                <Select label="Action" options={this.multimediaActions} valuePath="action" defaultValue="0" />
            </PropertyInspectorWrapper>
        );
    }
}

export default AppMultimediaControlsSettings;
