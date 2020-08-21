import '@babel/polyfill'

import Vue from 'vue'
import App from './app'
import router from './action/router.js'
import store from './store/index.js'

Vue.config.productionTip = false;

import Antd from 'ant-design-vue/lib'
import 'ant-design-vue/dist/antd.css'
Vue.use(Antd)

import Cache from './static/js/cache.js'
Vue.prototype.$cache = Cache

import Request from './action/request.js'
Vue.prototype.$axios = Request

import echarts from 'echarts'
Vue.prototype.$chart = echarts

Vue.prototype.$auth = store.getters
store.dispatch("init")

import 'jsoneditor/dist/jsoneditor.css'
import './static/css/components.css'


new Vue({
	router,
	store,
	render: h => h(App)
}).$mount("#app")
