const path = require("path");
const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const CopyPlugin = require("copy-webpack-plugin")

const source = path.resolve(__dirname, "src/SoundDeck.PI");
const dest = path.resolve(process.env["APPDATA"], "Elgato/StreamDeck/Plugins/com.geekyeggo.sounddeck.sdPlugin/PI");

let config = {
    entry: {
        index: path.resolve(source, "js/index.jsx")
    },
    module: {
        rules: [{
            test: /\.css$/,
            use: [{ loader: "style-loader" }, { loader: "css-loader" }]
        },
        {
            test: /\.jsx$/,
			include: [
				/react-sharpdeck/,
				source
			],
            exclude: /node_modules/,
            use: {
                loader: "babel-loader",
				options: {
					presets: ["@babel/preset-react"]
				}
            }
        }]
    },
    output: {
        filename: "[name].js",
        path: dest
    },
    plugins: [
        new CleanWebpackPlugin({
            cleanAfterEveryBuildPatterns: ["!*.html", "!css/**/*.*", "!imgs/**/*.*"],
        }),
        new CopyPlugin([
            {
                from: source,
                to: dest,
                context: source,
                ignore: [".babelrc", "*.csproj", "*.js", "*.jsx", "bin/**/*.*", "obj/**/*.*", "js/react-sharpdeck/**/*"]
            },
        ])
    ],
    resolve: {
        extensions: [".js", ".jsx"]
    }
};

module.exports = (env, argv) => {
    if (argv.mode === "development") {
        config.devtool = "inline-source-map";
    }

    return config;
};
