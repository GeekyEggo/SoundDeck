import React from "react";
import { PropertyInspectorWrapper, Select, store, TextField } from "react-sharpdeck";

class SetAppAudioDeviceSettings extends React.Component {
    constructor(props) {
        super(props);

        this.processSelectionOptions = [
            { label: "Foreground (Active)", value: "0" },
            { label: "By Name", value: "1" }
        ]

        this.state = { isProcessNameVisible: this.isProcessNameVisible() };
        store.subscribe(() => this.setState({ isProcessNameVisible: this.isProcessNameVisible() }));
    }

    isProcessNameVisible() {
        const { settings: { processSelectionType } } = store.getState();
        return processSelectionType == 1;
    }

    render() {
        return (
            <PropertyInspectorWrapper>
                <Select label="Audio Device" dataSourceUri="GetPlaybackAudioDevices" valuePath="audioDeviceId" />
                <Select label="Process" options={this.processSelectionOptions} valuePath="processSelectionType" />
                {this.state.isProcessNameVisible &&
                    <TextField label="Name" valuePath="processName" />
                }
            </PropertyInspectorWrapper>
        );
    }
}

export default SetAppAudioDeviceSettings
