import client from './streamDeckClient';
import soundDeck from './soundDeck';
import utils from './utils';

const DEFAULT_SETTINGS = {
    audioDeviceId: null,
    clipDuration: null,
    outputPath: "C:\\Temp\\"
};

let settings = {};

async function bind() {
    const getAudioDevicesResponse = await client.get("GetAudioDevices");
    const devicesElem = document.getElementById("devices");

    let grps = {};
    getAudioDevicesResponse.payload.devices.forEach(device => {
        const label = soundDeck.enums.FLOW[device.flow];
        if (grps[label] === undefined) {
            grps[label] = [];
        }

        grps[label].push(device);
    });

    utils.dataBindGroups(devicesElem, grps, (item, option) => {
        option.innerText = item.friendlyName;
        option.value = item.id;
    });

    observe(devicesElem, "audioDeviceId");
}

function bindDuration() {
    const elem = document.getElementById("duration");

    observe(elem, "clipDuration");
    for (var i = 0; i < elem.options.length; i++) {
        elem.options[i].removeAttribute("disabled");
    }
}

function observe(elem, key) {
    elem.removeAttribute("disabled");
    elem.value = settings[key];

    elem.addEventListener("change", (ev) => {
        const val = ev.target.options[ev.target.selectedIndex].value;
        if (val !== "" && settings[key] !== val) {
            settings[key] = val;
            client.setSettings(settings);
        }
    });
}

console.time();
(async function () {
    await client.connect();
    console.log(client);
    settings = { ...DEFAULT_SETTINGS, ...client.actionInfo.payload.settings }

    bindDuration();
    await bind();
    console.timeEnd();

    document.getElementById("settings").classList.remove("hidden");
    document.getElementById("loading").classList.add("hidden");
})();
