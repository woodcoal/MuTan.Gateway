const path = require('path');

module.exports = {
	port: 8000, //端口号
	dist: 'dist', //生成文件的根目录
	timestamp: false, //build生成的static文件夹是否添加时间戳
	clean: true, //打包之前清空dist目录
	react: false, //支持react项目
	openBrowser: true, // 自动打开浏览器
	stat: false,

	webpack: {
		console: true, //打包压缩是否保留console，默认为false
		sourceMap: false, //打包的时候要不要保留sourceMap, 默认为false
		compress: true, // 默认值取决于是开发还是打包，设置值会影响打包的时候是否压缩js代码
		publicPath: "/", //公开path

		//输出哪些文件，主要是html，默认会加载和html文件名一样的js文件为入口。支持定义公用包。
		output: {
			"./index.html": {
				name: 'app',
				entry: './src/main' //默认加载js文件，并且html自动引用。如果没有配置，则自动加载与html文件名同样的js文件。
			}
		},

		//定义resolve，https://webpack.js.org/configuration/resolve/
		alias: {
			// 你可以使用 import index from 'components/index'  => src/components/index
			components: './src/components/',
			pages: './src/pages/',
			assets: './src/assets/',
		},
		//定义全局变量, https://webpack.js.org/plugins/provide-plugin
		global: {
			Config: [path.resolve(__dirname, 'src/config'), 'default'],
			Utils: [path.resolve(__dirname, 'src/utils'), 'default'],
			Request: [path.resolve(__dirname, 'src/action/request'), 'default']
		},
		devServer: {
			// proxy: {
			// 	// 此处应该配置为开发服务器的后台地址
			// 	'/_gateway': {
			// 		target: 'http://api.xiongdi.org:49101/'
			// 		//target: 'http://localhost/'
			// 	}
			// },
			// historyApiFallback: true
		},
		externals: {}
	},

	copy: []
};
