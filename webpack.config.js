const path = require("path");
const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const CopyPlugin = require("copy-webpack-plugin")
const package = require("./package.json");

module.exports = (env, argv) => {
    const source = path.resolve(__dirname, package.paths.pi);
    const dest = argv.dist
        ? path.resolve(__dirname, package.paths.dist, "PI")
        : path.resolve(process.env["APPDATA"], package.paths.elgato, "PI");

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
            chunkFilename: "./js/[name].bundle.js",
            filename: "./js/[name].js",
            path: dest
        },
        plugins: [
            new CleanWebpackPlugin({
                cleanAfterEveryBuildPatterns: ["!*.html", "!css/**/*.*", "!imgs/**/*.*"],
            }),
            new CopyPlugin([{
                from: source,
                to: dest,
                context: source,
                ignore: [".babelrc", "*.csproj", "*.csproj*", "*.js", "*.jsx", "bin/**/*.*", "obj/**/*.*"]
            }])
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
    if (argv.mode === "development") {
        config.devtool = "inline-source-map";
    }

    return config;
};
