import React from "react";
import { connect } from "react-sharpdeck";

class FilesPicker extends React.Component {
    constructor(props) {
        super(props);
        this.state = { value: [...this.props.value || []] };

        this.handleFileChange = this.handleFileChange.bind(this);
        this.getFileListItem = this.getFileListItem.bind(this);
    }

    handleFileChange(ev) {
        // ensure the file isnt already in the array
        const path = decodeURIComponent(ev.target.files[0].name);
        if (this.state.value.indexOf(path) > -1) {
            return;
        }

        // update the state
        this.setState(s => {
            s.value = [...s.value, path];
            this.props.onChange({ target: { value: s.value } });

            return s;
        })
    }

    handleDelete(path) {
        this.setState(s => {
            s.value = s.value.filter(item => item !== path);
            this.props.onChange({ target: { value: value } });

            return s;
        })
    }

    getFileListItem(path) {
        const name = path.replace(/^.*[\\\/]/, '')
        return (<li key={path}>{name} <span onClick={this.handleDelete.bind(this, path)}>x</span></li>);
    }

    render() {
        return (
            <div>
                <div className="sdpi-item">
                    <div className="sdpi-item-label">{this.props.label}</div>
                    <div className="sdpi-item-group file">
                        <input className="sdpi-item-value" type="file" id="elgfilepicker" accept={this.props.accept} onChange={this.handleFileChange} />
                        <label className="sdpi-file-label max100 input__margin text-center" htmlFor="elgfilepicker">{this.props.buttonLabel}</label>
                    </div>
                </div>

                    <div type="list" className="sdpi-item list">
                    <div className="sdpi-item-label opacity-zero">&nbsp;</div>
                    {this.state.value.length > 0 ? (
                        <ol className="sdpi-item-value files-list">
                            {this.state.value.map(this.getFileListItem)}
                        </ol>
                    ) : (
                        <div>No files.</div>
                    )}
                    </div>
            </div>
        );
    }
}

FilesPicker.defaultProps = {
    accept: "",
    buttonLabel: "Choose File...",
    id: "",
    label: " ",
    valuePath: undefined
};

export default connect(FilesPicker);
