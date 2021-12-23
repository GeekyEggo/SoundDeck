import React from "react";
import { PropertyInspectorWrapper, Range, Select, store, TextField } from "react-sharpdeck";
import ProcessSelector from "./ProcessSelector";

class SetAppVolumeSettings extends React.Component {
    constructor(props) {
        super(props);

        this.volumeActions = [
            { label: "Mute / Unmute", value: "0" },
            { label: "Mute", value: "1" },
            { label: "Unmute", value: "2" },
            { label: "Set", value: "3" },
            { label: "Increase By", value: "4" },
            { label: "Decrease By", value: "5" },
        ]

        this.state = { isVolumeValueVisible: this.isVolumeValueVisible() };
        store.subscribe(() => this.setState({ isVolumeValueVisible: this.isVolumeValueVisible() }));
    }

    isVolumeValueVisible() {
        const { settings: { action } } = store.getState();
        return action == 3 || action == 4 || action == 5;
    }

    render() {
        return (
            <PropertyInspectorWrapper>
                <Select label="Action" options={this.volumeActions} valuePath="action" defaultValue="0" />
                {this.state.isVolumeValueVisible &&
                    <Range label="Value" valuePath="actionValue" defaultValue="100" min="0" max="100" step="5" />
                }
                <ProcessSelector />
            </PropertyInspectorWrapper>
        );
    }
}

export default SetAppVolumeSettings;
