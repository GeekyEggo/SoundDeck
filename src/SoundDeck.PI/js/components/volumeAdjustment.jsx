import React from "react";
import { Range, Select, store } from "react-sharpdeck";

class VolumeAdjustment extends React.Component {
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
        const { settings: { volumeAction } } = store.getState();
        return volumeAction == 3 || volumeAction == 4 || volumeAction == 5;
    }

    render() {
        return (
            <React.Fragment>
                <Select label="Action" options={this.volumeActions} valuePath="volumeAction" defaultValue="0" />
                {this.state.isVolumeValueVisible &&
                    <Range label="Value" valuePath="volumeValue" defaultValue="100" min="0" max="100" step="5" />
                }
            </React.Fragment>
        );
    }
}

export default VolumeAdjustment;
