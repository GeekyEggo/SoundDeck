import React from "react";

class FolderPicker extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        console.log(this.props.value);
        return (
            <div className="sdpi-item">
                <div className="sdpi-item-label">{this.props.label}</div>
                <div className="sdpi-item-group file">
                    <label className="sdpi-folder-info">{this.props.value || "No folder"}</label>
                    <label className="sdpi-file-label" onClick={this.props.onClick}>...</label>
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

export default FolderPicker;
