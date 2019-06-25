import React from "react";
import { sortableContainer, sortableElement, sortableHandle } from "react-sortable-hoc";
import arrayMove from "array-move";
import { connect } from "react-sharpdeck";

// a component wrapper of a sortable container, in the form of an ordered list
const SortableList = sortableContainer(({ className, items, onDelete }) => {
    return (
        <ol className={className}>
            {items.map((value, index) => (
                <SortableItem key={`item-${index}`} index={index} value={value} onDelete={onDelete} title="Move" />
            ))}
        </ol>
    );
});

// a component wrapper of a sortable element, in the form of a list item
const SortableItem = sortableElement(({ onDelete, index, value }) => {
    return (
        <li className="sortable">
            <DragHandle />
            <span className="sortable_value">{value}</span>
            <span className="delete-handle sortable_icon flex-right" title="Remove" onClick={() => onDelete(index)}></span>
        </li>
    );
});

// a component wrapper of a sortable handle, used to move items within the list
const DragHandle = sortableHandle(() => {
    return (
        <span className="drag-handle sortable_icon">&nbsp;</span>
    )
});

class FilesPicker extends React.Component {
    constructor(props) {
        super(props);
        this.state = { value: [...this.props.value || []] };

        this.handleChange = this.handleChange.bind(this);
        this.handleDelete = this.handleDelete.bind(this);
        this.handleFileChange = this.handleFileChange.bind(this);
        this.handleSortEnd = this.handleSortEnd.bind(this);
    }

    /**
     * Handles a generic change that should be made to the state, in the form of a delegate.
     * @param {Function} getUpdatedState The function to apply to the current state, that returns the new state.
     */
    handleChange(getUpdatedState) {
        this.setState(s => {
            s.value = getUpdatedState(s);
            this.props.onChange({ target: { value: s.value } });

            return s;
        })
    }

    /**
     * Handles the file change event; this mimics a file being selected, and results it in being added to the distinct state of files.
     * @param {any} ev The event arguments.
     */
    handleFileChange(ev) {
        // ensure the file isnt already in the array
        const path = decodeURIComponent(ev.target.files[0].name);
        if (this.state.value.indexOf(path) > -1) {
            return;
        }

        this.handleChange((state) => [...state.value, path]);
    }

    /**
     * Handles the deletion of a file path, from the state.
     * @param {Number} index The index of the path being deleted.
     */
    handleDelete(index) {
        this.handleChange((state) => {
            state.value.splice(index, 1)
            return state.value;
        });
    }

    /**
     * Handles an item within the list being sorted / moved.
     * @param {Object} obj The properties of the item.
     * @param {Number} obj.oldIndex The old index.
     * @param {Number} obj.newIndex The new index.
     */
    handleSortEnd({ oldIndex, newIndex }) {
        this.handleChange((state) => arrayMove(state.value, oldIndex, newIndex));
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
                    <SortableList className="sdpi-item-value files-list"
                        items={this.state.value.map(path => path.replace(/^.*[\\\/]/, ''))}
                        useDragHandle={true}
                        lockAxis="y"
                        onDelete={this.handleDelete}
                        onSortEnd={this.handleSortEnd} />
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
