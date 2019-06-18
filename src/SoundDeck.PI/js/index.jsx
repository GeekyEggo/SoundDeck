import React from "react"
import ReactDOM from "react-dom";
import App from "./components/app"
import { streamDeckClient } from "./react-sounddeck";

const render = () =>
    ReactDOM.render(<App />, document.getElementById("root")
);

streamDeckClient.connect().then(render);
