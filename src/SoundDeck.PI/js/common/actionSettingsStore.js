import client from "./streamDeckClient";

const defaultState = {
    settings: {}
};

export const SET_ACTION_SETTINGS = "setActionSettings";
export const SET_ACTION_SETTING = "setActionSetting";

export const settingsReducer = (state, action) => {
    if (state == undefined) {
        return defaultState;
    }

    if (action.type !== SET_ACTION_SETTING && action.type !== SET_ACTION_SETTINGS) {
        return state;
    }

    return Object.assign({}, state, {
        settings: {
            ...state.settings,
            ...action.value
        }
    });
}

export const mapDispatchToProps = (dispatch, ownProps) => {
    return {
        onChange: (ev) => dispatch(setSetting({ [ownProps.valuePath]: ev.target.value }))
    }
}

export const mapStateToProps = (state, ownProps) => {
    return Object.assign({}, ownProps, { value: state.settings[ownProps.valuePath] });
}

const setSetting = (value) => {
    return {
        type: SET_ACTION_SETTING,
        value: value,
    }
}

const setSettings = (settings) => {
    return {
        type: SET_ACTION_SETTINGS,
        value: settings
    }
}

export async function connectElgatoStreamDeck(store) {
    let connection = await client.connect();
    store.dispatch(setSettings(connection.actionInfo.payload.settings));
}

export const saveSettings = store => next => action => {
    next(action);

    if (action.type === SET_ACTION_SETTING) {
        client.setSettings(store.getState().settings);
    }
}