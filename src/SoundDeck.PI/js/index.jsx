import React from "react"
import ReactDOM from "react-dom";
import { Provider } from "react-redux"
import { createStore, applyMiddleware } from "redux"
import { settingsReducer, connectElgatoStreamDeck, saveSettings } from "./common/actionSettingsStore"
import App from "./components/app"
import client from "./common/streamDeckClient";

const store = createStore(settingsReducer, applyMiddleware(saveSettings));
connectElgatoStreamDeck(store);

const render = () =>
    ReactDOM.render((
    <Provider store={store}>
        <App />
    </Provider >
    ), document.getElementById("root")
);

client.connect().then(render);
