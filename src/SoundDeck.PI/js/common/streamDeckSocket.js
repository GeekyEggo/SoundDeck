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
    const ws = new WebSocket('ws://localhost:' + inPort),
        info = JSON.parse(inInfo),
        actionInfo = JSON.parse(inActionInfo),
        requests = [];

    /**
     * Sends a request to the web socket, and returns a promise that is awaiting a message matching the specified awaitEvent.
     * @param {string} event The event to trigger.
     * @param {string|Function} isMatch The event to await, or a predicate to wait for.
     * @param {any} payload The optional payload
     */
    ws.get = (event, waitFor, payload) => {
        return new Promise((resolve, _) => {
            requests.push({
                isMatch: typeof (waitFor) === 'string' || waitFor instanceof String ? (data) => data.event == waitFor : waitFor,
                resolve: resolve
            });

            ws.sendActionPayload(event, payload);
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
    }

    // listen for messages, handling any outstanding requests, and bubbling the original request with parsed data
    ws.addEventListener("message", (ev) => {
        let data = JSON.parse(ev.data);

        // determine if there are any outstanding requests
        let i = requests.length;
        while (i--) {
            if (requests[i].isMatch(data)) {
                requests[i].resolve(data);
                requests.splice(i, 1);
            }
        }

        // bubble the message, but with the original data parsed as an object
        ws.dispatchEvent(new MessageEvent("streamDeckMessage", {
            data: data
        }));
    })

    // upon establishing a connection, register the property inspector to the Stream Deck
    ws.addEventListener("open", () => {
        ws.send(JSON.stringify({
            event: inRegisterEvent,
            uuid: inPropertyInspectorUUID
        }));

        resolveConnection({
            actionInfo: actionInfo,
            info: info,
            connection: ws
        });
    });
}

export default connection;
