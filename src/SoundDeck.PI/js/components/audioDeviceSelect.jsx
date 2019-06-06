import React from "react";
import Select from "./select";
import soundDeck from "../soundDeck";
import streamDeckClient from "../streamDeckClient";

class AudioDeviceSelect extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            options: []
        };
    }

    async componentDidMount() {
        let options = [];
        const getGroup = (device) => {
            let grp = options.find(grp => grp.label == soundDeck.enums.FLOW[device.flow]) || this.transformDeviceToOptionGroup(device);
            if (grp.children.length == 0) {
                options.push(grp);
            }

            return grp;
        };

        // get the devices, and add them to their respective groups
        let devices = await streamDeckClient.get("GetAudioDevices");
        devices.payload.devices.forEach(d => getGroup(d).children.push(this.transformDeviceToOption(d)));

        this.setState({
            options: options
        });
    }

    transformDeviceToOption(device) {
        return {
            label: device.friendlyName,
            value: device.id
        }
    }

    transformDeviceToOptionGroup(device) {
        return {
            label: soundDeck.enums.FLOW[device.flow],
            children: []
        };
    }

    render() {
        return <Select
            id={this.props.id}
            label="Device"
            onChange={this.props.onChange}
            options={this.state.options}
            selected={this.props.selected} />
    }
}

AudioDeviceSelect.defaultProps = {
    id: undefined,
    onChange: undefined,
    selected: undefined
};

export default AudioDeviceSelect;
