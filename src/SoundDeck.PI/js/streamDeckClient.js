const EMPTY_HANDLER = (ev) => { };

// the connection promise, used to determine if a connection has been established
let resolveConnection, rejectConnection;
const connection = new Promise((resolve, reject) => {
    resolveConnection = resolve;
    rejectConnection = reject;
});

/**
 * Handles connections between the Elgato Stream Deck, plugin, and property inspector; this function is called by Elgato.
 * @param {string} inPort The port that should be used to create the WebSocket
 * @param {string} inPropertyInspectorUUID A unique identifier string to register Property Inspector with Stream Deck software
 * @param {string} inRegisterEvent The event type that should be used to register the plugin once the WebSocket is opened. For Property Inspector this is
 * @param {string} inInfo A JSON object containing information about the application. (see below Info parameter)
 * @param {string} inActionInfo A JSON object containing information about the action. (see below inActionInfo parameter.
 */
window.connectElgatoStreamDeckSocket = function (inPort, inPropertyInspectorUUID, inRegisterEvent, inInfo, inActionInfo) {
    // initialize the socket
    const ws = new WebSocket('ws://localhost:' + inPort);
    const info = JSON.parse(inInfo);
    const actionInfo = JSON.parse(inActionInfo);

    /**
     * Provides an extension method, allowing for a request to be sent, and the resolver of a promise to be pushed to an array to be resolved later.
     * @param {string} event The event to trigger.
     * @param {ResolveArray} resolveArr The resolve array.
     */
    ws.get = async (event, resolveArr) => {
        await connection;
        return new Promise((resolve, _) => {
            resolveArr.push(resolve);
            ws.sendEvent(event);
        });
    }

    /**
     * Provides an extension method allowing for a payload to be sent to the current action.
     * @param {string} event The event name.
     * @param {any} payload The payload.
     */
    ws.sendActionPayload = (event, payload) => {
        ws.send(JSON.stringify({
            "action": actionInfo["action"],
            "event": event,
            "context": inPropertyInspectorUUID,
            "payload": payload
        }))
    }

    /**
     * Provides an extension method allowing for an event to be triggered on the Stream Deck.
     * @param {string} event The event name.
     */
    ws.sendEvent = (event) => {
        ws.send(JSON.stringify({
            "event": event,
            "context": inPropertyInspectorUUID
        }));
    }

    /**
     * Provides an extension methods allowing for a payload to be sent to the Stream Deck.
     * @param {string} event The event name.
     * @param {any} payload The payload.
     */
    ws.sendPayload = (event, payload) => {
        ws.send(JSON.stringify({
            "event": event,
            "context": inPropertyInspectorUUID,
            "payload": payload
        }));
    };

    // upon establishing a connection, register the property inspector to the Stream Deck.
    ws.onopen = function () {
        ws.send(JSON.stringify({
            event: inRegisterEvent,
            uuid: inPropertyInspectorUUID
        }));

        resolveConnection({
            actionInfo: actionInfo,
            info: info,
            connection: ws
        });
    };
};

class ResolveArray extends Array {
    constructor() {
        super();
    }

    resolveAll(obj) {
        while (this.length) {
            this.pop()(obj);
        }
    }
}

/**
 * Provides a wrapper for events that can be received or sent to the Elgato Stream Deck.
 */
class StreamDeckClient {
    /**
     * Initializes a new Stream Deck client.
     * @param {Promise} conn The connetion as a promise.
     */
    constructor(conn) {
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

        this.__resolveGlobalSettings = new ResolveArray();
        this.__resolveSettings = new ResolveArray();

        conn.then((deck) => {
            this.actionInfo = deck.actionInfo;
            this.info = deck.info;

            deck.connection.addEventListener("message", this.parseMessage.bind(this));
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
     * Triggers the `getGlobalSettings` event; upon receiving the settings, `onDidReceiveGlobalSettings` is raised.
     * @returns {Promise} The global settings as part of a promise.
     */
    getGlobalSettings() {
        return this.connect()
            .then(client => client.connection.get("getGlobalSettings", this.__resolveGlobalSettings));
    }

    /**
     * Triggers the `getSettings` event; upon receiving the settings, `onDidReceiveSettings` is raised.
     * @returns {Promise} The settings as part of a promise.
     */
    getSettings() {
        return this.connect()
            .then(client => client.connection.get("getSettings", this.__resolveSettings));
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
            .then(client => client.connection.sendActionPayload("sendToPlugin", payload));
    }

    /**
     * Parses a message received from the Stream Deck, and raises the appropriate event
     * @param {EventMessage} ev The event message.
     */
    parseMessage(ev) {
        let data = JSON.parse(ev.data);
        switch (data.event) {
            case "didReceiveGlobalSettings":
                this.onDidReceiveGlobalSettings(data);
                this.__resolveGlobalSettings.resolveAll(data);
                break;

            case "didReceiveSettings":
                this.onDidReceiveSettings(data);
                this.__resolveSettings.resolveAll(data);
                break;

            case "sendToPropertyInspector":
                this.onSendToPropertyInspector(data);
                break;
        }
    }
};

export default new StreamDeckClient(connection);
