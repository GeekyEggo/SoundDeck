import React from "react";
import { Select, store, TextField } from "react-sharpdeck";

class ProcessSelector extends React.Component {
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
            <React.Fragment>
                <Select label="Process" options={this.processSelectionOptions} valuePath="processSelectionType" defaultValue="0" />
                {this.state.isProcessNameVisible &&
                    <TextField label="Name" valuePath="processName" />
                }
            </React.Fragment>
        );
    }
}

export default ProcessSelector;