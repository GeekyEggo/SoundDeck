import React from "react";
import { PropertyInspectorWrapper, Select, store, streamDeckClient } from "react-sharpdeck";
import FilesPicker from "./filesPicker";

class PlayAudioSettings extends React.Component {
    constructor(props) {
        super(props);

        this.actionTypes = [
            { label: "Play / Next", value: "0" },
            { label: "Play / Stop", value: "1" },
            { label: "Loop / Stop", value: "2" },
            { label: "Loop All / Stop", value: "3" },
            { label: "Loop All / Stop (Reset)", value: "4" }
        ]

        this.orderTypes = [
            { label: "Sequential", value: "0" },
            { label: "Random", value: "1" },
        ]

        this.state = { enableSort: this.isSortEnabled() };
        store.subscribe(() => this.setState({ enableSort: this.isSortEnabled() }));

        streamDeckClient.getGlobalSettings()
            .then((ev) => this.setState({ defaultPlaybackDeviceId: ev.payload.settings.defaultPlaybackDeviceId }));
    }

    isSortEnabled() {
        const { settings: { order } } = store.getState();
        return order !== 1; // default to true
    }

    render() {
        return (
            <PropertyInspectorWrapper>
                <Select label="Audio Device" dataSourceUri="GetAudioDevices" valuePath="audioDeviceId" defaultValue={this.state.defaultPlaybackDeviceId} />
                <Select label="Action" options={this.actionTypes} valuePath="action" defaultValue="0" onChange={this.onActionTypeChange} />
                <Select label="Order" options={this.orderTypes} valuePath="order" defaultValue="0" />
                <FilesPicker label="Files" valuePath="files" accept="audio/mpeg,audio/wav" buttonLabel="Add file..." enableSort={this.state.enableSort} />
            </PropertyInspectorWrapper>
        );
    }
}

export default PlayAudioSettings;
