﻿import React from "react";

class Select extends React.Component {
    constructor(props) {
        super(props);

        this.handleChange = this.handleChange.bind(this);
        this.state = { value: props.value };
    }

    getOptions() {
        return this.props.options.map(item => {
            return item.children && item.children instanceof Array
                ? <optgroup key={item.label} label={item.label}>{item.children.map(this.getOption)}</optgroup>
                : this.getOption(item);
        });
    }

    getOption(item) {
        return <option key={item.value} value={item.value}>{item.label}</option>
    }

    handleChange(ev) {
        this.setState({ value: ev.target.value });
        this.props.onChange && this.props.onChange(ev);
    }

    render() {
        return (
            <div className="sdpi-item">
                <label className="sdpi-item-label" htmlFor={this.props.id}>{this.props.label}</label>
                <select className="sdpi-item-value select" name={this.props.id} id={this.props.id} value={this.props.value || this.state.value} onChange={this.handleChange}>
                    {this.getOptions()}
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
    value: undefined
};

export default Select;
