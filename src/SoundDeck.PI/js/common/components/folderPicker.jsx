import React from "react";
import client from "../streamDeckClient";
import { connect } from "react-redux"
import { mapStateToProps, mapDispatchToProps } from "../actionSettingsStore";

class FolderPicker extends React.Component {
    constructor(props) {
        super(props);

        this.handleClick = this.handleClick.bind(this);
        this.state = { disabled: false }
    }

    async handleClick(ev) {
        this.setState({ disabled: true });

        var response = await client.get(this.props.pluginUri);
        if (response.payload.success) {
            this.props.onChange({ target: { value: response.payload.path } })
        }

        this.setState({ disabled: false });
    }

    render() {
        return (
            <div className="sdpi-item">
                <div className="sdpi-item-label">{this.props.label}</div>
                <div className="sdpi-item-group file">
                    <label className="sdpi-folder-info">{this.props.value || "No folder"}</label>
                    <button className="sdpi-folder-button" disabled={this.state.disabled} onClick={this.handleClick}>...</button>
                </div>
            </div>
        );
    }
}

FolderPicker.defaultProps = {
    id: undefined,
    label: undefined,
    onClick: undefined,
    value: undefined
}

export default connect(mapStateToProps, mapDispatchToProps)(FolderPicker);
