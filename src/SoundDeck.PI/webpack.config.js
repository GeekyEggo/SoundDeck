const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const CopyPlugin = require('copy-webpack-plugin')

const piPath = path.resolve(process.env['APPDATA'], 'Elgato/StreamDeck/Plugins/com.geekyeggo.sounddeck.sdPlugin/PI')

module.exports = {
    devtool: 'source-map',
    entry: {
        replayBufferAction: path.resolve(__dirname, 'js/replayBufferAction.js'),
        vendor: ['jquery']
    },
    output: {
        filename: '[name].js',
        path: piPath
    },
    plugins: [
        new CleanWebpackPlugin(),
        new CopyPlugin([
            {
                from: __dirname + '**/*.html',
                to: piPath,
                context: __dirname
            },
        ]),
    ]
};