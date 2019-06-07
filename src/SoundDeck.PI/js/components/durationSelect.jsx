import React from "react";
import Select from "../common/components/select";

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
        return <Select label="Duration" {...this.props} {...this.state} />
    }
}

DurationSelect.defaultProps = {
    id: undefined,
    onChange: undefined,
    value: undefined,
    valuePath: undefined
};

export default DurationSelect;
