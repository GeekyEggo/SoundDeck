import client from './streamDeckClient';

/*
async function doSomething() {
    await streamDeckClient.connect();
    streamDeckClient.logMessage("Hello world, this is from the property inspector");
    console.log("Done");
    return true;
}
*/

async function init() {
    await client.connect();
    client.openUrl("https://www.google.com");

    var settings = await client.getSettings();
    console.log(settings);
}

init();