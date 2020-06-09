import React from "react";
import { Checkbox, FolderPicker, PropertyInspectorWrapper, Select } from "react-sharpdeck";
import PlayActionTypes from "../models/playActionTypes";

class SamplerSettings extends React.Component {
    constructor(props) {
        super(props);

        this.actionTypes = [
            PlayActionTypes.PlayNext,
            PlayActionTypes.PlayStop,
            PlayActionTypes.LoopStop
        ];
    }

    render() {
        return (
            <PropertyInspectorWrapper>
                <Select label="Capture Device" dataSourceUri="GetCaptureAudioDevices" valuePath="captureAudioDeviceId" />
                <FolderPicker label="Output Path" pluginUri="GetOutputPath" valuePath="outputPath" />
                <Checkbox label="Encode to MP3" valuePath="encodeToMP3" defaultValue={false} id="encodeToMP3" />
                <Checkbox label="Normalize Volume" valuePath="normalizeVolume" defaultValue={true} id="normalizeVolume" />
                <hr />
                <Select label="Playback Device" dataSourceUri="GetPlaybackAudioDevices" valuePath="playbackAudioDeviceId" />
                <Select label="Action" options={this.actionTypes} valuePath="action" defaultValue={PlayActionTypes.PlayNext.value} />
            </PropertyInspectorWrapper>
        );
    }
}

export default SamplerSettings
