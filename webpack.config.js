const path = require('path');

const isProduction = !process.argv.find(v => v.indexOf('watch') !== -1 || v.indexOf('serve') !== -1);
const isDevelopment = !isProduction && process.env.NODE_ENV !== 'production';

module.exports = {
  entry: './src/Program.fs.js',
  mode: isProduction ? 'production' : 'development',
  devtool: isDevelopment ? 'inline-source-map' : undefined,
  devServer: {
    static: './public',
  },
  output: {
    filename: '[name].bundle.js',
    path: path.resolve(__dirname, 'public'),
  },
};
