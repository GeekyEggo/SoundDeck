import React from "react";
import Select from "./common/select";

class DurationSelect extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            options: [
                { label: "15 seconds", value: "15"},
                { label: "30 seconds", value: "30"},
                { label: "60 seconds", value: "60"},
                { label: "90 seconds", value: "90"},
                { label: "120 seconds", value: "120"}
            ]
        }
    }

    render() {
        return <Select
            id={this.props.id}
            label="Duration"
            onChange={this.props.onChange}
            options={this.state.options}
            selected={this.props.selected} />
    }
}

DurationSelect.defaultProps = {
    id: undefined,
    onChange: undefined,
    selected: undefined
};

export default DurationSelect;
