import client from './streamDeckClient';
import soundDeck from './soundDeck';
import utils from './utils';

const DEFAULT_SETTINGS = {
    audioDeviceId: null,
    clipDuration: null,
    outputPath: null
};

let settings = {};

async function bindAsync() {
    await bindAudioDevicesAsync();
    bindDuration();
    bindOutputPath();
}

/**
 * Binds the audio devices drop down asynchronously by loading the devices from the plugin API
 */
async function bindAudioDevicesAsync() {
    const input = document.getElementById("devices");

    let grps = {};
    (await client.get("GetAudioDevices")).payload.devices.forEach(device => {
        const label = soundDeck.enums.FLOW[device.flow];
        if (grps[label] === undefined) {
            grps[label] = [];
        }

        grps[label].push(device);
    });

    utils.dataBindGroups(input, grps, (item, option) => {
        option.innerText = item.friendlyName;
        option.value = item.id;
    });

    utils.observe(input, settings, "audioDeviceId");
}

/**
 * Binds the duration drop down, and monitors for changes.
 * */
function bindDuration() {
    const input = document.getElementById("duration");
    utils.observe(input, settings, "clipDuration");
}

/**
 * Binds the output path control, and button to open a folder picker.
 * */
function bindOutputPath() {
    const input = document.getElementById("outputPath"),
        outputPathLbl = document.getElementById("outputPathLbl"),
        outputPathBtn = document.getElementById("outputPathBtn");

    const setLabel = (val) => {
        outputPathLbl.innerText = val || "No folder";
    };
    setLabel(settings.outputPath);
    utils.observe(input, settings, "outputPath", setLabel);

    outputPathBtn.addEventListener("click", async _ => {
        var response = await client.get("GetOutputPath");
        if (response.payload.success) {
            utils.change(input, response.payload.path);
        }
    })
}

console.time();
(async function () {
    await client.connect();
    console.log(client);
    settings = { ...DEFAULT_SETTINGS, ...client.actionInfo.payload.settings }

    await bindAsync();
    console.timeEnd();

    document.getElementById("settings").classList.remove("hidden");
    document.getElementById("loading").classList.add("hidden");
})();
