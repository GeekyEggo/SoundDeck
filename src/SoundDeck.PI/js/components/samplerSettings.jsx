import React from "react";
import { Checkbox, FolderPicker, PropertyInspectorWrapper, Range, Select } from "react-sharpdeck";
import PlayActionTypes from "../models/playActionTypes";

class SamplerSettings extends React.Component {
    constructor(props) {
        super(props);

        this.actionTypes = [
            PlayActionTypes.PlayNext,
            PlayActionTypes.PlayStop,
            PlayActionTypes.PlayOverlap,
            PlayActionTypes.LoopStop
        ];
    }

    render() {
        return (
            <PropertyInspectorWrapper>
                <Select label="Capture Device" dataSourceUri="GetAudioDevices" valuePath="captureAudioDeviceId" />
                <FolderPicker label="Output Path" pluginUri="GetOutputPath" valuePath="outputPath" />
                <Checkbox label="Encode to MP3" valuePath="encodeToMP3" defaultValue={false} id="encodeToMP3" />
                <Checkbox label="Normalize Volume" valuePath="normalizeVolume" defaultValue={true} id="normalizeVolume" />
                <hr />
                <Select label="Playback Device" dataSourceUri="GetPlaybackAudioDevices" valuePath="playbackAudioDeviceId" defaultValue="PLAYBACK_DEFAULT" />
                <Select label="Action" options={this.actionTypes} valuePath="action" defaultValue={PlayActionTypes.PlayNext.value} />
                <Range label="Volume" valuePath="playbackVolume" defaultValue="75" min="0" max="100" step="1" />
            </PropertyInspectorWrapper>
        );
    }
}

export default SamplerSettings;
