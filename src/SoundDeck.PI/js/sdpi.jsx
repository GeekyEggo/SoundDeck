import React from "react";
import ReactDOM from "react-dom";
import client from "./streamDeckClient";
import CaptureAudioBufferSettings from "./forms/captureAudioBufferSettings";

function render() {
    const container = document.getElementById("root");
    ReactDOM.render(getElement(container), container);
}

function getElement(container) {
    switch (container.getAttribute("data-settings")) {
        case "captureAudioBufferSettings":
            return <CaptureAudioBufferSettings />;
        default:
            return (
                <details className="message">
                    <summary>data-settings attribute not set on root element</summary>
                </details>
            );
    }
}

client.connect().then(render);
