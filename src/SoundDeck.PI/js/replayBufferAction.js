import client from './streamDeckClient';

async function init() {
    var settings = await client.getSettings();
    console.group("Settings")
    console.log(settings);
    console.groupEnd();

    var global = await client.getGlobalSettings();
    console.group("Global")
    console.log(global);
    console.groupEnd();

    var me = await client.get("SayHello");
    console.log(me);
}

init();
