import React from "react";
import { connect } from "../actionSettingsStore";
import streamDeckClient from "../streamDeckClient";

class Select extends React.Component {
    constructor(props) {
        super(props);

        this.mapOptions = this.mapOptions.bind(this);
        this.state = props.options instanceof Array ? { options: [...props.options] } : { options: undefined };

        if (props.dataSourceUri) {
            streamDeckClient.get(props.dataSourceUri).then(r => this.setState({ options: r.payload.options }));
        }
    }

    mapOptions(item) {
        return item.children && item.children instanceof Array
            ? <optgroup key={item.label} label={item.label}>{item.children.map(this.mapOptions)}</optgroup>
            : <option key={item.value} value={item.value}>{item.label}</option>
    }

    render() {
        let options = this.state.options || this.props.options || [];

        return (
            <div className="sdpi-item">
                <label className="sdpi-item-label" htmlFor={this.props.id}>{this.props.label}</label>
                <select className="sdpi-item-value select" name={this.props.id} id={this.props.id} value={this.props.value || this.props.defaultValue} onChange={this.props.onChange}>
                    <option hidden disabled value="" />
                    {options.map(this.mapOptions)}
                </select>
            </div>
        );
    }
}

Select.defaultProps = {
    defaultValue: "",
    id: "",
    label: " ",
    onChange: undefined,
    options: undefined,
    value: undefined,
    valuePath: undefined
};

export default connect(Select);
