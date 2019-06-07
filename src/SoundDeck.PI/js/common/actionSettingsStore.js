import React from "react";
import { createStore, applyMiddleware } from "redux"
import { connect as reduxConnect } from "react-redux"
import client from "./streamDeckClient";

const DEFAULT_STATE = { settings: {} };
const ACTIONS = {
        SET_ACTION_SETTINGS: "setActionSettings",
        SET_ACTION_SETTING: "setActionSetting"
    };

function reduce(state, action) {
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

function mapDispatchToProps(dispatch, ownProps) {
    return {
        onChange: (ev) => dispatch({
            type: ACTIONS.SET_ACTION_SETTING,
            value: { [ownProps.valuePath]: ev.target.value }
        })
    }
}

function mapStateToProps(state, ownProps) {
    return Object.assign({}, ownProps, { value: state.settings[ownProps.valuePath] });
}

const saveSettings = store => next => action => {
    next(action);

    if (action.type === ACTIONS.SET_ACTION_SETTING) {
        client.setSettings(store.getState().settings);
    }
}

const store = createStore(reduce, applyMiddleware(saveSettings));
client.connect()
    .then(conn => store.dispatch({
        type: ACTIONS.SET_ACTION_SETTINGS,
        value: conn.actionInfo.payload.settings
    }));

export const Context = React.createContext();

export const connect = (component) => {
    return reduxConnect(mapStateToProps, mapDispatchToProps, null, { context: Context })(component);
}

export default store;
