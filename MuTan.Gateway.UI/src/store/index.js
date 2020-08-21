import Vue from 'vue'
import Vuex from 'vuex'

import axios from '../action/request.js'
import cache from '../static/js/cache.js'

Vue.use(Vuex)

const store = new Vuex.Store({
	state: {
		token: '',
		user: '',
		exp: 0,
		extTime: 1800000, //30分钟，单位：毫秒 30 * 60 * 1000
	},
	mutations: {
		setAuth(state, {
			token,
			user
		}) {
			state.token = token;
			state.user = user || 'U/A';
			state.exp = new Date().getTime() + state.extTime;
		},
		clearAuth(state) {
			state.token = '';
			state.user = '';
			state.exp = 0;
		}
	},
	getters: {
		isAuth(state) {
			return new Date().getTime() <= state.exp && !!state.token;
		},
		token(state) {
			return new Date().getTime() <= state.exp ? state.token : '';
		},
		user(state) {
			return new Date().getTime() <= state.exp ? (state.token ? state.user : '') : '';
		}
	},
	actions: {
		// 启动时初始化，将缓存数据读到 stats
		init(context) {
			context.commit('setAuth', {
				token: cache.get('_client_token'),
				user: cache.get('_client_user')
			});

			context.dispatch('refresh');
		},

		// 登录
		async login(context, {
			name,
			key
		}) {
			let user = null;
			let token = null;

			await axios
				.get('_gateway/login', {
					name,
					key
				})
				.then(res => {
					if (res && res.Data) {
						// 登录成功，缓存 1 小时
						user = res.Data.Name;
						token = res.Data.Token;
					}
				}).catch(res => {
					if (res && res.status == 400) {
						return false;
					}
				});

			if (user && token) {
				context.commit('setAuth', {
					token,
					user
				});
				cache.set('_client_token', token, context.state.extTime);
				cache.set('_client_user', user, context.state.extTime);

				return true;
			} else {
				context.commit('clear');
				cache.remove('_client_token');
				cache.remove('_client_user');

				return false;
			}
		},

		// 定时刷新登录状态
		refresh(context) {
			const reload = function() {
				// 未登录，不执行
				if (!context.getters.isAuth) return;

				// 更新状态
				axios.$get('_gateway/refresh').then(res => {
					let user = '';
					let token = '';

					if (res && res.Data) {
						user = res.Data.Name || '';
						token = res.Data.Token || '';
					}

					context.commit('setAuth', {
						token,
						user
					});
					cache.set('_client_token', token, context.state.extTime);
					cache.set('_client_user', user, context.state.extTime);
				}).catch(res => {
					context.commit('clear');
					cache.remove('_client_token');
					cache.remove('_client_user');
				});
			};

			reload();

			setInterval(reload, context.state.extTime);
		},

		// 注销
		logout(context) {
			context.commit('clearAuth');
			cache.remove('_client_token');
			cache.remove('_client_user');
		}
	}
});

export default store
