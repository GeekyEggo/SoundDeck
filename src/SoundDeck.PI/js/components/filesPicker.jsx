import React, { useState } from "react";
import { sortableContainer, sortableElement, sortableHandle } from "react-sortable-hoc";
import arrayMove from "array-move";
import { connect } from "react-sharpdeck";

// a component wrapper of a sortable container, in the form of an ordered list
const FileList = sortableContainer(({ className, enableSort, items, onDelete, onVolumeChanged }) => {
    return (
        <ol className={className}>
            {items.map((item, index) => (
                <FileItem
                    key={`item-${index}`}
                    index={index}
                    fileIndex={index}
                    item={item}
                    onDelete={onDelete}
                    onVolumeChanged={onVolumeChanged}
                    enableSort={enableSort} />
            ))}
        </ol>
    );
});

// a component wrapper of a sortable handle, used to move items within the list
const DragHandle = sortableHandle(() => {
    return (
        <span className="drag-handle sortable_icon">&nbsp;</span>
    )
});

// a component wrapper of a sortable element, in the form of a list item
const FileItem = sortableElement(({ enableSort, fileIndex, item, onDelete, onVolumeChanged }) => {
    const [showOptions, setShowOptions] = useState(false);
    const [volume, setVolume] = useState(item.volume || 100);

    /*
     * Bubbles the deletion after reseting the options; this prevents subsequent item options from showing.
     */
    function handleDelete() {
        setShowOptions(false);
        onDelete(fileIndex);
    }

    return (
        <li className="sortable">
            {showOptions
                ? <ClipOptions onDelete={handleDelete} volume={volume} onVolumeChange={ev => setVolume(ev.target.value)} onVolumeChanged={() => onVolumeChanged(fileIndex, volume)} />
                : <ClipInfo enableSort={enableSort} fileName={item.path.replace(/^.*[\\\/]/, '')} />
            }
            <span className="cog-handle icon sortable_icon flex-right can-click" title="Options" onClick={() => setShowOptions(!showOptions)}></span>
        </li>
    );
});

// shows the clip info, including the drag handle if available
function ClipInfo({ enableSort, fileName }) {
    return (
        <React.Fragment>
            {enableSort && <DragHandle /> }
            <span className="sortable_value">{fileName}</span>
        </React.Fragment>
    );
}

// shows the clip options
function ClipOptions({ onDelete, onVolumeChange, onVolumeChanged, volume }) {
    return (
        <React.Fragment>
            <span className="volume-icon icon sortable_icon"></span>
            <span className="sortable_value">
                <input type="range" min="50" max="125" value={volume} onChange={onVolumeChange} onMouseUp={onVolumeChanged} />
            </span>
            <span className="sortable_icon">{volume}</span>
            <span className="delete-handle icon sortable_icon flex-right can-click" title="Remove" onClick={onDelete}></span>
        </React.Fragment>
    )
}

class FilesPicker extends React.Component {
    constructor(props) {
        super(props);
        this.state = { value: [...this.props.value || []] };

        this.handleChange = this.handleChange.bind(this);
        this.handleDelete = this.handleDelete.bind(this);
        this.handleFileChange = this.handleFileChange.bind(this);
        this.handleSortEnd = this.handleSortEnd.bind(this);
        this.handleVolumeChanged = this.handleVolumeChanged.bind(this);
    }

    /**
     * Handles a generic change that should be made to the state, in the form of a delegate.
     * @param {Function} getValue Gets the updated value based on the current states value, returning the new value to be applied to the state.
     */
    handleChange(getValue) {
        this.setState(state => {
            state.value = getValue(state.value);
            this.props.onChange({ target: { value: state.value } });

            return state;
        })
    }

    /**
     * Handles the file change event; this mimics a file being selected, and results it in being added to the distinct state of files.
     * @param {any} ev The event arguments.
     */
    handleFileChange(ev) {
        const files = Array.from(ev.target.files);

        this.handleChange(value => {
            return files.reduce((seed, { name }) => {
                seed.push({ path: decodeURIComponent(name) });
                return seed;
            }, [...value]);
        });
    }

    /**
     * Handles the deletion of a file path, from the state.
     * @param {Number} index The index of the path being deleted.
     */
    handleDelete(index) {
        this.handleChange(value => {
            value.splice(index, 1)
            return value;
        });
    }

    /**
     * Handles an item within the list being sorted / moved.
     * @param {Object} obj The properties of the item.
     * @param {Number} obj.oldIndex The old index.
     * @param {Number} obj.newIndex The new index.
     */
    handleSortEnd({ oldIndex, newIndex }) {
        this.handleChange(value => arrayMove(value, oldIndex, newIndex));
    }

    /**
     * Handles an items volume changing.
     * @param {Number} index The index of the item being changed.
     * @param {Number} volume The new desired volume.
     */
    handleVolumeChanged(index, volume) {
        this.handleChange(value => {
            value[index].volume = volume;
            return value;
        })
    }

    render() {
        const id = this.props.id || this.props.valuePath;

        return (
            <div>
                <div className="sdpi-item">
                    <div className="sdpi-item-label">{this.props.label}</div>
                    <div className="sdpi-item-group file">
                        <input className="sdpi-item-value" type="file" value={""} multiple id={id} accept={this.props.accept} onChange={this.handleFileChange} />
                        <label className="sdpi-file-label max100 input__margin text-center" htmlFor={id}>{this.props.buttonLabel}</label>
                    </div>
                </div>

                <div type="list" className="sdpi-item list">
                    <div className="sdpi-item-label opacity-zero">&nbsp;</div>
                    <FileList className="sdpi-item-value files-list"
                        enableSort={this.props.enableSort}
                        items={this.state.value}
                        lockAxis="y"
                        onDelete={this.handleDelete}
                        onSortEnd={this.handleSortEnd}
                        onVolumeChanged={this.handleVolumeChanged}
                        useDragHandle={true} />
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
