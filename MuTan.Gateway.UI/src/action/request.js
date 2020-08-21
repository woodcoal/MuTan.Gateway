/**
 * axios封装
 * 请求拦截、响应拦截、错误统一处理
 */
import axios from 'axios';
import qs from 'qs';
import {
	notification
} from 'ant-design-vue';
import store from '../store/index.js';
import router from './router.js';
import config from '../config/system.js';

const DefaultParam = {
	repeatable: false
};

const URLS = {
	list: new Set(),

	_update: function(url) {
		return url ? url.split('?')[0] : '';
	},
	has: function(url) {
		let api = this._update(url);
		return this.list.has(api);
	},
	add: function(url) {
		let api = this._update(url);
		if (!this.has) this.list.add(api);
	},
	delete: function(url) {
		let api = this._update(url);
		if (this.has) this.list.delete(api);
	}
};

/**
 * 请求失败后的错误统一处理
 * @param {Number} status 请求失败的状态码
 */
const errorHandle = (status, data) => {
	const code = parseInt(status);

	if (code === 403 || code === 401) {
		// 账号密码错误
		notification.warn({
			message: '无权限',
			description: '你的登录信息已经过期，请重新登录后再操作'
		});

		router.push('/');
	} else {
		// 其他错误
		notification.error({
			message: '操作异常',
			description: '操作发生异常：' + data ? '(' + code + ')' + data : '代码 ' + code
		});
	}
};

const ajax = function(param, extendParam) {
	let params = { ...DefaultParam,
		...param,
		...extendParam
	};
	params.crossDomain = params.url.indexOf('http') === 0;

	let url = params.url;
	if (!params.crossDomain) {
		if (config.Server) {
			if (!config.Server.endsWith('/')) config.Server += '/';
			if (params.url.startsWith("/")) params.url = params.url.substr(1);
		}

		url = params.url = config.Server + params.url;
	}

	if (params.method != 'GET') {
		if (URLS.has(url)) {
			return new Promise((resolve, reject) => {
				resolve({
					code: -1,
					message: '重复请求'
				});
			});
		}
		if (params.repeatable === false) {
			URLS.add(url);
		}
	}

	let header = {
		Authorization: ''
	};

	const token = store.getters.token;
	if (token) {
		header.Authorization = 'Bearer ' + token;
	}

	let defaultParam = {
		headers: header,
		responseType: 'json',
		validateStatus: function(status) {
			return true;
		},
		paramsSerializer: params => {
			return qs.stringify(params, {
				allowDots: true
			});
		}
	};

	if (params.isUpload) {
		params.paramsSerializer = null;
		params.data = extendParam;
	}

	params = { ...defaultParam,
		...params
	};

	// axios.defaults.withCredentials = true
	// axios.defaults.crossDomain = true

	//axios.defaults.headers['Content-Type']="application/json;charset=UTF-8"
	return new Promise((resolve, reject) => {
		return axios
			.request(params)
			.then(response => {
				// console.log('请求成功', response);

				URLS.delete(url);
				if (response.status < 300 && response.status >= 200) {
					resolve(response.data);
				} else {
					errorHandle(response.status, response.data);
					reject(response);
				}
			})
			.catch(error => {
				// console.log('网络异常', error);

				const {
					response
				} = error;
				if (response) {
					// 请求已发出，但是不在2xx的范围
					errorHandle(response.status, response.data);
					reject(response);
				} else {
					// 处理断网的情况
					// eg:请求超时或断网时，更新state的network状态
					// network状态在app.vue中控制着一个全局的断网提示组件的显示隐藏
					// 关于断网组件中的刷新重新获取数据，会在断网组件中说明
					if (!window.navigator.onLine) {
						notification.error({
							message: '您已离线',
							description: '您已经离线，请检查您的网络是否畅通！'
						});
					} else {
						reject(error);
						notification.error({
							message: '网络异常',
							description: '您网络发生异常 ' + error.toString()
						});
					}
				}
			});
	});
};

const get = function(url, param, extendParam) {
	return ajax({
			url,
			method: 'GET',
			params: param
		},
		extendParam
	);
};

const post = function(url, param, extendParam) {
	return ajax({
			url,
			method: 'POST',
			data: param
		},
		extendParam
	);
};

const postJson = function(url, paramJson, extendParam) {
	return ajax({
			url,
			method: 'POST',
			data: paramJson
		},
		extendParam
	);
};

const put = function(url, param, extendParam) {
	return ajax({
			url,
			method: 'PUT',
			data: param
		},
		extendParam
	);
};

const putJson = function(url, paramJson, extendParam) {
	return ajax({
			url,
			method: 'PUT',
			data: paramJson
		},
		extendParam
	);
};

const del = function(url, param, extendParam) {
	return ajax({
			url,
			method: 'DETETE',
			params: param
		},
		extendParam
	);
};

const upload = function(url, param) {
	return ajax({
			url: url,
			method: 'POST',
			isUpload: true
		},
		param
	);
};

export default {
	ajax,
	get,
	post,
	put,
	del,
	postJson,
	putJson,
	upload,
	$get: get,
	$post: post
};
