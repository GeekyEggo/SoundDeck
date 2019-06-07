import React from "react"
import ReactDOM from "react-dom";
import App from "./components/app"
import client from "./common/streamDeckClient";

const render = () =>
    ReactDOM.render(<App />, document.getElementById("root")
);

client.connect().then(render);
