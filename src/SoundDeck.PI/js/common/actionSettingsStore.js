import React from "react";
import { createStore, applyMiddleware } from "redux"
import client from "./streamDeckClient";

const DEFAULT_STATE = { settings: {} };
const ACTIONS = {
        SET_ACTION_SETTINGS: "setActionSettings",
        SET_ACTION_SETTING: "setActionSetting"
    };

const reducer = (state, action) => {
    if (state == undefined) {
        return DEFAULT_STATE;
    }

    if (!Object.keys(ACTIONS).find(key => ACTIONS[key] === action.type)) {
        return state;
    }

    return Object.assign({}, state, {
        settings: {
            ...state.settings,
            ...action.value
        }
    });
}

const saveSettings = store => next => action => {
    next(action);

    if (action.type === ACTIONS.SET_ACTION_SETTING) {
        client.setSettings(store.getState().settings);
    }
}

export const mapDispatchToProps = (dispatch, ownProps) => {
    return {
        onChange: (ev) => dispatch({
            type: ACTIONS.SET_ACTION_SETTING,
            value: { [ownProps.valuePath]: ev.target.value }
        })
    }
}

export const mapStateToProps = (state, ownProps) => {
    let obj = Object.assign({}, ownProps, { value: state.settings[ownProps.valuePath] });
    console.log(obj);
    return obj;
}

export const Context = React.createContext();

const store = createStore(reducer, applyMiddleware(saveSettings));
client.connect()
    .then(conn => store.dispatch({
        type: ACTIONS.SET_ACTION_SETTINGS,
        value: conn.actionInfo.payload.settings
    }));

export default store;
