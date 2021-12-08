const path = require("path");
const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const CopyPlugin = require("copy-webpack-plugin")

module.exports = (env, _) => {
    const source = path.resolve(__dirname, "src/SoundDeck.PI");
    const dest = env.dist
        ? path.resolve(__dirname, "dist/com.geekyeggo.sounddeck.sdPlugin/PI")
        : path.resolve(process.env["APPDATA"], "Elgato/StreamDeck/Plugins/com.geekyeggo.sounddeck.sdPlugin/PI");

    let config = {
        entry: {
            index: path.resolve(source, "js/index.jsx")
        },
        module: {
            rules: [{
                test: /\.css$/,
                use: [
                    { loader: "style-loader/url" },
                    {
                        loader: "file-loader",
                        options: {
                            name: "./css/[name].css"
                        }
                    }],
            }, {
                test: /\.jsx$/,
                include: source,
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
            filename: "./js/[name].js",
            path: dest
        },
        plugins: [
            new CleanWebpackPlugin({
                cleanAfterEveryBuildPatterns: ["!*.html", "!css/**/*.*", "!imgs/**/*.*"],
            }),
            new CopyPlugin({
                patterns: [
                    {
                        from: source,
                        to: dest,
                        context: source,
                        globOptions: {
                            ignore: [
                                "**/bin",
                                "**/obj",
                                "**/.babelrc",
                                "**/*.csproj",
                                "**/*.csproj*",
                                "**/*.jsx"
                            ]
                        }
                    }
                ]
            })
        ],
        resolve: {
            extensions: [".js", ".jsx"],
            alias: {
                react: path.resolve("./node_modules/react"),
                "react-dom": path.resolve("./node_modules/react-dom"),
                "react-redux": path.resolve("./node_modules/react-redux")
            },
        },
        optimization: {
            splitChunks: {
                cacheGroups: {
                    vendor: {
                        test: /node_modules/,
                        chunks: "all",
                        name: "vendor",
                        enforce: true,
                        priority: -1
                    }
                }
            }
        }
    };

    // set the dev tool
    if (!env.dist) {
        config.devtool = "inline-source-map";
    }

    return config;
};
