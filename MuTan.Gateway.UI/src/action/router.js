import Vue from 'vue'
import VueRouter from 'vue-router'

Vue.use(VueRouter)

const router = new VueRouter({
	mode: 'hash',
	routes: [{
		path: '/',
		name: 'index',
		component: () => import('../pages/index')
	}, {
		path: '/client',
		name: 'client',
		component: () => import('../pages/client')
	}, {
		path: '/cache',
		name: 'cache',
		component: () => import('../pages/cache')
	}, {
		path: '/log',
		name: 'log',
		component: () => import('../pages/log')
	}, {
		path: '/plugin',
		name: 'plugin',
		component: () => import('../pages/plugin')
	}, {
		path: '/rewrite',
		name: 'rewrite',
		component: () => import('../pages/rewrite')
	}, {
		path: '/route',
		name: 'route',
		component: () => import('../pages/route')
	}, {
		path: '/server',
		name: 'server',
		component: () => import('../pages/server')
	}, {
		path: '/config',
		name: 'config',
		component: () => import('../pages/config')
	}]
})

export default router
























// 	let router = new Router(routerParam);

// 	// router.beforeEach((to, from, next) => {
// 	// 	HeyUI.$LoadingBar.start();
// 	// 	if (to.meta && to.meta.title) {
// 	// 		document.title = to.meta.title + ' - 管理应用';
// 	// 	} else {
// 	// 		document.title = '管理系统';
// 	// 	}
// 	// 	next();
// 	// });
// 	router.afterEach(() => {
// 		// HeyUI.$LoadingBar.success();
// 		document.documentElement.scrollTop = 0;
// 		document.body.scrollTop = 0;
// 		let layoutContent = document.querySelector('#_top');
// 		if (layoutContent) {
// 			layoutContent.scrollTop = 0;
// 		}
// 		console.log('-----------------------------', layoutContent, 'layoutContent');
// 		// baidu 统计，如果有自己的统计，请至index.html修改至自己的埋点
// 		if (window._hmt) {
// 			window._hmt.push(['_trackPageview', window.location.pathname]);
// 		}
// 	});
// 	return router;
// };

// mode: hash / history

// const router = new Router({
// 	mode: 'hash',
// 	routes: [{
// 		path: '/',
// 		name: 'Home',
// 		component: _Home,
// 		meta: {
// 			title: "",
// 			auth: true
// 		}
// 	}]
// })

// router.beforeEach((to, from, next) => {
// 	if (to.meta.title) {
// 		document.title = to.meta.title
// 	}

// 	// 判断该路由是否需要登录权限
// 	if (to.meta.auth) {
// 		if (Utils.Store.get('token')) {
// 			next()
// 		} else {
// 			next('/login')
// 		}
// 	} else {
// 		next()
// 	}
// })

// router.afterEach(() => {
// 	// // 刷新页面，返回顶部
// 	// Utils.Goto('_top')

// 	// // 页面访问统计
// 	// Utils.Record.view()
// });
