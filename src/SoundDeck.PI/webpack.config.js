const path = require("path");
const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const CopyPlugin = require("copy-webpack-plugin")

const piPath = path.resolve(process.env["APPDATA"], "Elgato/StreamDeck/Plugins/com.geekyeggo.sounddeck.sdPlugin/PI")

module.exports = {
    devtool: "source-map",
    entry: {
        captureAudioBuffer: path.resolve(__dirname, "js/captureAudioBuffer.js")
    },
    module: {
        rules: [{
            test: /\.css$/,
            use: [{ loader: "style-loader" }, { loader: "css-loader" }]
        }]
    },
    output: {
        filename: "[name].js",
        path: piPath
    },
    plugins: [
        new CleanWebpackPlugin(),
        new CopyPlugin([
            {
                from: __dirname,
                to: piPath,
                context: __dirname,
                ignore: ["*.csproj", "*.js", "bin/**/*.*", "obj/**/*.*"]
            },
        ]),
    ]
};