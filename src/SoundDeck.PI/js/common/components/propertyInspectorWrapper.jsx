import React from "react";
import { Provider } from "react-redux"
import store, { Context } from "../actionSettingsStore"

class PropertyInspectorWrapper extends React.Component {
    render() {
        return (
            <Provider store={store} context={Context}>
                <div className="sdpi-wrapper">{this.props.children}</div>
            </Provider>
        );
    }
}

export default PropertyInspectorWrapper;
