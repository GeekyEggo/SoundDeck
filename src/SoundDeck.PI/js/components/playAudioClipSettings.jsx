import React from "react";
import { PropertyInspectorWrapper, Select, store } from "react-sharpdeck";
import FilesPicker from "./filesPicker";

class PlayAudioClipSettings extends React.Component {
    constructor(props) {
        super(props);

        this.actionTypes = [
            { label: "Play / Next", value: "0" },
            { label: "Play / Stop", value: "1" },
        ]

        this.orderTypes = [
            { label: "Sequential", value: "0" },
            { label: "Random", value: "1" },
        ]

        this.state = { enableSort: this.isSortEnabled() };
        store.subscribe(() => this.setState({ enableSort: this.isSortEnabled() }));

    }

    isSortEnabled() {
        const { settings: { order } } = store.getState();
        return order == 0;
    }

    render() {
        return (
            <PropertyInspectorWrapper>
                <Select label="Audio Device" dataSourceUri="GetAudioDevices" valuePath="audioDeviceId" />
                <Select label="Action" options={this.actionTypes} valuePath="action" onChange={this.onActionTypeChange} />
                <Select label="Order" options={this.orderTypes} valuePath="order" />
                <FilesPicker label="Files" valuePath="files" accept="audio/mpeg,audio/wav" buttonLabel="Add file..." enableSort={this.state.enableSort} />
            </PropertyInspectorWrapper>
        );
    }
}

export default PlayAudioClipSettings;
