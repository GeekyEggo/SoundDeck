import React from "react";
import AudioDevices from "../components/audioDeviceSelect";
import client from "../common/streamDeckClient";
import Duration from "../components/durationSelect";
import FolderPicker from "../components/common/folderPicker"

class CaptureAudioBufferSettings extends React.Component {
    constructor(props) {
        super(props);

        this.outputPathOnClick = this.outputPathOnClick.bind(this);
        this.onSettingChange = this.onSettingChange.bind(this);

        this.state = {
            disabled: true,
            settings: {
                audioDeviceId: undefined,
                clipDuration: undefined,
                outputPath: undefined
            }
        }

        client.connect()
            .then(_ => this.setState({
                disabled: false,
                settings: client.actionInfo.payload.settings
            }));
    }

    onSettingChange(ev) {
        ev.persist();
        this.setState((state, _) => {
            if (state.settings[ev.target.id] !== ev.target.value) {
                state.settings[ev.target.id] = ev.target.value;
            }

            client.setSettings(state.settings);
            return state;
        });
    }

    async outputPathOnClick(ev) {
        this.state.disabled = true;
        this.setState(this.state);

        var response = await client.get("GetOutputPath");
        this.setState((state, _) => {
            state.disabled = false;
            if (response.payload.success) {
                state.settings.outputPath = response.payload.path;
            };

            return state;
        });
    }

    render() {
        return (
            <div className="sdpi-wrapper">
                <AudioDevices id="audioDeviceId" selected={this.state.settings.audioDeviceId} onChange={this.onSettingChange} />
                <Duration id="clipDuration" selected={this.state.settings.clipDuration} onChange={this.onSettingChange} />
                <FolderPicker label="Output Path" onClick={this.outputPathOnClick} value={this.state.settings.outputPath} />
                {this.state.disabled &&
                    <div className="disabler"></div>
                }
            </div>
        );
    }
}

export default CaptureAudioBufferSettings
