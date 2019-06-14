import React from "react";
import { createStore, applyMiddleware } from "redux"
import { connect as reduxConnect } from "react-redux"
import objectPath from "object-path";
import client from "./streamDeckClient";

const DEFAULT_STATE = { settings: {} };
const ACTIONS = {
        SET_ACTION_SETTINGS: "setActionSettings",
        SET_ACTION_SETTING: "setActionSetting"
    };

/**
 * Reduces the state based on the specified action.
 * @param {any} state The current state.
 * @param {any} action The action information.
 * @returns {any} The new state.
 */
function reduce(state, action) {
    if (state == undefined) {
        return DEFAULT_STATE;
    }

    if (!Object.keys(ACTIONS).find(key => ACTIONS[key] === action.type)) {
        return state;
    }

    // merge the new value into the current state
    return Object.assign({}, state, {
        settings: {
            ...state.settings,
            ...action.value
        }
    });
}

/**
 * Handles mapping dispatching to properties of an element.
 * @param {Function} dispatch The dispatch function, use to trigger updates on the store.
 * @param {any} ownProps The owned properties of the element.
 * @returns {any} The properties that can be used for dispatching.
 */
function mapDispatchToProps(dispatch, ownProps) {
    return {
        onChange: (ev) => {
            let value = {};
            objectPath.set(value, ownProps.valuePath, ev.target.value);

            dispatch({
                type: ACTIONS.SET_ACTION_SETTING,
                value: value
            })
        }
    }
}

/**
 * Maps the redux store state to an elements properties.
 * @param {any} state The state of the store.
 * @param {any} ownProps The elements properties.
 * @returns {any} The state of the element.
 */
function mapStateToProps(state, ownProps) {
    return Object.assign({}, ownProps, { value: objectPath.get(state.settings, ownProps.valuePath) });
}

/**
 * Provides a middleware for saving the current state to the Elgato Stream Deck plugin.
 * @param {any} store The store.
 */
const saveSettingsToStreamDeck = store => next => action => {
    next(action);

    if (action.type === ACTIONS.SET_ACTION_SETTING) {
        client.setSettings(store.getState().settings);
    }
}

// the main store, initialized after the Stream Deck has connected to the plugin
const store = createStore(reduce, applyMiddleware(saveSettingsToStreamDeck));
client.connect()
    .then(conn => store.dispatch({
        type: ACTIONS.SET_ACTION_SETTINGS,
        value: conn.actionInfo.payload.settings
    }));

// an extended connect method, used to establish a connection to this store
export const connect = (component) => {
    return reduxConnect(mapStateToProps, mapDispatchToProps, null, { context: Context })(component);
}

export const Context = React.createContext();
export default store;
