let utils = {};

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

export default utils;
