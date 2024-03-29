import React from "react";
import { PropertyInspectorWrapper, Select, store } from "react-sharpdeck";
import FilesPicker from "./filesPicker";
import PlayActionTypes from "../models/playActionTypes";

class PlayAudioSettings extends React.Component {
    constructor(props) {
        super(props);

        this.actionTypes = [
            PlayActionTypes.PlayNext,
            PlayActionTypes.PlayStop,
            PlayActionTypes.PlayAllStop,
            PlayActionTypes.PlayOverlap,
            PlayActionTypes.LoopStop,
            PlayActionTypes.LoopAllStop,
            PlayActionTypes.LoopAllStopReset
        ];

        this.orderTypes = [
            { label: "Sequential", value: "0" },
            { label: "Random", value: "1" },
        ];

        this.state = { enableSort: this.isSortEnabled() };
        store.subscribe(() => this.setState({ enableSort: this.isSortEnabled() }));
    }

    isSortEnabled() {
        const { settings: { order } } = store.getState();
        return order != 1; // default to true
    }

    render() {
        return (
            <PropertyInspectorWrapper>
                <Select label="Playback Device" dataSourceUri="GetPlaybackAudioDevices" valuePath="audioDeviceId" defaultValue="PLAYBACK_DEFAULT" />
                <Select label="Action" options={this.actionTypes} valuePath="action" defaultValue={PlayActionTypes.PlayNext.value} />
                <Select label="Order" options={this.orderTypes} valuePath="order" defaultValue="0" />
                <FilesPicker label="Files" valuePath="files" buttonLabel="Add files..." enableSort={this.state.enableSort} />
            </PropertyInspectorWrapper>
        );
    }
}

export default PlayAudioSettings;
