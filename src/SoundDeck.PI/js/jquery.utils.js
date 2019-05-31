import jQuery from 'jquery';

(function ($) {
    /**
     * Populates the jQuery object, typically a select element, with the specified groups.
     * @param {Array} groups The items used to populate the dropdown.
     * @param {Function} fnGetOptions The delegate used to get the child options.
     * @param {Function} fnGetValue The delegate to get the value of an option or group.
     * @param {Function} fnGetText The delegate to get the text of an option or group.
     * @returns The jQuery items.
     */
    $.fn.dropdown = function (groups, fnGetOptions, fnGetValue, fnGetText) {
        return this.each(function () {
            const $this = $(this).empty();

            /**
             * Adds the option to this instance.
             * @param {any} option The option to add.
             */
            function addOption(option) {
                $this.append($('<option />').val(fnGetValue(option)).text(fnGetText(option)));
            };

            // iterate over each group, when the group has no children, treat it as an option
            groups.forEach(grp => {
                const options = fnGetOptions(grp);
                if (options.length === 0) {
                    addOption(grp);
                } else {
                    $this.append($('<optgroup />').attr('label', fnGetText(grp)));
                    options.forEach(addOption);
                }
            });
        })
    };
}(jQuery));
