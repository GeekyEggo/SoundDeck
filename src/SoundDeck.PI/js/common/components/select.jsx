import React from "react";
import { connect } from "../actionSettingsStore";

class Select extends React.Component {
    constructor(props) {
        super(props);
        this.mapOptions = this.mapOptions.bind(this);
    }

    mapOptions(item) {
        return item.children && item.children instanceof Array
            ? <optgroup key={item.label} label={item.label}>{item.children.map(this.mapOptions)}</optgroup>
            : <option key={item.value} value={item.value}>{item.label}</option>
    }

    render() {
        return (
            <div className="sdpi-item">
                <label className="sdpi-item-label" htmlFor={this.props.id}>{this.props.label}</label>
                <select className="sdpi-item-value select" name={this.props.id} id={this.props.id} value={this.props.value} onChange={this.props.onChange}>
                    {this.props.options.map(this.mapOptions)}
                </select>
            </div>
        );
    }
}

Select.defaultProps = {
    id: "",
    label: " ",
    onChange: undefined,
    options: [],
    value: undefined,
    valuePath: undefined
};

export default connect(Select);
