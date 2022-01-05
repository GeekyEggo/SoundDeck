(function (global, factory) {
    typeof exports === 'object' && typeof module !== 'undefined' ? factory(exports, require('react'), require('redux'), require('react-redux'), require('object-path'), require('event-target-shim')) :
    typeof define === 'function' && define.amd ? define(['exports', 'react', 'redux', 'react-redux', 'object-path', 'event-target-shim'], factory) :
    (global = typeof globalThis !== 'undefined' ? globalThis : global || self, factory(global["react-sharpdeck"] = {}, global.React, global.redux, global.reactRedux, global.objectPath, global.eventTargetShim));
})(this, (function (exports, React, redux, reactRedux, objectPath, eventTargetShim) { 'use strict';

    function _interopDefaultLegacy (e) { return e && typeof e === 'object' && 'default' in e ? e : { 'default': e }; }

    var React__default = /*#__PURE__*/_interopDefaultLegacy(React);
    var objectPath__default = /*#__PURE__*/_interopDefaultLegacy(objectPath);

    /*! *****************************************************************************
    Copyright (c) Microsoft Corporation.

    Permission to use, copy, modify, and/or distribute this software for any
    purpose with or without fee is hereby granted.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
    REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
    AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
    INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
    LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
    OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
    PERFORMANCE OF THIS SOFTWARE.
    ***************************************************************************** */

    function __awaiter(thisArg, _arguments, P, generator) {
        function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
            function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
            function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    }

    /**
     * Provides a connection between the property inspector and the Stream Deck.
     */
    class StreamDeckConnection extends eventTargetShim.EventTarget {
        /**
         * Initializes a new instance of a Stream Deck connection.
         * @constructor
         */
        constructor() {
            super();
            this.requests = [];
            this.connection = new Promise((resolve, reject) => {
                this.resolveConnection = resolve;
            });
        }
        /**
         * Returns the underlying promise that determines if the connection is connected.
         */
        connect() {
            return this.connection;
        }
        /**
         * Registers the connection with the Elgato Stream Deck
         * @param inPort The port that should be used to create the WebSocket
         * @param inPropertyInspectorUUID A unique identifier string to register Property Inspector with Stream Deck software
         * @param inRegisterEvent The event type that should be used to register the plugin once the WebSocket is opened. For Property Inspector this is
         * @param inInfo A JSON object containing information about the application. (see below Info parameter)
         * @param inActionInfo A JSON object containing information about the action. (see below inActionInfo parameter.
         */
        register(inPort, inPropertyInspectorUUID, inRegisterEvent, inInfo, inActionInfo) {
            // set the local state of the connection
            this.inPropertyInspectorUUID = inPropertyInspectorUUID;
            this.inRegisterEvent = inRegisterEvent;
            this.info = JSON.parse(inInfo);
            this.actionInfo = JSON.parse(inActionInfo);
            // register the web socket
            this.webSocket = new WebSocket(`ws://localhost:${inPort}`);
            this.webSocket.addEventListener("message", this.onMessage.bind(this));
            this.webSocket.addEventListener("open", this.onOpen.bind(this));
        }
        /**
         * Sends a request to the web socket, and returns a promise that is awaiting a message matching the specified awaitEvent.
         * @param event The event to trigger.
         * @param isMatch The event to await, or a predicate to wait for.
         * @param payload The optional payload
         */
        get(event, waitFor, payload) {
            return new Promise((resolve, _) => {
                this.requests.push({
                    isMatch: typeof (waitFor) === 'string' || waitFor instanceof String ? (data) => data.event == waitFor : waitFor,
                    resolve: resolve
                });
                this.sendActionPayload(event, payload);
            });
        }
        /**
         * Provides an extension method allowing for a payload to be sent to the current action.
         * @param event The event name.
         * @param payload The payload.
         */
        sendActionPayload(event, payload) {
            this.webSocket.send(JSON.stringify({
                action: this.actionInfo["action"],
                event: event,
                context: this.inPropertyInspectorUUID,
                payload: payload
            }));
        }
        /**
         * Provides an extension method allowing for an event to be triggered on the Stream Deck.
         * @param event The event name.
         */
        sendEvent(event) {
            this.webSocket.send(JSON.stringify({
                event: event,
                context: this.inPropertyInspectorUUID
            }));
        }
        /**
         * Provides an extension methods allowing for a payload to be sent to the Stream Deck.
         * @param {string} event The event name.
         * @param {any} payload The payload.
         */
        sendPayload(event, payload) {
            this.webSocket.send(JSON.stringify({
                event: event,
                context: this.inPropertyInspectorUUID,
                payload: payload
            }));
        }
        /**
         * Handles the message event of the web socket.
         * @param ev The event arguments.
         */
        onMessage(ev) {
            let data = JSON.parse(ev.data);
            // determine if there are any outstanding requests
            let i = this.requests.length;
            while (i--) {
                if (this.requests[i].isMatch(data)) {
                    this.requests[i].resolve(data);
                    this.requests.splice(i, 1);
                }
            }
            // bubble the message, but with the original data parsed as an object
            this.dispatchEvent(new MessageEvent("streamDeckMessage", {
                data: data
            }));
        }
        /**
         * Handles the open event of the web socket.
         * @param ev The event arguments.
         */
        onOpen(ev) {
            this.webSocket.send(JSON.stringify({
                event: this.inRegisterEvent,
                uuid: this.inPropertyInspectorUUID
            }));
            this.resolveConnection(this);
        }
    }
    const connection = new StreamDeckConnection();
    window.connectElgatoStreamDeckSocket = connection.register.bind(connection);

    /**
     * Generates a "unique" identifier.
     * @returns The unique identifier.
     */
    function getUUID() {
        let chr4 = () => Math.random().toString(16).slice(-4);
        return chr4() + chr4() + '-' + chr4() + '-' + chr4() + '-' + chr4() + '-' + chr4() + chr4() + chr4();
    }

    const EMPTY_HANDLER = (ev) => { };
    const EVENTS = {
        DID_RECEIVE_GLOBAL_SETTINGS: "didReceiveGlobalSettings",
        DID_RECEIVE_SETTINGS: "didReceiveSettings",
        SEND_TO_PLUGIN: "sendToPlugin"
    };
    /**
     * Provides a wrapper for events that can be received or sent to the Elgato Stream Deck.
     */
    class StreamDeckClient extends eventTargetShim.EventTarget {
        /**
         * Initializes a new Stream Deck client.
         * @param connection The underlying connetion.
         * @constructor
         */
        constructor(connection) {
            super();
            this.onDidReceiveGlobalSettings = EMPTY_HANDLER;
            this.onDidReceiveSettings = EMPTY_HANDLER;
            this.onSendToPropertyInspector = EMPTY_HANDLER;
            this.connection = connection;
            this.connection.connect().then((conn) => {
                conn.addEventListener("streamDeckMessage", this.parseMessage.bind(this));
            });
        }
        /**
         * Gets the action information.
         */
        get actionInfo() {
            return this.connection.actionInfo;
        }
        /**
         * Gets the registration information.
         */
        get info() {
            return this.connection.info;
        }
        /**
         * Connects to the Stream Deck, returning the connection as a promise.
         * @returns The connection as a promise.
         */
        connect() {
            return this.connection.connect();
        }
        /**
         * Sends a `get` request to the plugin, utilising SharpDeck libraries `PropertyInspectorMethod` attribute.
         * @param event The name of the event or method, i.e. URI endpoint.
         * @param parameters The optional parameters.
         * @returns A promise containing the result.
         */
        get(event, parameters) {
            return __awaiter(this, void 0, void 0, function* () {
                let request = {
                    event: event,
                    requestId: getUUID()
                };
                const client = yield this.connect();
                return yield client.get(EVENTS.SEND_TO_PLUGIN, (data) => data.payload && data.payload.event == request.event && data.payload.requestId == request.requestId, Object.assign({ data: Object.assign({}, parameters) }, request));
            });
        }
        /**
         * Triggers the `getGlobalSettings` event; upon receiving the settings, `onDidReceiveGlobalSettings` is raised.
         * @returns The global settings as part of a promise.
         */
        getGlobalSettings() {
            return this.connect()
                .then(conn => conn.get("getGlobalSettings", EVENTS.DID_RECEIVE_GLOBAL_SETTINGS));
        }
        /**
         * Triggers the `getSettings` event; upon receiving the settings, `onDidReceiveSettings` is raised.
         * @returns The settings as part of a promise.
         */
        getSettings() {
            return this.connect()
                .then(conn => conn.get("getSettings", EVENTS.DID_RECEIVE_SETTINGS));
        }
        /**
         * Logs a debug message to the logs file.
         * @param {string} msg The message to log.
         */
        logMessage(msg) {
            this.connect()
                .then(conn => conn.sendPayload("logMessage", { message: msg }));
        }
        /**
         * Tells the Stream Deck application to open an URL in the default browser.
         * @param url The URL to open.
         */
        openUrl(url) {
            this.connect()
                .then(conn => conn.sendPayload("openUrl", { url: url }));
        }
        /**
         * Saves persistent data globally.
         * @param settings The setting to save globally.
         */
        setGlobalSettings(settings) {
            this.connect()
                .then(conn => conn.sendPayload("setGlobalSettings", settings));
        }
        /**
         * Save persistent data for the action's instance.
         * @param settings The settings to save.
         */
        setSettings(settings) {
            this.connect()
                .then(conn => conn.sendPayload("setSettings", settings));
        }
        /**
         * Sends payload information to the plugin.
         * @param payload The payload information to send.
         */
        sendToPlugin(payload) {
            this.connect()
                .then(conn => conn.sendActionPayload(EVENTS.SEND_TO_PLUGIN, payload));
        }
        /**
         * Parses a message received from the Stream Deck, and raises the appropriate event
         * @param ev The event message.
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
    var streamDeckClient = new StreamDeckClient(connection);

    const DEFAULT_STATE = {
      settings: {}
    };
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
      } // merge the new value into the current state


      return Object.assign({}, state, {
        settings: { ...state.settings,
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
      const localOnChange = ownProps.onChange ? ownProps.onChange : () => {};
      return {
        onChange: ev => {
          localOnChange(ev);
          let value = {};
          objectPath__default["default"].set(value, ownProps.valuePath, ev.target.value);
          dispatch({
            type: ACTIONS.SET_ACTION_SETTING,
            value: value
          });
        }
      };
    }
    /**
     * Maps the redux store state to an elements properties.
     * @param {any} state The state of the store.
     * @param {any} ownProps The elements properties.
     * @returns {any} The state of the element.
     */


    function mapStateToProps(state, ownProps) {
      return { ...ownProps,
        value: objectPath__default["default"].get(state.settings, ownProps.valuePath)
      };
    }
    /**
     * Provides a middleware for saving the current state to the Elgato Stream Deck plugin.
     * @param {any} store The store.
     */


    const saveSettingsToStreamDeck = store => next => action => {
      next(action);

      if (action.type === ACTIONS.SET_ACTION_SETTING) {
        streamDeckClient.setSettings(store.getState().settings);
      }
    }; // the main store, initialized after the Stream Deck has connected to the plugin


    const store = redux.createStore(reduce, redux.applyMiddleware(saveSettingsToStreamDeck));
    streamDeckClient.connect().then(conn => {
      store.dispatch({
        type: ACTIONS.SET_ACTION_SETTINGS,
        value: conn.actionInfo.payload.settings
      });

      streamDeckClient.onDidReceiveSettings = function (ev) {
        store.dispatch({
          type: ACTIONS.SET_ACTION_SETTINGS,
          value: ev.payload.settings
        });
      };
    }); // an extended connect method, used to establish a connection to this store

    const connect = component => {
      return reactRedux.connect(mapStateToProps, mapDispatchToProps, null, {
        context: Context
      })(component);
    };
    const Context = /*#__PURE__*/React__default["default"].createContext();

    class CheckBox extends React__default["default"].Component {
      constructor(props) {
        super(props);
        this.state = {
          checked: typeof this.props.value === "boolean" ? this.props.value : this.props.defaultValue
        };
        this.handleClick = this.handleClick.bind(this);
      }

      handleClick(ev) {
        this.setState({
          checked: ev.target.checked
        });
        this.props.onChange({
          target: {
            value: ev.target.checked
          }
        });
      }

      render() {
        const id = this.props.id || this.props.valuePath;
        return /*#__PURE__*/React__default["default"].createElement("div", {
          type: "checkbox",
          className: "sdpi-item"
        }, /*#__PURE__*/React__default["default"].createElement("div", {
          className: "sdpi-item-label"
        }, this.props.label), /*#__PURE__*/React__default["default"].createElement("input", {
          className: "sdpi-item-value",
          id: id,
          type: "checkbox",
          checked: this.state.checked,
          onChange: this.handleClick
        }), /*#__PURE__*/React__default["default"].createElement("label", {
          htmlFor: id
        }, /*#__PURE__*/React__default["default"].createElement("span", null), this.props.checkBoxLabel));
      }

    }

    CheckBox.defaultProps = {
      checkBoxLabel: "",
      defaultValue: false,
      id: "",
      label: " ",
      value: undefined,
      valuePath: undefined
    };
    var checkbox = connect(CheckBox);

    class FolderPicker extends React__default["default"].Component {
      constructor(props) {
        super(props);
        this.handleClick = this.handleClick.bind(this);
        this.state = {
          disabled: false
        };
      }

      async handleClick() {
        this.setState({
          disabled: true
        });
        var response = await streamDeckClient.get(this.props.pluginUri);

        if (response.payload.success) {
          this.props.onChange({
            target: {
              value: response.payload.path
            }
          });
        }

        this.setState({
          disabled: false
        });
      }

      render() {
        return /*#__PURE__*/React__default["default"].createElement("div", {
          className: "sdpi-item"
        }, /*#__PURE__*/React__default["default"].createElement("div", {
          className: "sdpi-item-label"
        }, this.props.label), /*#__PURE__*/React__default["default"].createElement("div", {
          className: "sdpi-item-group file"
        }, /*#__PURE__*/React__default["default"].createElement("label", {
          className: "sdpi-folder-info"
        }, this.props.value || "\u00a0"), /*#__PURE__*/React__default["default"].createElement("button", {
          className: "sdpi-folder-button",
          disabled: this.state.disabled,
          onClick: this.handleClick
        }, "...")));
      }

    }

    FolderPicker.defaultProps = {
      id: undefined,
      label: undefined,
      onClick: undefined,
      value: undefined
    };
    var folderPicker = connect(FolderPicker);

    class PropertyInspectorWrapper extends React__default["default"].Component {
      render() {
        return /*#__PURE__*/React__default["default"].createElement(reactRedux.Provider, {
          store: store,
          context: Context
        }, /*#__PURE__*/React__default["default"].createElement("div", {
          className: "sdpi-wrapper"
        }, this.props.children));
      }

    }

    class Range extends React__default["default"].Component {
      constructor(props) {
        super(props);
        this.state = {
          value: this.props.value || this.props.defaultValue
        };
        this.handleChange = this.handleChange.bind(this);
      }

      handleChange(ev) {
        this.setState({
          value: ev.target.value
        });
        this.props.onChange({
          target: {
            value: ev.target.value
          }
        });
      }

      render() {
        const id = this.props.id || this.props.valuePath;
        return /*#__PURE__*/React__default["default"].createElement("div", {
          type: "range",
          class: "sdpi-item"
        }, /*#__PURE__*/React__default["default"].createElement("label", {
          className: "sdpi-item-label",
          htmlFor: this.props.id
        }, this.props.label), /*#__PURE__*/React__default["default"].createElement("div", {
          className: "sdpi-item-value"
        }, /*#__PURE__*/React__default["default"].createElement("input", {
          id: id,
          type: "range",
          min: this.props.min,
          max: this.props.max,
          step: this.props.step,
          value: this.state.value,
          onChange: this.handleChange
        }), /*#__PURE__*/React__default["default"].createElement("span", null, this.state.value)));
      }

    }

    Range.defaultProps = {
      defaultValue: 100,
      id: "",
      label: " ",
      value: undefined,
      valuePath: undefined,
      min: 0,
      max: 100,
      step: 5
    };
    var range = connect(Range);

    class Select extends React__default["default"].Component {
      constructor(props) {
        super(props);
        this.mapOptions = this.mapOptions.bind(this);
        this.state = props.options instanceof Array ? {
          options: [...props.options]
        } : {
          options: undefined
        };

        if (props.dataSourceUri) {
          streamDeckClient.get(props.dataSourceUri).then(r => this.setState({
            options: r.payload.data
          }));
        }
      }

      mapOptions(item) {
        return item.children && item.children instanceof Array ? /*#__PURE__*/React__default["default"].createElement("optgroup", {
          key: item.label,
          label: item.label
        }, item.children.map(this.mapOptions)) : /*#__PURE__*/React__default["default"].createElement("option", {
          key: item.value,
          value: item.value
        }, item.label);
      }

      render() {
        let options = this.state.options || this.props.options || [];
        return /*#__PURE__*/React__default["default"].createElement("div", {
          className: "sdpi-item"
        }, /*#__PURE__*/React__default["default"].createElement("label", {
          className: "sdpi-item-label",
          htmlFor: this.props.id
        }, this.props.label), /*#__PURE__*/React__default["default"].createElement("select", {
          className: "sdpi-item-value select",
          name: this.props.id,
          id: this.props.id,
          value: this.props.value || this.props.defaultValue,
          onChange: this.props.onChange
        }, /*#__PURE__*/React__default["default"].createElement("option", {
          hidden: true,
          disabled: true,
          value: ""
        }), options.map(this.mapOptions)));
      }

    }

    Select.defaultProps = {
      defaultValue: "",
      id: "",
      label: " ",
      onChange: undefined,
      options: undefined,
      value: undefined,
      valuePath: undefined
    };
    var select = connect(Select);

    class TextField extends React__default["default"].Component {
      constructor(props) {
        super(props);
        this.state = {
          value: this.props.value || this.props.defaultValue
        };
        this.handleChange = this.handleChange.bind(this);
      }

      handleChange(ev) {
        this.setState({
          value: ev.target.value
        });
        this.props.onChange({
          target: {
            value: ev.target.value
          }
        });
      }

      render() {
        const id = this.props.id || this.props.valuePath;
        return /*#__PURE__*/React__default["default"].createElement("div", {
          className: "sdpi-item"
        }, /*#__PURE__*/React__default["default"].createElement("div", {
          className: "sdpi-item-label"
        }, this.props.label), /*#__PURE__*/React__default["default"].createElement("input", {
          className: "sdpi-item-value",
          id: id,
          type: "text",
          value: this.state.value,
          onChange: this.handleChange
        }));
      }

    }

    TextField.defaultProps = {
      defaultValue: "",
      id: "",
      label: " ",
      value: undefined,
      valuePath: undefined
    };
    var textfield = connect(TextField);

    function styleInject(css, ref) {
      if ( ref === void 0 ) ref = {};
      var insertAt = ref.insertAt;

      if (!css || typeof document === 'undefined') { return; }

      var head = document.head || document.getElementsByTagName('head')[0];
      var style = document.createElement('style');
      style.type = 'text/css';

      if (insertAt === 'top') {
        if (head.firstChild) {
          head.insertBefore(style, head.firstChild);
        } else {
          head.appendChild(style);
        }
      } else {
        head.appendChild(style);
      }

      if (style.styleSheet) {
        style.styleSheet.cssText = css;
      } else {
        style.appendChild(document.createTextNode(css));
      }
    }

    var css_248z = ":root {\r\n    --sdpi-bgcolor: #2D2D2D;\r\n    --sdpi-background: #3D3D3D;\r\n    --sdpi-color: #d8d8d8;\r\n    --sdpi-bordercolor: #3a3a3a;\r\n    --sdpi-buttonbordercolor: #969696;\r\n    --sdpi-borderradius: 0px;\r\n    --sdpi-width: 224px;\r\n    --sdpi-fontweight: normal;\r\n    --sdpi-letterspacing: -0.25pt;\r\n    --sdpi-fontsize: 8pt;\r\n}\r\n\r\nhtml {\r\n    --sdpi-bgcolor: #2D2D2D;\r\n    --sdpi-background: #3D3D3D;\r\n    --sdpi-color: #d8d8d8;\r\n    --sdpi-bordercolor: #3a3a3a;\r\n    --sdpi-buttonbordercolor: #969696;\r\n    --sdpi-borderradius: 0px;\r\n    --sdpi-width: 224px;\r\n    --sdpi-fontweight: 600;\r\n    --sdpi-letterspacing: -0.25pt;\r\n    height: 100%;\r\n    width: 100%;\r\n    overflow: hidden;\r\n    touch-action: none;\r\n}\r\n\r\nhtml, body {\r\n    --sdpi-bgcolor: #2D2D2D;\r\n    --sdpi-background: #3D3D3D;\r\n    --sdpi-color: #d8d8d8;\r\n    font-family: system-ui, -apple-system, BlinkMacSystemFont, \"Segoe UI\", Roboto, Helvetica, Arial, sans-serif, \"Apple Color Emoji\", \"Segoe UI Emoji\", \"Segoe UI Symbol\";\r\n    font-size: var(--sdpi-fontsize);\r\n    background-color: var(--sdpi-bgcolor);\r\n    color: #9a9a9a;\r\n}\r\n\r\nbody {\r\n    height: 100%;\r\n    padding: 0;\r\n    overflow-x: hidden;\r\n    overflow-y: auto;\r\n    margin: 0;\r\n    -webkit-overflow-scrolling: touch;\r\n    -webkit-text-size-adjust: 100%;\r\n    -webkit-font-smoothing: antialiased;\r\n}\r\n\r\nmark {\r\n    background-color: var(--sdpi-bgcolor);\r\n    color: var(--sdpi-color);\r\n}\r\n\r\nhr, hr2 {\r\n    -webkit-margin-before: 1em;\r\n    -webkit-margin-after: 1em;\r\n    border-style: none;\r\n    background: var(--sdpi-background);\r\n    height: 1px;\r\n}\r\n\r\nhr2,\r\n.sdpi-heading {\r\n    display: flex;\r\n    flex-basis: 100%;\r\n    align-items: center;\r\n    color: inherit;\r\n    font-size: 9pt;\r\n    margin: 8px 0px;\r\n}\r\n\r\n    .sdpi-heading::before,\r\n    .sdpi-heading::after {\r\n        content: \"\";\r\n        flex-grow: 1;\r\n        background: var(--sdpi-background);\r\n        height: 1px;\r\n        font-size: 0px;\r\n        line-height: 0px;\r\n        margin: 0px 16px;\r\n    }\r\n\r\nhr2 {\r\n    height: 2px;\r\n}\r\n\r\nhr, hr2 {\r\n    margin-left: 16px;\r\n    margin-right: 16px;\r\n}\r\n\r\n.sdpi-item-value,\r\noption,\r\ninput,\r\nselect,\r\nbutton {\r\n    font-size: var(--sdpi-fontsize);\r\n    font-weight: var(--sdpi-fontweight);\r\n    letter-spacing: var(--sdpi-letterspacing);\r\n}\r\n\r\n.win .sdpi-item-value,\r\n.win option,\r\n.win input,\r\n.win select,\r\n.win button {\r\n    font-size: 11px;\r\n    font-style: normal;\r\n    letter-spacing: inherit;\r\n    font-weight: 100;\r\n}\r\n\r\n.win button {\r\n    font-size: 12px;\r\n}\r\n\r\n::-webkit-progress-value,\r\nmeter::-webkit-meter-optimum-value {\r\n    border-radius: 2px;\r\n    /* background: linear-gradient(#ccf, #99f 20%, #77f 45%, #77f 55%, #cdf); */\r\n}\r\n\r\n::-webkit-progress-bar,\r\nmeter::-webkit-meter-bar {\r\n    border-radius: 3px;\r\n    background: var(--sdpi-background);\r\n}\r\n\r\n    ::-webkit-progress-bar:active,\r\n    meter::-webkit-meter-bar:active {\r\n        border-radius: 3px;\r\n        background: #222222;\r\n    }\r\n\r\n::-webkit-progress-value:active,\r\nmeter::-webkit-meter-optimum-value:active {\r\n    background: #99f;\r\n}\r\n\r\nprogress,\r\nprogress.sdpi-item-value {\r\n    min-height: 5px !important;\r\n    height: 5px;\r\n    background-color: #303030;\r\n}\r\n\r\nprogress {\r\n    margin-top: 8px !important;\r\n    margin-bottom: 8px !important;\r\n}\r\n\r\n    .full progress,\r\n    progress.full {\r\n        margin-top: 3px !important;\r\n    }\r\n\r\n::-webkit-progress-inner-element {\r\n    background-color: transparent;\r\n}\r\n\r\n.sdpi-item[type=\"progress\"] {\r\n    margin-top: 4px !important;\r\n    margin-bottom: 12px;\r\n    min-height: 15px;\r\n}\r\n\r\n.sdpi-item-child.full:last-child {\r\n    margin-bottom: 4px;\r\n}\r\n\r\n.tabs {\r\n    /**\r\n   * Setting display to flex makes this container lay\r\n   * out its children using flexbox, the exact same\r\n   * as in the above \"Stepper input\" example.\r\n   */\r\n    display: flex;\r\n    border-bottom: 1px solid #D7DBDD;\r\n}\r\n\r\n.tab {\r\n    cursor: pointer;\r\n    padding: 5px 30px;\r\n    color: #16a2d7;\r\n    font-size: 9pt;\r\n    border-bottom: 2px solid transparent;\r\n}\r\n\r\n    .tab.is-tab-selected {\r\n        border-bottom-color: #4ebbe4;\r\n    }\r\n\r\nselect {\r\n    -webkit-appearance: none;\r\n    -moz-appearance: none;\r\n    -o-appearance: none;\r\n    appearance: none;\r\n    background: url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='6' viewBox='0 0 12 6'%3E%3Cpolygon fill='%238E8E92' fill-rule='evenodd' points='5 4 9 0 10 1 5 6 0 1 1 0'/%3E%3C/svg%3E%0A\") no-repeat 97% center;\r\n}\r\n\r\nlabel.sdpi-file-label,\r\ninput[type=\"button\"],\r\ninput[type=\"submit\"],\r\ninput[type=\"reset\"],\r\ninput[type=\"file\"],\r\ninput[type=file]::-webkit-file-upload-button,\r\nbutton,\r\nselect {\r\n    color: var(--sdpi-color);\r\n    border: 1pt solid #303030;\r\n    font-size: 8pt;\r\n    background-color: var(--sdpi-background);\r\n    border-radius: var(--sdpi-borderradius);\r\n}\r\n\r\nlabel.sdpi-file-label,\r\ninput[type=\"button\"],\r\ninput[type=\"submit\"],\r\ninput[type=\"reset\"],\r\ninput[type=\"file\"],\r\ninput[type=file]::-webkit-file-upload-button,\r\nbutton {\r\n    border: 1pt solid var(--sdpi-buttonbordercolor);\r\n    border-radius: var(--sdpi-borderradius);\r\n    border-color: var(--sdpi-buttonbordercolor);\r\n    min-height: 23px !important;\r\n    height: 23px !important;\r\n    margin-right: 8px;\r\n}\r\n\r\ninput[type=number]::-webkit-inner-spin-button,\r\ninput[type=number]::-webkit-outer-spin-button {\r\n    -webkit-appearance: none;\r\n    margin: 0;\r\n}\r\n\r\ninput[type=\"file\"] {\r\n    border-radius: var(--sdpi-borderradius);\r\n    max-width: 220px;\r\n}\r\n\r\noption {\r\n    height: 1.5em;\r\n    padding: 4px;\r\n}\r\n\r\n/* SDPI */\r\n\r\n.sdpi-wrapper {\r\n    overflow-x: hidden;\r\n    height: 100%;\r\n}\r\n\r\n.sdpi-item {\r\n    display: flex;\r\n    flex-direction: row;\r\n    min-height: 32px;\r\n    align-items: center;\r\n    margin-top: 2px;\r\n    max-width: 344px;\r\n    -webkit-user-drag: none;\r\n}\r\n\r\n    .sdpi-item:first-child {\r\n        margin-top: -1px;\r\n    }\r\n\r\n    .sdpi-item:last-child {\r\n        margin-bottom: 0px;\r\n    }\r\n\r\n    .sdpi-item > *:not(.sdpi-item-label):not(meter):not(details):not(canvas) {\r\n        min-height: 26px;\r\n        padding: 0px 4px 0px 4px;\r\n    }\r\n\r\n    .sdpi-item > *:not(.sdpi-item-label.empty):not(meter) {\r\n        min-height: 26px;\r\n        padding: 0px 4px 0px 4px;\r\n    }\r\n\r\n.sdpi-item-group {\r\n    padding: 0 !important;\r\n}\r\n\r\nmeter.sdpi-item-value {\r\n    margin-left: 6px;\r\n}\r\n\r\n.sdpi-item[type=\"group\"] {\r\n    display: block;\r\n    margin-top: 12px;\r\n    margin-bottom: 12px;\r\n    /* border: 1px solid white; */\r\n    flex-direction: unset;\r\n    text-align: left;\r\n}\r\n\r\n    .sdpi-item[type=\"group\"] > .sdpi-item-label,\r\n    .sdpi-item[type=\"group\"].sdpi-item-label {\r\n        width: 96%;\r\n        text-align: left;\r\n        font-weight: 700;\r\n        margin-bottom: 4px;\r\n        padding-left: 4px;\r\n    }\r\n\r\ndl,\r\nul,\r\nol {\r\n    -webkit-margin-before: 0px;\r\n    -webkit-margin-after: 4px;\r\n    -webkit-padding-start: 1em;\r\n    max-height: 90px;\r\n    overflow-y: scroll;\r\n    cursor: pointer;\r\n    user-select: none;\r\n}\r\n\r\n    table.sdpi-item-value,\r\n    dl.sdpi-item-value,\r\n    ul.sdpi-item-value,\r\n    ol.sdpi-item-value {\r\n        -webkit-margin-before: 4px;\r\n        -webkit-margin-after: 8px;\r\n        -webkit-padding-start: 1em;\r\n        width: var(--sdpi-width);\r\n        text-align: center;\r\n    }\r\n\r\ntable > caption {\r\n    margin: 2px;\r\n}\r\n\r\n.list,\r\n.sdpi-item[type=\"list\"] {\r\n    align-items: baseline;\r\n}\r\n\r\n.sdpi-item-label {\r\n    text-align: right;\r\n    flex: none;\r\n    width: 92px;\r\n    padding-right: 4px;\r\n    font-weight: var(--sdpi-fontweight);\r\n    -webkit-user-select: none;\r\n}\r\n\r\n    .win .sdpi-item-label,\r\n    .sdpi-item-label > small {\r\n        font-weight: normal;\r\n    }\r\n\r\n    .sdpi-item-label:after {\r\n        content: \":\";\r\n        margin-left: 1px;\r\n    }\r\n\r\n    .sdpi-item-label.empty:after {\r\n        content: \"\";\r\n    }\r\n\r\n.sdpi-test,\r\n.sdpi-item-value {\r\n    flex: 1 0 0;\r\n    /* flex-grow: 1;\r\n  flex-shrink: 0; */\r\n    margin-right: 14px;\r\n    margin-left: 4px;\r\n    justify-content: space-evenly;\r\n}\r\n\r\ncanvas.sdpi-item-value {\r\n    max-width: 144px;\r\n    max-height: 144px;\r\n    width: 144px;\r\n    height: 144px;\r\n    margin: 0 auto;\r\n    cursor: pointer;\r\n}\r\n\r\ninput.sdpi-item-value {\r\n    margin-left: 7px;\r\n}\r\n\r\n.sdpi-item-value button,\r\nbutton.sdpi-item-value {\r\n    margin-left: 6px;\r\n    margin-right: 14px;\r\n}\r\n\r\n.sdpi-item-value.range {\r\n    margin-left: 0px;\r\n}\r\n\r\ntable,\r\ndl.sdpi-item-value,\r\nul.sdpi-item-value,\r\nol.sdpi-item-value,\r\n.sdpi-item-value > dl,\r\n.sdpi-item-value > ul,\r\n.sdpi-item-value > ol {\r\n    list-style-type: none;\r\n    list-style-position: outside;\r\n    margin-left: -4px;\r\n    margin-right: -4px;\r\n    padding: 4px;\r\n    border: 1px solid var(--sdpi-bordercolor);\r\n}\r\n\r\ndl.sdpi-item-value,\r\nul.sdpi-item-value,\r\nol.sdpi-item-value,\r\n.sdpi-item-value > ol {\r\n    list-style-type: none;\r\n    list-style-position: inside;\r\n    margin-left: 5px;\r\n    margin-right: 12px;\r\n    padding: 4px !important;\r\n    display: flex;\r\n    flex-direction: column;\r\n}\r\n\r\n.two-items li {\r\n    display: flex;\r\n}\r\n\r\n    .two-items li > *:first-child {\r\n        flex: 0 0 50%;\r\n        text-align: left;\r\n    }\r\n\r\n.two-items.thirtyseventy li > *:first-child {\r\n    flex: 0 0 30%;\r\n}\r\n\r\nol.sdpi-item-value,\r\n.sdpi-item-value > ol[listtype=\"none\"] {\r\n    list-style-type: none;\r\n}\r\n\r\n    ol.sdpi-item-value[type=\"decimal\"],\r\n    .sdpi-item-value > ol[type=\"decimal\"] {\r\n        list-style-type: decimal;\r\n    }\r\n\r\n    ol.sdpi-item-value[type=\"decimal-leading-zero\"],\r\n    .sdpi-item-value > ol[type=\"decimal-leading-zero\"] {\r\n        list-style-type: decimal-leading-zero;\r\n    }\r\n\r\n    ol.sdpi-item-value[type=\"lower-alpha\"],\r\n    .sdpi-item-value > ol[type=\"lower-alpha\"] {\r\n        list-style-type: lower-alpha;\r\n    }\r\n\r\n    ol.sdpi-item-value[type=\"upper-alpha\"],\r\n    .sdpi-item-value > ol[type=\"upper-alpha\"] {\r\n        list-style-type: upper-alpha;\r\n    }\r\n\r\n    ol.sdpi-item-value[type=\"upper-roman\"],\r\n    .sdpi-item-value > ol[type=\"upper-roman\"] {\r\n        list-style-type: upper-roman;\r\n    }\r\n\r\n    ol.sdpi-item-value[type=\"lower-roman\"],\r\n    .sdpi-item-value > ol[type=\"lower-roman\"] {\r\n        list-style-type: upper-roman;\r\n    }\r\n\r\ntr:nth-child(even),\r\n.sdpi-item-value > ul > li:nth-child(even),\r\n.sdpi-item-value > ol > li:nth-child(even),\r\nli:nth-child(even) {\r\n    background-color: rgba(0,0,0,.2)\r\n}\r\n\r\ntd:hover,\r\n.sdpi-item-value > ul > li:hover:nth-child(even),\r\n.sdpi-item-value > ol > li:hover:nth-child(even),\r\nli:hover:nth-child(even),\r\nli:hover {\r\n    background-color: rgba(255,255,255,.1);\r\n}\r\n\r\ntd.selected,\r\ntd.selected:hover,\r\nli.selected:hover,\r\nli.selected {\r\n    color: white;\r\n    background-color: #77f;\r\n}\r\n\r\ntr {\r\n    border: 1px solid var(--sdpi-bordercolor);\r\n}\r\n\r\ntd {\r\n    border-right: 1px solid var(--sdpi-bordercolor);\r\n    -webkit-user-select: none;\r\n}\r\n\r\n    tr:last-child,\r\n    td:last-child {\r\n        border: none;\r\n    }\r\n\r\n.sdpi-item-value.select,\r\n.sdpi-item-value > select {\r\n    margin-right: 13px;\r\n    margin-left: 6px;\r\n}\r\n\r\n.sdpi-item-child,\r\n.sdpi-item-group > .sdpi-item > input[type=\"color\"] {\r\n    margin-top: 0.4em;\r\n    margin-right: 4px;\r\n}\r\n\r\n    .full,\r\n    .full *,\r\n    .sdpi-item-value.full,\r\n    .sdpi-item-child > full > *,\r\n    .sdpi-item-child.full,\r\n    .sdpi-item-child.full > *,\r\n    .full > .sdpi-item-child,\r\n    .full > .sdpi-item-child > * {\r\n        display: flex;\r\n        flex: 1 1 0;\r\n        margin-bottom: 4px;\r\n        margin-left: 0px;\r\n        width: 100%;\r\n        justify-content: space-evenly;\r\n    }\r\n\r\n.sdpi-item-group > .sdpi-item > input[type=\"color\"] {\r\n    margin-top: 0px;\r\n}\r\n\r\n::-webkit-calendar-picker-indicator:focus,\r\ninput[type=file]::-webkit-file-upload-button:focus,\r\nbutton:focus,\r\ntextarea:focus,\r\ninput:focus,\r\nselect:focus,\r\noption:focus,\r\ndetails:focus,\r\nsummary:focus,\r\n.custom-select select {\r\n    outline: none;\r\n}\r\n\r\nsummary {\r\n    cursor: default;\r\n    -webkit-user-select: none;\r\n}\r\n\r\n    .pointer,\r\n    summary .pointer {\r\n        cursor: pointer;\r\n    }\r\n\r\ndetails * {\r\n    font-size: var(--sdpi-fontsize);\r\n    font-weight: var(--sdpi-fontweight);\r\n}\r\n\r\ndetails.message {\r\n    padding: 4px 18px 4px 12px;\r\n}\r\n\r\n    details.message summary {\r\n        font-size: var(--sdpi-fontsize);\r\n        font-weight: var(--sdpi-fontweight);\r\n        min-height: 18px;\r\n    }\r\n\r\n    details.message:first-child {\r\n        margin-top: 4px;\r\n        margin-left: 0;\r\n        padding-left: 102px;\r\n    }\r\n\r\n    details.message h1 {\r\n        text-align: left;\r\n    }\r\n\r\n.message > summary::-webkit-details-marker {\r\n    display: none;\r\n}\r\n\r\n.info20,\r\n.question,\r\n.caution,\r\n.info {\r\n    background-repeat: no-repeat;\r\n    background-position: 72px center;\r\n}\r\n\r\n.info20 {\r\n    background-image: url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='20' height='20' viewBox='0 0 20 20'%3E%3Cpath fill='%23999' d='M10,20 C4.4771525,20 0,15.5228475 0,10 C0,4.4771525 4.4771525,0 10,0 C15.5228475,0 20,4.4771525 20,10 C20,15.5228475 15.5228475,20 10,20 Z M10,8 C8.8954305,8 8,8.84275812 8,9.88235294 L8,16.1176471 C8,17.1572419 8.8954305,18 10,18 C11.1045695,18 12,17.1572419 12,16.1176471 L12,9.88235294 C12,8.84275812 11.1045695,8 10,8 Z M10,3 C8.8954305,3 8,3.88165465 8,4.96923077 L8,5.03076923 C8,6.11834535 8.8954305,7 10,7 C11.1045695,7 12,6.11834535 12,5.03076923 L12,4.96923077 C12,3.88165465 11.1045695,3 10,3 Z'/%3E%3C/svg%3E%0A\");\r\n}\r\n\r\n.info {\r\n    background-image: url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='20' height='20' viewBox='0 0 20 20'%3E%3Cpath fill='%23999' d='M10,18 C5.581722,18 2,14.418278 2,10 C2,5.581722 5.581722,2 10,2 C14.418278,2 18,5.581722 18,10 C18,14.418278 14.418278,18 10,18 Z M10,8 C9.44771525,8 9,8.42137906 9,8.94117647 L9,14.0588235 C9,14.5786209 9.44771525,15 10,15 C10.5522847,15 11,14.5786209 11,14.0588235 L11,8.94117647 C11,8.42137906 10.5522847,8 10,8 Z M10,5 C9.44771525,5 9,5.44082732 9,5.98461538 L9,6.01538462 C9,6.55917268 9.44771525,7 10,7 C10.5522847,7 11,6.55917268 11,6.01538462 L11,5.98461538 C11,5.44082732 10.5522847,5 10,5 Z'/%3E%3C/svg%3E%0A\");\r\n}\r\n\r\n.info2 {\r\n    background-image: url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='15' height='15' viewBox='0 0 15 15'%3E%3Cpath fill='%23999' d='M7.5,15 C3.35786438,15 0,11.6421356 0,7.5 C0,3.35786438 3.35786438,0 7.5,0 C11.6421356,0 15,3.35786438 15,7.5 C15,11.6421356 11.6421356,15 7.5,15 Z M7.5,2 C6.67157287,2 6,2.66124098 6,3.47692307 L6,3.52307693 C6,4.33875902 6.67157287,5 7.5,5 C8.32842705,5 9,4.33875902 9,3.52307693 L9,3.47692307 C9,2.66124098 8.32842705,2 7.5,2 Z M5,6 L5,7.02155172 L6,7 L6,12 L5,12.0076778 L5,13 L10,13 L10,12 L9,12.0076778 L9,6 L5,6 Z'/%3E%3C/svg%3E%0A\");\r\n}\r\n\r\n.sdpi-more-info {\r\n    background-image: linear-gradient(to right, #00000000 0%,#00000040 80%), url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 16 16'%3E%3Cpolygon fill='%23999' points='4 7 8 7 8 5 12 8 8 11 8 9 4 9'/%3E%3C/svg%3E%0A\");\r\n}\r\n\r\n.caution {\r\n    background-image: url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='20' height='20' viewBox='0 0 20 20'%3E%3Cpath fill='%23999' fill-rule='evenodd' d='M9.03952676,0.746646542 C9.57068894,-0.245797319 10.4285735,-0.25196227 10.9630352,0.746646542 L19.7705903,17.2030214 C20.3017525,18.1954653 19.8777595,19 18.8371387,19 L1.16542323,19 C0.118729947,19 -0.302490098,18.2016302 0.231971607,17.2030214 L9.03952676,0.746646542 Z M10,2.25584053 L1.9601405,17.3478261 L18.04099,17.3478261 L10,2.25584053 Z M10,5.9375 C10.531043,5.9375 10.9615385,6.37373537 10.9615385,6.91185897 L10.9615385,11.6923077 C10.9615385,12.2304313 10.531043,12.6666667 10,12.6666667 C9.46895697,12.6666667 9.03846154,12.2304313 9.03846154,11.6923077 L9.03846154,6.91185897 C9.03846154,6.37373537 9.46895697,5.9375 10,5.9375 Z M10,13.4583333 C10.6372516,13.4583333 11.1538462,13.9818158 11.1538462,14.6275641 L11.1538462,14.6641026 C11.1538462,15.3098509 10.6372516,15.8333333 10,15.8333333 C9.36274837,15.8333333 8.84615385,15.3098509 8.84615385,14.6641026 L8.84615385,14.6275641 C8.84615385,13.9818158 9.36274837,13.4583333 10,13.4583333 Z'/%3E%3C/svg%3E%0A\");\r\n}\r\n\r\n.question {\r\n    background-image: url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='20' height='20' viewBox='0 0 20 20'%3E%3Cpath fill='%23999' d='M10,18 C5.581722,18 2,14.418278 2,10 C2,5.581722 5.581722,2 10,2 C14.418278,2 18,5.581722 18,10 C18,14.418278 14.418278,18 10,18 Z M6.77783203,7.65332031 C6.77783203,7.84798274 6.85929281,8.02888914 7.0222168,8.19604492 C7.18514079,8.36320071 7.38508996,8.44677734 7.62207031,8.44677734 C8.02409055,8.44677734 8.29703704,8.20768468 8.44091797,7.72949219 C8.59326248,7.27245865 8.77945854,6.92651485 8.99951172,6.69165039 C9.2195649,6.45678594 9.56233491,6.33935547 10.027832,6.33935547 C10.4256205,6.33935547 10.7006836,6.37695313 11.0021973,6.68847656 C11.652832,7.53271484 10.942627,8.472229 10.3750916,9.1321106 C9.80755615,9.79199219 8.29492188,11.9897461 10.027832,12.1347656 C10.4498423,12.1700818 10.7027991,11.9147157 10.7832031,11.4746094 C11.0021973,9.59857178 13.1254883,8.82415771 13.1254883,7.53271484 C13.1254883,7.07568131 12.9974785,6.65250846 12.7414551,6.26318359 C12.4854317,5.87385873 12.1225609,5.56600048 11.652832,5.33959961 C11.1831031,5.11319874 10.6414419,5 10.027832,5 C9.36767248,5 8.79004154,5.13541531 8.29492187,5.40625 C7.79980221,5.67708469 7.42317837,6.01879677 7.16503906,6.43139648 C6.90689975,6.8439962 6.77783203,7.25130007 6.77783203,7.65332031 Z M10.0099668,15 C10.2713191,15 10.5016601,14.9108147 10.7009967,14.7324415 C10.9003332,14.5540682 11,14.3088087 11,13.9966555 C11,13.7157177 10.9047629,13.4793767 10.7142857,13.2876254 C10.5238086,13.0958742 10.2890379,13 10.0099668,13 C9.72646591,13 9.48726565,13.0958742 9.2923588,13.2876254 C9.09745196,13.4793767 9,13.7157177 9,13.9966555 C9,14.313268 9.10077419,14.5596424 9.30232558,14.735786 C9.50387698,14.9119295 9.73975502,15 10.0099668,15 Z'/%3E%3C/svg%3E%0A\");\r\n}\r\n\r\n.sdpi-more-info {\r\n    position: fixed;\r\n    left: 0px;\r\n    right: 0px;\r\n    bottom: 0px;\r\n    min-height: 16px;\r\n    padding-right: 16px;\r\n    text-align: right;\r\n    -webkit-touch-callout: none;\r\n    cursor: pointer;\r\n    user-select: none;\r\n    background-position: right center;\r\n    background-repeat: no-repeat;\r\n    border-radius: var(--sdpi-borderradius);\r\n    text-decoration: none;\r\n    color: var(--sdpi-color);\r\n}\r\n\r\n.sdpi-bottom-bar {\r\n    display: flex;\r\n    align-self: right;\r\n    margin-left: auto;\r\n    position: fixed;\r\n    right: 17px;\r\n    bottom: 0px;\r\n    user-select: none;\r\n}\r\n\r\n    .sdpi-bottom-bar.right {\r\n        right: 0px;\r\n    }\r\n\r\n    .sdpi-bottom-bar button {\r\n        min-height: 20px !important;\r\n        height: 20px !important;\r\n    }\r\n\r\n.sdpi-more-info-button {\r\n    display: flex;\r\n    align-self: right;\r\n    margin-left: auto;\r\n    position: fixed;\r\n    right: 17px;\r\n    bottom: 0px;\r\n    user-select: none;\r\n}\r\n\r\ndetails a {\r\n    background-position: right !important;\r\n    min-height: 24px;\r\n    display: inline-block;\r\n    line-height: 24px;\r\n    padding-right: 28px;\r\n}\r\n\r\ninput:not([type=\"range\"]),\r\ntextarea {\r\n    -webkit-appearance: none;\r\n    background: var(--sdpi-background);\r\n    color: var(--sdpi-color);\r\n    font-weight: normal;\r\n    font-size: 9pt;\r\n    border: none;\r\n    margin-top: 2px;\r\n    margin-bottom: 2px;\r\n    min-width: 219px;\r\n}\r\n\r\n    textarea + label {\r\n        display: flex;\r\n        justify-content: flex-end\r\n    }\r\n\r\ninput[type=\"radio\"],\r\ninput[type=\"checkbox\"] {\r\n    display: none;\r\n}\r\n\r\n    input[type=\"radio\"] + label,\r\n    input[type=\"checkbox\"] + label {\r\n        font-size: 9pt;\r\n        color: var(--sdpi-color);\r\n        font-weight: normal;\r\n        margin-right: 8px;\r\n        -webkit-user-select: none;\r\n    }\r\n\r\n        input[type=\"radio\"] + label:after,\r\n        input[type=\"checkbox\"] + label:after {\r\n            content: \" \" !important;\r\n        }\r\n\r\n.sdpi-item[type=\"radio\"] > .sdpi-item-value,\r\n.sdpi-item[type=\"checkbox\"] > .sdpi-item-value {\r\n    padding-top: 2px;\r\n}\r\n\r\n    .sdpi-item[type=\"checkbox\"] > .sdpi-item-value > * {\r\n        margin-top: 4px;\r\n    }\r\n\r\n.sdpi-item[type=\"checkbox\"] .sdpi-item-child,\r\n.sdpi-item[type=\"radio\"] .sdpi-item-child {\r\n    display: inline-block;\r\n}\r\n\r\n.sdpi-item[type=\"range\"] .sdpi-item-value,\r\n.sdpi-item[type=\"meter\"] .sdpi-item-child,\r\n.sdpi-item[type=\"progress\"] .sdpi-item-child {\r\n    display: flex;\r\n}\r\n\r\n.sdpi-item[type=\"range\"] .sdpi-item-value {\r\n    min-height: 26px;\r\n}\r\n\r\n    .sdpi-item[type=\"range\"] .sdpi-item-value span,\r\n    .sdpi-item[type=\"meter\"] .sdpi-item-child span,\r\n    .sdpi-item[type=\"progress\"] .sdpi-item-child span {\r\n        margin-top: -2px;\r\n        min-width: 8px;\r\n        text-align: right;\r\n        user-select: none;\r\n        cursor: pointer;\r\n        -webkit-user-select: none;\r\n        user-select: none;\r\n    }\r\n\r\n    .sdpi-item[type=\"range\"] .sdpi-item-value span {\r\n        margin-top: 7px;\r\n        text-align: right;\r\n    }\r\n\r\nspan + input[type=\"range\"] {\r\n    display: flex;\r\n    max-width: 168px;\r\n}\r\n\r\n.sdpi-item[type=\"range\"] .sdpi-item-value span:first-child,\r\n.sdpi-item[type=\"meter\"] .sdpi-item-child span:first-child,\r\n.sdpi-item[type=\"progress\"] .sdpi-item-child span:first-child {\r\n    margin-right: 4px;\r\n}\r\n\r\n.sdpi-item[type=\"range\"] .sdpi-item-value span:last-child,\r\n.sdpi-item[type=\"meter\"] .sdpi-item-child span:last-child,\r\n.sdpi-item[type=\"progress\"] .sdpi-item-child span:last-child {\r\n    width: 30px;\r\n    text-align: right;\r\n    float: right;\r\n}\r\n\r\n.reverse {\r\n    transform: rotate(180deg);\r\n}\r\n\r\n.sdpi-item[type=\"meter\"] .sdpi-item-child meter + span:last-child {\r\n    margin-left: -10px;\r\n}\r\n\r\n.sdpi-item[type=\"progress\"] .sdpi-item-child meter + span:last-child {\r\n    margin-left: -14px;\r\n}\r\n\r\n.sdpi-item[type=\"radio\"] > .sdpi-item-value > * {\r\n    margin-top: 2px;\r\n}\r\n\r\ndetails {\r\n    padding: 8px 18px 8px 12px;\r\n    min-width: 86px;\r\n}\r\n\r\n    details > h4 {\r\n        border-bottom: 1px solid var(--sdpi-bordercolor);\r\n    }\r\n\r\nlegend {\r\n    display: none;\r\n}\r\n\r\n.sdpi-item-value > textarea {\r\n    padding: 0px;\r\n    width: 219px;\r\n    margin-left: 1px;\r\n    margin-top: 3px;\r\n    padding: 4px;\r\n}\r\n\r\ninput[type=\"radio\"] + label span,\r\ninput[type=\"checkbox\"] + label span {\r\n    display: inline-block;\r\n    width: 16px;\r\n    height: 16px;\r\n    margin: 2px 4px 2px 0;\r\n    border-radius: 3px;\r\n    vertical-align: middle;\r\n    background: var(--sdpi-background);\r\n    cursor: pointer;\r\n    border: 1px solid rgb(0,0,0,.2);\r\n}\r\n\r\ninput[type=\"radio\"] + label span {\r\n    border-radius: 100%;\r\n}\r\n\r\ninput[type=\"radio\"]:checked + label span,\r\ninput[type=\"checkbox\"]:checked + label span {\r\n    background-color: #77f;\r\n    background-image: url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='10' viewBox='0 0 12 10'%3E%3Cpolygon fill='%23FFF' points='7.2 7.5 7.2 -1.3 8.7 -1.3 8.6 9.1 2.7 8.7 2.7 7.2' transform='rotate(37 5.718 3.896)'/%3E%3C/svg%3E%0A\");\r\n    background-repeat: no-repeat;\r\n    background-position: center center;\r\n    border: 1px solid rgb(0,0,0,.4);\r\n}\r\n\r\ninput[type=\"radio\"]:active:checked + label span,\r\ninput[type=\"radio\"]:active + label span,\r\ninput[type=\"checkbox\"]:active:checked + label span,\r\ninput[type=\"checkbox\"]:active + label span {\r\n    background-color: #303030;\r\n}\r\n\r\ninput[type=\"radio\"]:checked + label span {\r\n    background-image: url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='6' height='6' viewBox='0 0 6 6'%3E%3Ccircle cx='3' cy='3' r='3' fill='%23FFF'/%3E%3C/svg%3E%0A\");\r\n}\r\n\r\ninput[type=\"range\"] {\r\n    width: var(--sdpi-width);\r\n    height: 30px;\r\n    overflow: hidden;\r\n    cursor: pointer;\r\n    background: transparent !important;\r\n}\r\n\r\n.sdpi-item > input[type=\"range\"] {\r\n    margin-left: 2px;\r\n    max-width: var(--sdpi-width);\r\n    width: var(--sdpi-width);\r\n    padding: 0px;\r\n    margin-top: 2px;\r\n}\r\n\r\n/*\r\ninput[type=\"range\"],\r\ninput[type=\"range\"]::-webkit-slider-runnable-track,\r\ninput[type=\"range\"]::-webkit-slider-thumb {\r\n  -webkit-appearance: none;\r\n}\r\n*/\r\n\r\ninput[type=\"range\"]::-webkit-slider-runnable-track {\r\n    height: 5px;\r\n    background: #979797;\r\n    border-radius: 3px;\r\n    padding: 0px !important;\r\n    border: 1px solid var(--sdpi-background);\r\n}\r\n\r\ninput[type=\"range\"]::-webkit-slider-thumb {\r\n    position: relative;\r\n    -webkit-appearance: none;\r\n    background-color: var(--sdpi-color);\r\n    width: 12px;\r\n    height: 12px;\r\n    border-radius: 20px;\r\n    margin-top: -5px;\r\n    border: none;\r\n}\r\n\r\ninput[type=\"range\" i] {\r\n    margin: 0;\r\n}\r\n\r\ninput[type=\"range\"]::-webkit-slider-thumb::before {\r\n    position: absolute;\r\n    content: \"\";\r\n    height: 5px; /* equal to height of runnable track or 1 less */\r\n    width: 500px; /* make this bigger than the widest range input element */\r\n    left: -502px; /* this should be -2px - width */\r\n    top: 8px; /* don't change this */\r\n    background: #77f;\r\n}\r\n\r\ninput[type=\"color\"] {\r\n    min-width: 32px;\r\n    min-height: 32px;\r\n    width: 32px;\r\n    height: 32px;\r\n    padding: 0;\r\n    background-color: var(--sdpi-bgcolor);\r\n    flex: none;\r\n}\r\n\r\n::-webkit-color-swatch {\r\n    min-width: 24px;\r\n}\r\n\r\ntextarea {\r\n    height: 3em;\r\n    word-break: break-word;\r\n    line-height: 1.5em;\r\n}\r\n\r\n.textarea {\r\n    padding: 0px !important;\r\n}\r\n\r\ntextarea {\r\n    width: 219px; /*98%;*/\r\n    height: 96%;\r\n    min-height: 6em;\r\n    resize: none;\r\n    border-radius: var(--sdpi-borderradius);\r\n}\r\n\r\n/* CAROUSEL */\r\n\r\n.sdpi-item[type=\"carousel\"] {\r\n}\r\n\r\n.sdpi-item.card-carousel-wrapper,\r\n.sdpi-item > .card-carousel-wrapper {\r\n    padding: 0;\r\n}\r\n\r\n.card-carousel-wrapper {\r\n    display: flex;\r\n    align-items: center;\r\n    justify-content: center;\r\n    margin: 12px auto;\r\n    color: #666a73;\r\n}\r\n\r\n.card-carousel {\r\n    display: flex;\r\n    justify-content: center;\r\n    width: 278px;\r\n}\r\n\r\n.card-carousel--overflow-container {\r\n    overflow: hidden;\r\n}\r\n\r\n.card-carousel--nav__left,\r\n.card-carousel--nav__right {\r\n    /* display: inline-block; */\r\n    width: 12px;\r\n    height: 12px;\r\n    border-top: 2px solid #42b883;\r\n    border-right: 2px solid #42b883;\r\n    cursor: pointer;\r\n    margin: 0 4px;\r\n    transition: transform 150ms linear;\r\n}\r\n\r\n    .card-carousel--nav__left[disabled],\r\n    .card-carousel--nav__right[disabled] {\r\n        opacity: 0.2;\r\n        border-color: black;\r\n    }\r\n\r\n.card-carousel--nav__left {\r\n    transform: rotate(-135deg);\r\n}\r\n\r\n    .card-carousel--nav__left:active {\r\n        transform: rotate(-135deg) scale(0.85);\r\n    }\r\n\r\n.card-carousel--nav__right {\r\n    transform: rotate(45deg);\r\n}\r\n\r\n    .card-carousel--nav__right:active {\r\n        transform: rotate(45deg) scale(0.85);\r\n    }\r\n\r\n.card-carousel-cards {\r\n    display: flex;\r\n    transition: transform 150ms ease-out;\r\n    transform: translatex(0px);\r\n}\r\n\r\n    .card-carousel-cards .card-carousel--card {\r\n        margin: 0 5px;\r\n        cursor: pointer;\r\n        /* box-shadow: 0 4px 15px 0 rgba(40, 44, 53, 0.06), 0 2px 2px 0 rgba(40, 44, 53, 0.08); */\r\n        background-color: #fff;\r\n        border-radius: 4px;\r\n        z-index: 3;\r\n    }\r\n\r\n.xxcard-carousel-cards .card-carousel--card:first-child {\r\n    margin-left: 0;\r\n}\r\n\r\n.xxcard-carousel-cards .card-carousel--card:last-child {\r\n    margin-right: 0;\r\n}\r\n\r\n.card-carousel-cards .card-carousel--card img {\r\n    vertical-align: bottom;\r\n    border-top-left-radius: 4px;\r\n    border-top-right-radius: 4px;\r\n    transition: opacity 150ms linear;\r\n    width: 60px;\r\n}\r\n\r\n    .card-carousel-cards .card-carousel--card img:hover {\r\n        opacity: 0.5;\r\n    }\r\n\r\n.card-carousel-cards .card-carousel--card--footer {\r\n    border-top: 0;\r\n    max-width: 80px;\r\n    overflow: hidden;\r\n    display: flex;\r\n    height: 100%;\r\n    flex-direction: column;\r\n}\r\n\r\n    .card-carousel-cards .card-carousel--card--footer p {\r\n        padding: 3px 0;\r\n        margin: 0;\r\n        margin-bottom: 2px;\r\n        font-size: 15px;\r\n        font-weight: 500;\r\n        color: #2c3e50;\r\n    }\r\n\r\n        .card-carousel-cards .card-carousel--card--footer p:nth-of-type(2) {\r\n            font-size: 12px;\r\n            font-weight: 300;\r\n            padding: 6px;\r\n            color: #666a73;\r\n        }\r\n\r\nh1 {\r\n    font-size: 1.3em;\r\n    font-weight: 500;\r\n    text-align: center;\r\n    margin-bottom: 12px;\r\n}\r\n\r\n::-webkit-datetime-edit {\r\n    font-family: system-ui, -apple-system, BlinkMacSystemFont, \"Segoe UI\", Roboto, Helvetica, Arial, sans-serif, \"Apple Color Emoji\", \"Segoe UI Emoji\", \"Segoe UI Symbol\";\r\n    background: url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 16 16'%3E%3Cg fill='%239C9C9C'%3E%3Cpath d='M15,15 L1.77635684e-15,15 L1.77635684e-15,1 L15,1 L15,15 Z M5,7 L5,8 L6,8 L6,7 L5,7 Z M3,7 L3,8 L4,8 L4,7 L3,7 Z M7,7 L7,8 L8,8 L8,7 L7,7 Z M9,7 L9,8 L10,8 L10,7 L9,7 Z M11,7 L11,8 L12,8 L12,7 L11,7 Z M3,9 L3,10 L4,10 L4,9 L3,9 Z M5,9 L5,10 L6,10 L6,9 L5,9 Z M7,9 L7,10 L8,10 L8,9 L7,9 Z M9,9 L9,10 L10,10 L10,9 L9,9 Z M11,9 L11,10 L12,10 L12,9 L11,9 Z M3,11 L3,12 L4,12 L4,11 L3,11 Z M5,11 L5,12 L6,12 L6,11 L5,11 Z M7,11 L7,12 L8,12 L8,11 L7,11 Z M9,11 L9,12 L10,12 L10,11 L9,11 Z M11,11 L11,12 L12,12 L12,11 L11,11 Z M14,4 L14,2 L1,2 L1,4 L14,4 Z'/%3E%3Crect width='1' height='1' x='2'/%3E%3Crect width='1' height='1' x='12'/%3E%3C/g%3E%3C/svg%3E%0A\") no-repeat left center;\r\n    padding-right: 1em;\r\n    padding-left: 25px;\r\n    background-position: 4px 0px;\r\n}\r\n\r\n::-webkit-datetime-edit-fields-wrapper {\r\n}\r\n\r\n::-webkit-datetime-edit-text {\r\n    padding: 0 0.3em;\r\n}\r\n\r\n::-webkit-datetime-edit-month-field {\r\n}\r\n\r\n::-webkit-datetime-edit-day-field {\r\n}\r\n\r\n::-webkit-datetime-edit-year-field {\r\n}\r\n\r\n::-webkit-inner-spin-button {\r\n    /* display: none; */\r\n}\r\n\r\n::-webkit-calendar-picker-indicator {\r\n    background: transparent;\r\n    font-size: 17px;\r\n}\r\n\r\n    ::-webkit-calendar-picker-indicator:focus {\r\n        background-color: rgba(0,0,0,0.2);\r\n    }\r\n\r\ninput[type=\"date\"] {\r\n    -webkit-align-items: center;\r\n    display: -webkit-inline-flex;\r\n    font-family: monospace;\r\n    overflow: hidden;\r\n    padding: 0;\r\n    -webkit-padding-start: 1px;\r\n}\r\n\r\ninput::-webkit-datetime-edit {\r\n    -webkit-flex: 1;\r\n    -webkit-user-modify: read-only !important;\r\n    display: inline-block;\r\n    min-width: 0;\r\n    overflow: hidden;\r\n}\r\n\r\n/*\r\ninput::-webkit-datetime-edit-fields-wrapper {\r\n -webkit-user-modify: read-only !important;\r\n display: inline-block;\r\n padding: 1px 0;\r\n white-space: pre;\r\n\r\n}\r\n*/\r\n\r\n/*\r\ninput[type=\"date\"] {\r\n  background-color: red;\r\n  outline: none;\r\n}\r\n\r\ninput[type=\"date\"]::-webkit-clear-button {\r\n  font-size: 18px;\r\n  height: 30px;\r\n  position: relative;\r\n}\r\n\r\ninput[type=\"date\"]::-webkit-inner-spin-button {\r\n  height: 28px;\r\n}\r\n\r\ninput[type=\"date\"]::-webkit-calendar-picker-indicator {\r\n  font-size: 15px;\r\n} */\r\n\r\ninput[type=\"file\"] {\r\n    opacity: 0;\r\n    display: none;\r\n}\r\n\r\n.sdpi-item > input[type=\"file\"] {\r\n    opacity: 1;\r\n    display: flex;\r\n}\r\n\r\ninput[type=\"file\"] + span {\r\n    display: flex;\r\n    flex: 0 1 auto;\r\n    background-color: #0000ff50;\r\n}\r\n\r\nlabel.sdpi-file-label {\r\n    cursor: pointer;\r\n    user-select: none;\r\n    display: inline-block;\r\n    min-height: 21px !important;\r\n    height: 21px !important;\r\n    line-height: 20px;\r\n    padding: 0px 4px;\r\n    margin: auto;\r\n    margin-right: 0px;\r\n    float: right;\r\n}\r\n\r\n    .sdpi-file-label > label:active,\r\n    .sdpi-file-label.file:active,\r\n    label.sdpi-file-label:active,\r\n    label.sdpi-file-info:active,\r\n    input[type=\"file\"]::-webkit-file-upload-button:active,\r\n    button:active {\r\n        background-color: var(--sdpi-color);\r\n        color: #303030;\r\n    }\r\n\r\ninput:required:invalid, input:focus:invalid {\r\n    background: var(--sdpi-background) url(data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI5IiBoZWlnaHQ9IjkiIHZpZXdCb3g9IjAgMCA5IDkiPgogICAgPHBhdGggZmlsbD0iI0Q4RDhEOCIgZD0iTTQuNSwwIEM2Ljk4NTI4MTM3LC00LjU2NTM4NzgyZS0xNiA5LDIuMDE0NzE4NjMgOSw0LjUgQzksNi45ODUyODEzNyA2Ljk4NTI4MTM3LDkgNC41LDkgQzIuMDE0NzE4NjMsOSAzLjA0MzU5MTg4ZS0xNiw2Ljk4NTI4MTM3IDAsNC41IEMtMy4wNDM1OTE4OGUtMTYsMi4wMTQ3MTg2MyAyLjAxNDcxODYzLDQuNTY1Mzg3ODJlLTE2IDQuNSwwIFogTTQsMSBMNCw2IEw1LDYgTDUsMSBMNCwxIFogTTQuNSw4IEM0Ljc3NjE0MjM3LDggNSw3Ljc3NjE0MjM3IDUsNy41IEM1LDcuMjIzODU3NjMgNC43NzYxNDIzNyw3IDQuNSw3IEM0LjIyMzg1NzYzLDcgNCw3LjIyMzg1NzYzIDQsNy41IEM0LDcuNzc2MTQyMzcgNC4yMjM4NTc2Myw4IDQuNSw4IFoiLz4KICA8L3N2Zz4) no-repeat 98% center;\r\n}\r\n\r\ninput:required:valid {\r\n    background: var(--sdpi-background) url(data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI5IiBoZWlnaHQ9IjkiIHZpZXdCb3g9IjAgMCA5IDkiPjxwb2x5Z29uIGZpbGw9IiNEOEQ4RDgiIHBvaW50cz0iNS4yIDEgNi4yIDEgNi4yIDcgMy4yIDcgMy4yIDYgNS4yIDYiIHRyYW5zZm9ybT0icm90YXRlKDQwIDQuNjc3IDQpIi8+PC9zdmc+) no-repeat 98% center;\r\n}\r\n\r\n.tooltip,\r\n:tooltip,\r\n:title {\r\n    color: yellow;\r\n}\r\n/*\r\n[title]:hover {\r\n  display: flex;\r\n  align-items: center;\r\n  justify-content: center;\r\n}\r\n\r\n[title]:hover::after {\r\n  content: '';\r\n  position: absolute;\r\n  bottom: -1000px;\r\n  left: 8px;\r\n  display: none;\r\n  color: #fff;\r\n  border: 8px solid transparent;\r\n  border-bottom: 8px solid #000;\r\n}\r\n\r\n[title]:hover::before {\r\n    content: attr(title);\r\n    display: flex;\r\n    justify-content: center;\r\n    align-self: center;\r\n    padding: 6px 12px;\r\n    border-radius: 5px;\r\n    background: rgba(0,0,0,0.8);\r\n    color: var(--sdpi-color);\r\n    font-size: 9pt;\r\n    font-family: sans-serif;\r\n    opacity: 1;\r\n    position: absolute;\r\n    height: auto;\r\n\r\n    text-align: center;\r\n    bottom: 2px;\r\n    z-index: 100;\r\n    box-shadow: 0px 3px 6px rgba(0, 0, 0, .5);\r\n}\r\n*/\r\n\r\n.sdpi-item-group.file {\r\n    width: 233px;\r\n    display: flex;\r\n    align-items: center;\r\n}\r\n\r\n.sdpi-file-info {\r\n    overflow-wrap: break-word;\r\n    word-wrap: break-word;\r\n    hyphens: auto;\r\n    min-width: 132px;\r\n    max-width: 144px;\r\n    max-height: 32px;\r\n    margin-top: 0px;\r\n    margin-left: 6px;\r\n    display: inline-block;\r\n    overflow: hidden;\r\n    padding: 6px 4px;\r\n    background-color: var(--sdpi-background);\r\n}\r\n\r\n::-webkit-scrollbar {\r\n    width: 8px;\r\n}\r\n\r\n::-webkit-scrollbar-track {\r\n    -webkit-box-shadow: inset 0 0 6px rgba(0,0,0,0.3);\r\n}\r\n\r\n::-webkit-scrollbar-thumb {\r\n    background-color: #999999;\r\n    outline: 1px solid slategrey;\r\n    border-radius: 8px;\r\n}\r\n\r\na {\r\n    color: #7397d2;\r\n}\r\n\r\n.testcontainer {\r\n    display: flex;\r\n    background-color: #0000ff20;\r\n    max-width: 400px;\r\n    height: 200px;\r\n    align-content: space-evenly;\r\n}\r\n\r\ninput[type=range] {\r\n    -webkit-appearance: none;\r\n    /* background-color: green; */\r\n    height: 6px;\r\n    margin-top: 11px !important;\r\n    z-index: 0;\r\n    overflow: visible;\r\n}\r\n\r\n/*\r\ninput[type=\"range\"]::-webkit-slider-thumb {\r\n  -webkit-appearance: none;\r\n  background-color: var(--sdpi-color);\r\n  width: 12px;\r\n  height: 12px;\r\n  border-radius: 20px;\r\n  margin-top: -6px;\r\n  border: none;\r\n} */\r\n\r\n:-webkit-slider-thumb {\r\n    -webkit-appearance: none;\r\n    background-color: var(--sdpi-color);\r\n    width: 16px;\r\n    height: 16px;\r\n    border-radius: 20px;\r\n    margin-top: -6px;\r\n    border: 1px solid #999999;\r\n}\r\n\r\n.sdpi-item[type=\"range\"] .sdpi-item-group {\r\n    display: flex;\r\n    flex-direction: column;\r\n}\r\n\r\n.xxsdpi-item[type=\"range\"] .sdpi-item-group input {\r\n    max-width: 204px;\r\n}\r\n\r\n.sdpi-item[type=\"range\"] .sdpi-item-group span {\r\n    margin-left: 0px !important;\r\n}\r\n\r\n.sdpi-item[type=\"range\"] .sdpi-item-group > .sdpi-item-child {\r\n    display: flex;\r\n    flex-direction: row;\r\n}\r\n\r\n.rangeLabel {\r\n    position: absolute;\r\n    font-weight: normal;\r\n    margin-top: 22px;\r\n}\r\n\r\n:disabled {\r\n    color: #777777;\r\n}\r\n\r\nselect,\r\nselect option {\r\n    color: var(--sdpi-color);\r\n}\r\n\r\n    select.disabled,\r\n    select option:disabled {\r\n        color: #fd9494;\r\n        font-style: italic;\r\n    }\r\n\r\n.runningAppsContainer {\r\n    display: none;\r\n}\r\n\r\n/* debug\r\ndiv {\r\n  background-color: rgba(64,128,255,0.2);\r\n}\r\n*/\r\n\r\n.one-line {\r\n    min-height: 1.5em;\r\n}\r\n\r\n.two-lines {\r\n    min-height: 3em;\r\n}\r\n\r\n.three-lines {\r\n    min-height: 4.5em;\r\n}\r\n\r\n.four-lines {\r\n    min-height: 6em;\r\n}\r\n\r\n.min80 > .sdpi-item-child {\r\n    min-width: 80px;\r\n}\r\n\r\n.min100 > .sdpi-item-child {\r\n    min-width: 100px;\r\n}\r\n\r\n.min120 > .sdpi-item-child {\r\n    min-width: 120px;\r\n}\r\n\r\n.min140 > .sdpi-item-child {\r\n    min-width: 140px;\r\n}\r\n\r\n.min160 > .sdpi-item-child {\r\n    min-width: 160px;\r\n}\r\n\r\n.min200 > .sdpi-item-child {\r\n    min-width: 200px;\r\n}\r\n\r\n.max40 {\r\n    flex-basis: 40%;\r\n    flex-grow: 0;\r\n}\r\n\r\n.max30 {\r\n    flex-basis: 30%;\r\n    flex-grow: 0;\r\n}\r\n\r\n.max20 {\r\n    flex-basis: 20%;\r\n    flex-grow: 0;\r\n}\r\n\r\n.up20 {\r\n    margin-top: -20px;\r\n}\r\n\r\n.alignCenter {\r\n    align-items: center;\r\n}\r\n\r\n.alignTop {\r\n    align-items: flex-start;\r\n}\r\n\r\n.alignBaseline {\r\n    align-items: baseline;\r\n}\r\n\r\n.noMargins,\r\n.noMargins *,\r\n.noInnerMargins * {\r\n    margin: 0;\r\n    padding: 0;\r\n}\r\n\r\n.hidden {\r\n    display: none;\r\n}\r\n\r\n.icon-help,\r\n.icon-help-line,\r\n.icon-help-fill,\r\n.icon-help-inv,\r\n.icon-brighter,\r\n.icon-darker,\r\n.icon-warmer,\r\n.icon-cooler {\r\n    min-width: 20px;\r\n    width: 20px;\r\n    background-repeat: no-repeat;\r\n    opacity: 1;\r\n}\r\n\r\n    .icon-help:active,\r\n    .icon-help-line:active,\r\n    .icon-help-fill:active,\r\n    .icon-help-inv:active,\r\n    .icon-brighter:active,\r\n    .icon-darker:active,\r\n    .icon-warmer:active,\r\n    .icon-cooler:active {\r\n        opacity: 0.5;\r\n    }\r\n\r\n.icon-brighter,\r\n.icon-darker,\r\n.icon-warmer,\r\n.icon-cooler {\r\n    margin-top: 5px !important;\r\n}\r\n\r\n.icon-help,\r\n.icon-help-line,\r\n.icon-help-fill,\r\n.icon-help-inv {\r\n    cursor: pointer;\r\n    margin: 0px;\r\n    margin-left: 4px;\r\n}\r\n\r\n.icon-brighter {\r\n    background-image: url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='20' height='20' viewBox='0 0 20 20'%3E%3Cg fill='%23999' fill-rule='evenodd'%3E%3Ccircle cx='10' cy='10' r='4'/%3E%3Cpath d='M14.8532861,7.77530426 C14.7173255,7.4682615 14.5540843,7.17599221 14.3666368,6.90157083 L16.6782032,5.5669873 L17.1782032,6.4330127 L14.8532861,7.77530426 Z M10.5,4.5414007 C10.2777625,4.51407201 10.051423,4.5 9.82179677,4.5 C9.71377555,4.5 9.60648167,4.50311409 9.5,4.50925739 L9.5,2 L10.5,2 L10.5,4.5414007 Z M5.38028092,6.75545367 C5.18389364,7.02383457 5.01124349,7.31068015 4.86542112,7.61289977 L2.82179677,6.4330127 L3.32179677,5.5669873 L5.38028092,6.75545367 Z M4.86542112,12.3871002 C5.01124349,12.6893198 5.18389364,12.9761654 5.38028092,13.2445463 L3.32179677,14.4330127 L2.82179677,13.5669873 L4.86542112,12.3871002 Z M9.5,15.4907426 C9.60648167,15.4968859 9.71377555,15.5 9.82179677,15.5 C10.051423,15.5 10.2777625,15.485928 10.5,15.4585993 L10.5,18 L9.5,18 L9.5,15.4907426 Z M14.3666368,13.0984292 C14.5540843,12.8240078 14.7173255,12.5317385 14.8532861,12.2246957 L17.1782032,13.5669873 L16.6782032,14.4330127 L14.3666368,13.0984292 Z'/%3E%3C/g%3E%3C/svg%3E\");\r\n}\r\n\r\n.icon-darker {\r\n    background-image: url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='20' height='20' viewBox='0 0 20 20'%3E%3Cg fill='%23999' fill-rule='evenodd'%3E%3Cpath d='M10 14C7.790861 14 6 12.209139 6 10 6 7.790861 7.790861 6 10 6 12.209139 6 14 7.790861 14 10 14 12.209139 12.209139 14 10 14zM10 13C11.6568542 13 13 11.6568542 13 10 13 8.34314575 11.6568542 7 10 7 8.34314575 7 7 8.34314575 7 10 7 11.6568542 8.34314575 13 10 13zM14.8532861 7.77530426C14.7173255 7.4682615 14.5540843 7.17599221 14.3666368 6.90157083L16.6782032 5.5669873 17.1782032 6.4330127 14.8532861 7.77530426zM10.5 4.5414007C10.2777625 4.51407201 10.051423 4.5 9.82179677 4.5 9.71377555 4.5 9.60648167 4.50311409 9.5 4.50925739L9.5 2 10.5 2 10.5 4.5414007zM5.38028092 6.75545367C5.18389364 7.02383457 5.01124349 7.31068015 4.86542112 7.61289977L2.82179677 6.4330127 3.32179677 5.5669873 5.38028092 6.75545367zM4.86542112 12.3871002C5.01124349 12.6893198 5.18389364 12.9761654 5.38028092 13.2445463L3.32179677 14.4330127 2.82179677 13.5669873 4.86542112 12.3871002zM9.5 15.4907426C9.60648167 15.4968859 9.71377555 15.5 9.82179677 15.5 10.051423 15.5 10.2777625 15.485928 10.5 15.4585993L10.5 18 9.5 18 9.5 15.4907426zM14.3666368 13.0984292C14.5540843 12.8240078 14.7173255 12.5317385 14.8532861 12.2246957L17.1782032 13.5669873 16.6782032 14.4330127 14.3666368 13.0984292z'/%3E%3C/g%3E%3C/svg%3E\");\r\n}\r\n\r\n.icon-warmer {\r\n    background-image: url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='20' height='20' viewBox='0 0 20 20'%3E%3Cg fill='%23999' fill-rule='evenodd'%3E%3Cpath d='M12.3247275 11.4890349C12.0406216 11.0007637 11.6761954 10.5649925 11.2495475 10.1998198 11.0890394 9.83238991 11 9.42659309 11 9 11 7.34314575 12.3431458 6 14 6 15.6568542 6 17 7.34314575 17 9 17 10.6568542 15.6568542 12 14 12 13.3795687 12 12.8031265 11.8116603 12.3247275 11.4890349zM17.6232392 11.6692284C17.8205899 11.4017892 17.9890383 11.1117186 18.123974 10.8036272L20.3121778 12.0669873 19.8121778 12.9330127 17.6232392 11.6692284zM18.123974 7.19637279C17.9890383 6.88828142 17.8205899 6.5982108 17.6232392 6.33077158L19.8121778 5.0669873 20.3121778 5.9330127 18.123974 7.19637279zM14.5 4.52746439C14.3358331 4.50931666 14.1690045 4.5 14 4.5 13.8309955 4.5 13.6641669 4.50931666 13.5 4.52746439L13.5 2 14.5 2 14.5 4.52746439zM13.5 13.4725356C13.6641669 13.4906833 13.8309955 13.5 14 13.5 14.1690045 13.5 14.3358331 13.4906833 14.5 13.4725356L14.5 16 13.5 16 13.5 13.4725356zM14 11C15.1045695 11 16 10.1045695 16 9 16 7.8954305 15.1045695 7 14 7 12.8954305 7 12 7.8954305 12 9 12 10.1045695 12.8954305 11 14 11zM9.5 11C10.6651924 11.4118364 11.5 12.5 11.5 14 11.5 16 10 17.5 8 17.5 6 17.5 4.5 16 4.5 14 4.5 12.6937812 5 11.5 6.5 11L6.5 7 9.5 7 9.5 11z'/%3E%3Cpath d='M12,14 C12,16.209139 10.209139,18 8,18 C5.790861,18 4,16.209139 4,14 C4,12.5194353 4.80439726,11.2267476 6,10.5351288 L6,4 C6,2.8954305 6.8954305,2 8,2 C9.1045695,2 10,2.8954305 10,4 L10,10.5351288 C11.1956027,11.2267476 12,12.5194353 12,14 Z M11,14 C11,12.6937812 10.1651924,11.5825421 9,11.1707057 L9,4 C9,3.44771525 8.55228475,3 8,3 C7.44771525,3 7,3.44771525 7,4 L7,11.1707057 C5.83480763,11.5825421 5,12.6937812 5,14 C5,15.6568542 6.34314575,17 8,17 C9.65685425,17 11,15.6568542 11,14 Z'/%3E%3C/g%3E%3C/svg%3E\");\r\n}\r\n\r\n.icon-cooler {\r\n    background-image: url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='20' height='20' viewBox='0 0 20 20'%3E%3Cg fill='%23999' fill-rule='evenodd'%3E%3Cpath d='M10.4004569 11.6239517C10.0554735 10.9863849 9.57597206 10.4322632 9 9.99963381L9 9.7450467 9.53471338 9.7450467 10.8155381 8.46422201C10.7766941 8.39376637 10.7419749 8.32071759 10.7117062 8.2454012L9 8.2454012 9 6.96057868 10.6417702 6.96057868C10.6677696 6.86753378 10.7003289 6.77722682 10.7389179 6.69018783L9.44918707 5.40045694 9 5.40045694 9 4.34532219 9.32816127 4.34532219 9.34532219 2.91912025 10.4004569 2.91912025 10.4004569 4.53471338 11.6098599 5.74411634C11.7208059 5.68343597 11.8381332 5.63296451 11.9605787 5.59396526L11.9605787 3.8884898 10.8181818 2.74609294 11.5642748 2 12.5727518 3.00847706 13.5812289 2 14.3273218 2.74609294 13.2454012 3.82801356 13.2454012 5.61756719C13.3449693 5.65339299 13.4408747 5.69689391 13.5324038 5.74735625L14.7450467 4.53471338 14.7450467 2.91912025 15.8001815 2.91912025 15.8001815 4.34532219 17.2263834 4.34532219 17.2263834 5.40045694 15.6963166 5.40045694 14.4002441 6.69652946C14.437611 6.78161093 14.4692249 6.86979146 14.4945934 6.96057868L16.2570138 6.96057868 17.3994107 5.81818182 18.1455036 6.56427476 17.1370266 7.57275182 18.1455036 8.58122888 17.3994107 9.32732182 16.3174901 8.2454012 14.4246574 8.2454012C14.3952328 8.31861737 14.3616024 8.38969062 14.3240655 8.45832192L15.6107903 9.7450467 17.2263834 9.7450467 17.2263834 10.8001815 15.8001815 10.8001815 15.8001815 12.2263834 14.7450467 12.2263834 14.7450467 10.6963166 13.377994 9.32926387C13.3345872 9.34850842 13.2903677 9.36625331 13.2454012 9.38243281L13.2454012 11.3174901 14.3273218 12.3994107 13.5812289 13.1455036 12.5848864 12.1491612 11.5642748 13.1455036 10.8181818 12.3994107 11.9605787 11.2570138 11.9605787 9.40603474C11.8936938 9.38473169 11.828336 9.36000556 11.7647113 9.33206224L10.4004569 10.6963166 10.4004569 11.6239517zM12.75 8.5C13.3022847 8.5 13.75 8.05228475 13.75 7.5 13.75 6.94771525 13.3022847 6.5 12.75 6.5 12.1977153 6.5 11.75 6.94771525 11.75 7.5 11.75 8.05228475 12.1977153 8.5 12.75 8.5zM9.5 14C8.5 16.3333333 7.33333333 17.5 6 17.5 4.66666667 17.5 3.5 16.3333333 2.5 14L9.5 14z'/%3E%3Cpath d='M10,14 C10,16.209139 8.209139,18 6,18 C3.790861,18 2,16.209139 2,14 C2,12.5194353 2.80439726,11.2267476 4,10.5351288 L4,4 C4,2.8954305 4.8954305,2 6,2 C7.1045695,2 8,2.8954305 8,4 L8,10.5351288 C9.19560274,11.2267476 10,12.5194353 10,14 Z M9,14 C9,12.6937812 8.16519237,11.5825421 7,11.1707057 L7,4 C7,3.44771525 6.55228475,3 6,3 C5.44771525,3 5,3.44771525 5,4 L5,11.1707057 C3.83480763,11.5825421 3,12.6937812 3,14 C3,15.6568542 4.34314575,17 6,17 C7.65685425,17 9,15.6568542 9,14 Z'/%3E%3C/g%3E%3C/svg%3E\");\r\n}\r\n\r\n.icon-help {\r\n    background-image: url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='20' height='20'%3E%3Cpath fill='%23999' d='M11.292 12.516l.022 1.782H9.07v-1.804c0-1.98 1.276-2.574 2.662-3.278h-.022c.814-.44 1.65-.88 1.694-2.2.044-1.386-1.122-2.728-3.234-2.728-1.518 0-2.662.902-3.366 2.354L5 5.608C5.946 3.584 7.662 2 10.17 2c3.564 0 5.632 2.442 5.588 5.06-.066 2.618-1.716 3.41-3.102 4.158-.704.374-1.364.682-1.364 1.298zm-1.122 2.442c.858 0 1.452.594 1.452 1.452 0 .682-.594 1.408-1.452 1.408-.77 0-1.386-.726-1.386-1.408 0-.858.616-1.452 1.386-1.452z'/%3E%3C/svg%3E\");\r\n}\r\n\r\n.icon-help-line {\r\n    background-image: url(\"data:image/svg+xml,%3Csvg width='20' height='20' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='%23999' fill-rule='evenodd'%3E%3Cpath d='M10 20C4.477 20 0 15.523 0 10S4.477 0 10 0s10 4.477 10 10-4.477 10-10 10zm0-1a9 9 0 1 0 0-18 9 9 0 0 0 0 18z'/%3E%3Cpath d='M10.848 12.307l.02 1.578H8.784v-1.597c0-1.753 1.186-2.278 2.474-2.901h-.02c.756-.39 1.533-.78 1.574-1.948.041-1.226-1.043-2.414-3.006-2.414-1.41 0-2.474.798-3.128 2.083L5 6.193C5.88 4.402 7.474 3 9.805 3 13.118 3 15.04 5.161 15 7.478c-.061 2.318-1.595 3.019-2.883 3.68-.654.332-1.268.604-1.268 1.15zM9.805 14.47c.798 0 1.35.525 1.35 1.285 0 .603-.552 1.246-1.35 1.246-.715 0-1.288-.643-1.288-1.246 0-.76.573-1.285 1.288-1.285z' fill-rule='nonzero'/%3E%3C/g%3E%3C/svg%3E\");\r\n}\r\n\r\n.icon-help-fill {\r\n    background-image: url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='20' height='20'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Ccircle cx='10' cy='10' r='10' fill='%23999'/%3E%3Cpath fill='%23FFF' fill-rule='nonzero' d='M8.368 7.189H5C5 3.5 7.668 2 10.292 2 13.966 2 16 4.076 16 7.012c0 3.754-3.849 3.136-3.849 5.211v1.656H8.455v-1.832c0-2.164 1.4-2.893 2.778-3.6.437-.242 1.006-.574 1.006-1.236 0-2.208-3.871-2.142-3.871-.022zM10.25 18a1.75 1.75 0 1 1 0-3.5 1.75 1.75 0 0 1 0 3.5z'/%3E%3C/g%3E%3C/svg%3E\");\r\n}\r\n\r\n.icon-help-inv {\r\n    background-image: url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='20' height='20'%3E%3Cpath fill='%23999' fill-rule='evenodd' d='M10 20C4.477 20 0 15.523 0 10S4.477 0 10 0s10 4.477 10 10-4.477 10-10 10zM8.368 7.189c0-2.12 3.87-2.186 3.87.022 0 .662-.568.994-1.005 1.236-1.378.707-2.778 1.436-2.778 3.6v1.832h3.696v-1.656c0-2.075 3.849-1.457 3.849-5.21C16 4.075 13.966 2 10.292 2 7.668 2 5 3.501 5 7.189h3.368zM10.25 18a1.75 1.75 0 1 0 0-3.5 1.75 1.75 0 0 0 0 3.5z'/%3E%3C/svg%3E\");\r\n}\r\n\r\n.kelvin::after {\r\n    content: \"K\";\r\n}\r\n\r\n.mired::after {\r\n    content: \" Mired\";\r\n}\r\n\r\n.percent::after {\r\n    content: \"%\";\r\n}\r\n\r\n.sdpi-item-value + .icon-cooler,\r\n.sdpi-item-value + .icon-warmer {\r\n    margin-left: 0px !important;\r\n    margin-top: 15px !important;\r\n}\r\n\r\n/**\r\n  CONTROL-CENTER STYLES\r\n*/\r\ninput[type=\"range\"].colorbrightness::-webkit-slider-runnable-track,\r\ninput[type=\"range\"].colortemperature::-webkit-slider-runnable-track {\r\n    height: 8px;\r\n    background: #979797;\r\n    border-radius: 4px;\r\n    background-image: linear-gradient(to right,#94d0ec, #ffb165);\r\n}\r\n\r\ninput[type=\"range\"].colorbrightness::-webkit-slider-runnable-track {\r\n    background-color: #efefef;\r\n    background-image: linear-gradient(to right, black, rgba(0,0,0,0));\r\n}\r\n\r\ninput[type=\"range\"].colorbrightness::-webkit-slider-thumb,\r\ninput[type=\"range\"].colortemperature::-webkit-slider-thumb {\r\n    width: 16px;\r\n    height: 16px;\r\n    border-radius: 20px;\r\n    margin-top: -5px;\r\n    background-color: #86c6e8;\r\n    box-shadow: 0px 0px 1px #000000;\r\n    border: 1px solid #d8d8d8;\r\n}\r\n\r\n.sdpi-info-label {\r\n    display: inline-block;\r\n    user-select: none;\r\n    position: absolute;\r\n    height: 15px;\r\n    width: auto;\r\n    text-align: center;\r\n    border-radius: 4px;\r\n    min-width: 44px;\r\n    max-width: 80px;\r\n    background: white;\r\n    font-size: 11px;\r\n    color: black;\r\n    z-index: 1000;\r\n    box-shadow: 0px 0px 12px rgba(0,0,0,.8);\r\n    padding: 2px;\r\n}\r\n\r\n    .sdpi-info-label.hidden {\r\n        opacity: 0;\r\n        transition: opacity 0.25s linear;\r\n    }\r\n\r\n    .sdpi-info-label.shown {\r\n        position: absolute;\r\n        opacity: 1;\r\n        transition: opacity 0.25s ease-out;\r\n    }\r\n";
    styleInject(css_248z);

    exports.Checkbox = checkbox;
    exports.FolderPicker = folderPicker;
    exports.PropertyInspectorWrapper = PropertyInspectorWrapper;
    exports.Range = range;
    exports.Select = select;
    exports.TextField = textfield;
    exports.connect = connect;
    exports.css = css_248z;
    exports.store = store;
    exports.streamDeckClient = streamDeckClient;

    Object.defineProperty(exports, '__esModule', { value: true });

}));
