import React, { useState } from "react";
import { sortableContainer, sortableElement, sortableHandle } from "react-sortable-hoc";
import { connect, streamDeckClient } from "react-sharpdeck";

const DEFAULT_VOLUME = 0.75;

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

    /*
     * Bubbles the deletion after reseting the options; this prevents subsequent item options from showing.
     */
    function handleDelete() {
        setShowOptions(false);
        onDelete(fileIndex);
    }

    /**
     * Sends a request to the plug-in to play the audio clip.
     */
    function handlePlay() {
        streamDeckClient.sendToPlugin({
            event: "TestVolume",
            path: item.path,
            volume: item.volume || DEFAULT_VOLUME
        })
    }

    return (
        <li className="sortable">
            {showOptions
                ? <ClipOptions handlePlay={handlePlay} onDelete={handleDelete} onVolumeChanged={volume => onVolumeChanged({ index: fileIndex, path: item.path, volume})} volume={item.volume} />
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
function ClipOptions({ handlePlay, onDelete, onVolumeChanged, volume }) {
    const [rangeVolume, setVolume] = useState(Math.round((volume || DEFAULT_VOLUME) * 100));

    /**
     * Handles the volume changing; this can occur when the mouse is moving the slider, or when a key press changes the value.
     * @param {Event} ev The event that triggered the volume change.
     */
    function handleVolumeChange(ev) {
        setVolume(ev.target.value);

        // when a change occurred via a key, invoke changed
        const keyCode = ev.nativeEvent.keyCode;
        if (keyCode == 37 || keyCode == 38 || keyCode == 39 || keyCode == 40) {
            onVolumeChanged(rangeVolume / 100);
        }
    }

    return (
        <React.Fragment>
            <span className="volume-icon icon sortable_icon can-click" onClick={handlePlay}></span>
            <span className="sortable_value">
                <input
                    type="range"
                    min="0"
                    max="100"
                    value={rangeVolume}
                    onChange={handleVolumeChange}
                    onKeyUp={handleVolumeChange}
                    onMouseUp={() => onVolumeChanged(rangeVolume / 100)} />
            </span>
            <span className="sortable_icon">{rangeVolume}</span>
            <span className="delete-handle icon sortable_icon flex-right can-click" title="Remove" onClick={onDelete}></span>
        </React.Fragment>
    )
}

class FilesPicker extends React.Component {
    constructor(props) {
        super(props);

        this.handleAddFiles = this.handleAddFiles.bind(this);
        this.handleChange = this.handleChange.bind(this);
        this.handleDelete = this.handleDelete.bind(this);
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
        });
    }

    /**
     * Handles the add files button event; this displays a prompt, and allows the the user to select files to be added to the playlist.
     */
    handleAddFiles() {
        streamDeckClient.sendToPlugin({
            event: "AddFiles"
        });
    }

    /**
     * Handles the deletion of a file path, from the state.
     * @param {Number} index The index of the path being deleted.
     */
    handleDelete(index) {
        streamDeckClient.sendToPlugin({
            event: "RemoveFile",
            index: index
        });
    }

    /**
     * Handles an item within the list being sorted / moved.
     * @param {Object} obj The properties of the item.
     * @param {Number} obj.oldIndex The old index.
     * @param {Number} obj.newIndex The new index.
     */
    handleSortEnd({ oldIndex, newIndex }) {
        streamDeckClient.sendToPlugin({
            event: "MoveFile",
            oldIndex: oldIndex,
            newIndex: newIndex
        });
    }

    /**
     * Handles an items volume changing.
     * @param {Number} index The index of the item being changed.
     * @param {Number} volume The new desired volume.
     */
    handleVolumeChanged(file) {
        streamDeckClient.sendToPlugin({
            event: "SetVolume",
            ...file
        });
    }

    render() {
        const id = this.props.id || this.props.valuePath;

        return (
            <div>
                <div className="sdpi-item">
                    <div className="sdpi-item-label">{this.props.label}</div>
                    <div className="sdpi-item-group file">
                        <button className="sdpi-file-button sdpi-item-value max100 input__margin text-center" onClick={this.handleAddFiles}>{this.props.buttonLabel}</button>
                    </div>
                </div>

                <div type="list" className="sdpi-item list">
                    <div className="sdpi-item-label opacity-zero">&nbsp;</div>
                    <FileList className="sdpi-item-value files-list"
                        enableSort={this.props.enableSort}
                        items={this.props.value || []}
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
