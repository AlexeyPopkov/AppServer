const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const CopyPlugin = require("copy-webpack-plugin");
const HtmlWebpackPlugin = require("html-webpack-plugin");
const ModuleFederationPlugin = require("webpack").container
  .ModuleFederationPlugin;
const TerserPlugin = require("terser-webpack-plugin");
const { InjectManifest } = require("workbox-webpack-plugin");
const combineUrl = require("@appserver/common/utils/combineUrl");
const AppServerConfig = require("@appserver/common/constants/AppServerConfig");
const { proxyURL } = AppServerConfig;

const path = require("path");
const pkg = require("./package.json");
const deps = pkg.dependencies;
const homepage = pkg.homepage; // combineUrl(proxyURL, pkg.homepage);
const title = pkg.title;

var config = {
  mode: "development",
  entry: "./src/index",

  devServer: {
    publicPath: homepage,

    contentBase: [path.join(__dirname, "dist")],
    contentBasePublicPath: homepage,
    port: 5011,
    historyApiFallback: {
      // Paths with dots should still use the history fallback.
      // See https://github.com/facebook/create-react-app/issues/387.
      disableDotRule: true,
      index: homepage,
    },
    // proxy: [
    //   {
    //     context: "/api",
    //     target: "http://localhost:8092",
    //   },
    // ],
    hot: false,
    hotOnly: false,
    headers: {
      "Access-Control-Allow-Origin": "*",
      "Access-Control-Allow-Methods": "GET, POST, PUT, DELETE, PATCH, OPTIONS",
      "Access-Control-Allow-Headers":
        "X-Requested-With, content-type, Authorization",
    },
  },

  output: {
    publicPath: "auto",
    chunkFilename: "static/js/[id].[contenthash].js",
    //assetModuleFilename: "static/images/[hash][ext][query]",
    path: path.resolve(process.cwd(), "dist"),
    filename: "static/js/[name].[contenthash].bundle.js",
  },

  resolve: {
    extensions: [".jsx", ".js", ".json"],
    fallback: {
      crypto: false,
    },
  },

  module: {
    rules: [
      {
        test: /\.(png|jpe?g|gif|ico)$/i,
        type: "asset/resource",
        generator: {
          filename: "static/images/[hash][ext][query]",
        },
      },
      {
        test: /\.m?js/,
        type: "javascript/auto",
        resolve: {
          fullySpecified: false,
        },
      },
      {
        test: /\.react.svg$/,
        use: [
          {
            loader: "@svgr/webpack",
            options: {
              svgoConfig: {
                plugins: [{ removeViewBox: false }],
              },
            },
          },
        ],
      },
      { test: /\.json$/, loader: "json-loader" },
      {
        test: /\.s[ac]ss$/i,
        use: [
          // Creates `style` nodes from JS strings
          "style-loader",
          // Translates CSS into CommonJS
          "css-loader",
          // Compiles Sass to CSS
          "sass-loader",
        ],
      },
      {
        test: /\.(js|jsx)$/,
        exclude: /node_modules/,
        use: [
          {
            loader: "babel-loader",
            options: {
              presets: ["@babel/preset-react", "@babel/preset-env"],
              plugins: [
                "@babel/plugin-transform-runtime",
                "@babel/plugin-proposal-class-properties",
                "@babel/plugin-proposal-export-default-from",
              ],
            },
          },
          "source-map-loader",
        ],
      },
    ],
  },

  plugins: [
    new CleanWebpackPlugin(),
    new ModuleFederationPlugin({
      name: "login",
      filename: "remoteEntry.js",
      remotes: {
        studio: `studio@${combineUrl(proxyURL, "/remoteEntry.js")}`,
      },
      exposes: {
        "./app": "./src/Login.jsx",
      },
      shared: {
        ...deps,
        react: {
          singleton: true,
          requiredVersion: deps.react,
        },
        "react-dom": {
          singleton: true,
          requiredVersion: deps["react-dom"],
        },
      },
    }),
    new HtmlWebpackPlugin({
      template: "./public/index.html",
      publicPath: homepage,
      title: title,
      templateParameters: {
        proxyURL: proxyURL,
      },
      base: `${homepage}/`,
    }),
    new CopyPlugin({
      patterns: [
        {
          from: "public",
          globOptions: {
            dot: true,
            gitignore: true,
            ignore: ["**/index.ejs"],
          },
        },
      ],
    }),
  ],
};

module.exports = (env, argv) => {
  if (argv.mode === "production") {
    config.mode = "production";
    config.optimization = {
      splitChunks: { chunks: "all" },
      minimize: true,
      minimizer: [new TerserPlugin()],
    };
    config.plugins.push(
      new InjectManifest({
        mode: "production", //"development",
        swSrc: "@appserver/common/utils/sw-template.js", // this is your sw template file
        swDest: "sw.js", // this will be created in the build step
        exclude: [/\.map$/, /manifest$/, /service-worker\.js$/],
      })
    );
  } else {
    config.devtool = "cheap-module-source-map";
  }

  return config;
};
