// @ts-check
"use strict";
const path = require('path');
const { DefinePlugin } = require('webpack');
const { GenerateSW } = require('workbox-webpack-plugin');

const isProduction = !process.argv.find(v => v.indexOf('watch') !== -1 || v.indexOf('serve') !== -1);
const isDevelopment = !isProduction && process.env.NODE_ENV !== 'production';

/** @type {import('webpack').Configuration} */
module.exports = {
  entry: './src/Program.fs.js',
  mode: isProduction ? 'production' : 'development',
  devtool: isDevelopment ? 'inline-source-map' : undefined,
  devServer: {
    static: './public',
    hot: true
  },
  output: {
    filename: '[name].bundle.js',
    path: path.resolve(__dirname, 'public'),
  },
  plugins: [
    new DefinePlugin({
      'process.env.NODE_ENV': JSON.stringify(isProduction && "production"),
      'process.env.PUBLIC_URL': JSON.stringify('.'),
    }),
    isProduction ? new GenerateSW({
      swDest: 'sw.js',
      sourcemap: false
    }) : (_ => undefined)
  ]
};
