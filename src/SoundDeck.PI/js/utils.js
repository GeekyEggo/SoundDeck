import client from './streamDeckClient';

let utils = {};

utils.change = (elem, val) => {
    elem.value = val;

    const event = new Event('change', { bubbles: true });
    elem.dispatchEvent(event);
};

utils.dataBindGroups = (select, groups, fnBindOption) => {
    select.length = 0;

    if (groups === null || groups === undefined || Object.keys(groups).length === 0) {
        return;
    }

    for (let key in groups) {
        var grp = document.createElement("optgroup");
        grp.label = key;

        groups[key].forEach(item => {
            var option = document.createElement("option");
            fnBindOption(item, option);

            grp.appendChild(option);
        })

        select.appendChild(grp);
    }
};

utils.observe = (elem, settings, key, onChange) => {
    elem.value = settings[key];
    elem.addEventListener("change", (ev) => {
        if (settings[key] !== ev.target.value) {
            settings[key] = ev.target.value;
            client.setSettings(settings);

            if (onChange !== undefined) {
                onChange(ev.target.value);
            }
        }
    });
};

export default utils;
