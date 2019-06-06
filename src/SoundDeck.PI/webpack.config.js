const path = require("path");
const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const CopyPlugin = require("copy-webpack-plugin")

const piPath = path.resolve(process.env["APPDATA"], "Elgato/StreamDeck/Plugins/com.geekyeggo.sounddeck.sdPlugin/PI")

module.exports = {
    devtool: "source-map",
    entry: {
        sdpi: path.resolve(__dirname, "js/sdpi.jsx")
    },
    module: {
        rules: [{
            test: /\.css$/,
            use: [{ loader: "style-loader" }, { loader: "css-loader" }]
        },
        {
            test: /\.jsx$/,
            exclude: /node_modules/,
            use: {
                loader: "babel-loader"
            }
        }]
    },
    output: {
        filename: "[name].js",
        path: piPath
    },
    plugins: [
        new CleanWebpackPlugin({
            cleanAfterEveryBuildPatterns: ["!*.html", "!css/**/*.*", "!imgs/**/*.*"],
        }),
        new CopyPlugin([
            {
                from: __dirname,
                to: piPath,
                context: __dirname,
                ignore: [".babelrc", "*.csproj", "*.js", "*.jsx", "bin/**/*.*", "obj/**/*.*"]
            },
        ])
    ],
    resolve: {
        extensions: [".js", ".jsx"]
    }
};
