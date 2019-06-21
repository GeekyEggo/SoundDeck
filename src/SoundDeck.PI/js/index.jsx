import React from "react"
import ReactDOM from "react-dom";
import App from "./components/app"
import { streamDeckClient } from "./react-sharpdeck";

const render = (args) => {
    ReactDOM.render(<App uuid={args.actionInfo.action} />, document.getElementById("root"));
};

streamDeckClient.connect().then(render);
