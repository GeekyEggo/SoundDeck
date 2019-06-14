import React from "react";
import { connect } from "../actionSettingsStore";

class CheckBox extends React.Component {
    constructor(props) {
        super(props);

        this.state = { checked: this.props.value instanceof Boolean ? this.props.value : this.props.defaultValue };
        this.handleClick = this.handleClick.bind(this);
    }

    handleClick(ev) {
        this.setState({ checked: ev.target.checked });
        this.props.onChange({ target: { value: ev.target.checked } });
    }

    render() {
        const id = this.props.id || this.props.valuePath;

        return (
            <div type="checkbox" className="sdpi-item">
                <div className="sdpi-item-label">{this.props.label}</div>
                <input className="sdpi-item-value" id={id} type="checkbox" checked={this.state.checked} onChange={this.handleClick} />
                <label htmlFor={id}><span></span>{this.props.checkBoxLabel}</label>
            </div>
        );
    }
}

CheckBox.defaultProps = {
    checkBoxLabel: "",
    defaultValue: false,
    id: "",
    label: " ",
    value: undefined,
    valuePath: undefined
};

export default connect(CheckBox);
