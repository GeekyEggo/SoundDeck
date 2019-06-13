﻿import connection from "./streamDeckSocket";

const EMPTY_HANDLER = (ev) => { },
    EVENTS = {
        DID_RECEIVE_GLOBAL_SETTINGS: "didReceiveGlobalSettings",
        DID_RECEIVE_SETTINGS: "didReceiveSettings",
        SEND_TO_PLUGIN: "sendToPlugin"
    };


/**
 * Generates a "unique" identifier.
 * @returns {string} The unique identifier.
 */
const getUUID = () => {
    let chr4 = () => Math.random().toString(16).slice(-4);
    return chr4() + chr4() + '-' + chr4() + '-' + chr4() + '-' + chr4() + '-' + chr4() + chr4() + chr4();
}

/**
 * Provides a wrapper for events that can be received or sent to the Elgato Stream Deck.
 */
class StreamDeckClient extends EventTarget {
    /**
     * Initializes a new Stream Deck client.
     * @param {Promise} conn The connetion as a promise.
     */
    constructor(conn) {
        super();

        /**
         * The didReceiveGlobalSettings event is received after calling the getGlobalSettings API to retrieve the global persistent data stored for the plugin.
         * @param {any} ev The event data, as sent by the Stream Deck.
         */
        this.onDidReceiveGlobalSettings = EMPTY_HANDLER;

        /**
         * The didReceiveSettings event is received after calling the getSettings API to retrieve the persistent data stored for the action.
         * @param {any} ev The event data, as sent by the Stream Deck.
         */
        this.onDidReceiveSettings = EMPTY_HANDLER

        /**
         * The Property Inspector will receive a sendToPropertyInspector event when the plugin sends a sendToPropertyInspector event.
         * @param {any} ev The event data, as sent by the Stream Deck.
         */
        this.onSendToPropertyInspector = EMPTY_HANDLER

        /**
         * Gets information about the Stream Deck, application, and devices available.
         */
        this.info;

        /**
         * Gets information about the action the Property Inspector is associated with.
         */
        this.actionInfo;

        conn.then((deck) => {
            this.actionInfo = deck.actionInfo;
            this.info = deck.info;

            deck.connection.addEventListener("streamDeckMessage", this.parseMessage.bind(this));
        });
    }

    /**
     * Connects to the Stream Deck, returning the connection as a promise.
     * @returns {Promise} The connection as a promise.
     */
    connect() {
        return connection;
    }

    /**
     * Sends a `get` request to the plugin, utilising SharpDeck libraries `PropertyInspectorMethod` attribute.
     * @param {string} event The name of the event or method, i.e. URI endpoint.
     * @param {any} payload The optional payload.
     */
    get(event, payload) {
        let request = {
            event: event,
            requestId: getUUID()
        };

        return this.connect()
            .then(client => client.connection.get(EVENTS.SEND_TO_PLUGIN, (data) => data.payload && data.payload.event == request.event && data.payload.requestId == request.requestId, { ...payload, ...request }));
    }

    /**
     * Triggers the `getGlobalSettings` event; upon receiving the settings, `onDidReceiveGlobalSettings` is raised.
     * @returns {Promise} The global settings as part of a promise.
     */
    getGlobalSettings() {
        return this.connect()
            .then(client => client.connection.get("getGlobalSettings", EVENTS.DID_RECEIVE_GLOBAL_SETTINGS));
    }

    /**
     * Triggers the `getSettings` event; upon receiving the settings, `onDidReceiveSettings` is raised.
     * @returns {Promise} The settings as part of a promise.
     */
    getSettings() {
        return this.connect()
            .then(client => client.connection.get("getSettings", EVENTS.DID_RECEIVE_SETTINGS));
    }

    /**
     * Logs a debug message to the logs file.
     * @param {string} msg The message to log.
     */
    logMessage(msg) {
        this.connect()
            .then(client => client.connection.sendPayload("logMessage", { message: msg }));
    }

    /**
     * Tells the Stream Deck application to open an URL in the default browser.
     * @param {string} url The URL to open.
     */
    openUrl(url) {
        this.connect()
            .then(client => client.connection.sendPayload("openUrl", { url: url }));
    }

    /**
     * Saves persistent data globally.
     * @param {any} settings The setting to save globally.
     */
    setGlobalSettings(settings) {
        this.connect()
            .then(client => client.connection.sendPayload("setGlobalSettings", settings));
    }

    /**
     * Save persistent data for the action's instance.
     * @param {any} settings The settings to save.
     */
    setSettings(settings) {
        this.connect()
            .then(client => client.connection.sendPayload("setSettings", settings));
    }

    /**
     * Sends payload information to the plugin.
     * @param {any} payload The payload information to send.
     */
    sendToPlugin(payload) {
        this.connect()
            .then(client => client.connection.sendActionPayload(EVENTS.SEND_TO_PLUGIN, payload));
    }

    /**
     * Parses a message received from the Stream Deck, and raises the appropriate event
     * @param {EventMessage} ev The event message.
     */
    parseMessage(ev) {
        switch (ev.data.event) {
            case EVENTS.DID_RECEIVE_GLOBAL_SETTINGS:
                this.onDidReceiveGlobalSettings(ev.data);
                break;

            case EVENTS.DID_RECEIVE_SETTINGS:
                this.onDidReceiveSettings(ev.data);
                break;

            case "sendToPropertyInspector":
                this.onSendToPropertyInspector(ev.data);
                break;
        }

        // dispatch the event
        this.dispatchEvent(new MessageEvent(ev.data.event, { data: ev.data }));
    }
}

export default new StreamDeckClient(connection);