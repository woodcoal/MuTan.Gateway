<template>
	<a-layout id="appLayout">
		<a-layout-sider breakpoint="md" :collapsed-width="sider.width" @breakpoint="updateSiderWidth" v-model="sider.collapsed" :trigger="null" collapsible width="240">
			<div class="logo"><img :src="sider.logo" /></div>
			<a-menu mode="inline" theme="dark" :inline-collapsed="sider.collapsed">
				<template v-for="(menu, i) in menus">
					<a-menu-item :key="i" :title="menu.title" v-if="!menu.childs" @click="openMenu(menu)">
						<a-icon :type="menu.icon || 'appstore'" />
						<span>{{ menu.title }}</span>
					</a-menu-item>
					<a-sub-menu :key="i" v-else>
						<span slot="title">
							<a-icon :type="menu.icon || 'appstore'" />
							<span>{{ menu.title }}</span>
						</span>
						<a-menu-item v-for="(sub, j) in menu.childs" :key="i + '_' + j" :title="sub.title" @click="openMenu(sub)">
							<a-icon :type="sub.icon || 'appstore'" />
							<span>{{ sub.title }}</span>
						</a-menu-item>
					</a-sub-menu>
				</template>
			</a-menu>
		</a-layout-sider>
		<a-layout>
			<a-layout-header style="background-color: #ffffff;padding: 0;">
				<img :src="head.logo" class="logo" v-if="!head.hide" />
				<a-icon class="trigger" :type="sider.collapsed ? 'menu-unfold' : 'menu-fold'" @click="collapsedSide" />
				<div class="right">
					<div v-if="user">
						<a-avatar :title="user">{{ user }}</a-avatar>
						<a-divider type="vertical" />
						<a-button icon="logout" @click="logout">注销</a-button>
					</div>
					<div v-else>
						<a-dropdown placement="bottomRight">
							<a-button icon="login">登录</a-button>
							<a-form :form="form" style="padding: 16px;" @submit="login" slot="overlay">
								<a-form-item>
									<a-input
										addon-before="账号:"
										v-decorator="['name', { rules: [{ required: true, min: 3, max: 24, message: '账号只能为 3~24 个字符' }] }]"
										placeholder="账号"
									>
										<a-icon slot="prefix" type="user" style="color: rgba(0,0,0,.25)" />
									</a-input>
								</a-form-item>
								<a-form-item>
									<a-input
										addon-before="密匙:"
										v-decorator="['key', { rules: [{ required: true, min: 36, max: 36, message: '密匙为36位字符' }] }]"
										type="password"
										placeholder="密匙"
									>
										<a-icon slot="prefix" type="lock" style="color: rgba(0,0,0,.25)" />
									</a-input>
								</a-form-item>
								<div><a-button type="primary" html-type="submit" icon="login">登录</a-button></div>
							</a-form>
						</a-dropdown>
					</div>
				</div>
			</a-layout-header>
			<a-layout-content><router-view /></a-layout-content>
			<a-layout-footer>Copyright</a-layout-footer>
		</a-layout>
	</a-layout>
</template>

<script>
import Logo from '../static/logo/logo.png';
import LogoA from '../static/logo/logoa.png';
import LogoB from '../static/logo/logob.png';

import Menus from '../config/menu.js';

export default {
	data() {
		return {
			sider: {
				collapsed: false,
				width: 0,
				logo: Logo
			},
			head: {
				logo: LogoA,
				hide: true
			},
			user: this.$auth.user,
			form: this.$form.createForm(this, { name: 'login' }),
			menus: Menus,
			delay: 1800000 // 30分钟
		};
	},
	methods: {
		// 更新侧边栏收缩宽度
		updateSiderWidth() {
			this.sider.width = document.documentElement.clientWidth > 768 ? 80 : 0;
			this.sider.logo = document.documentElement.clientWidth > 768 ? Logo : LogoB;
			this.head.hide = document.documentElement.clientWidth > 768;
		},

		collapsedSide() {
			this.updateSiderWidth();
			this.sider.collapsed = !this.sider.collapsed;
			this.sider.logo = this.sider.collapsed ? LogoB : Logo;
			if (document.documentElement.clientWidth <= 768) {
				this.head.hide = !this.sider.collapsed;
			} else {
				this.head.hide = true;
			}
		},

		// 打开菜单项目
		openMenu(menu) {
			if (!menu) return;
			if (menu.nav) {
				if (this.$route.path !== menu.nav) this.$router.push(menu.nav);
			} else {
				location.href = menu.link;
			}
		},

		// 登录
		login(e) {
			e.preventDefault();
			this.form.validateFields((err, values) => {
				// values.name = 'manager';
				// values.key = '99f68d2e-e496-4456-80ee-525090281af8';
				// err = false;

				if (!err) {
					this.$store.dispatch('login', values).then(ret => {
						if (ret) {
							// 登录成功
							this.$notification.success({
								message: '登录成功',
								description: '欢迎使用本系统!'
							});

							location.reload();
						} else {
							// 未获取到 token
							this.$notification.error({
								message: '登录失败',
								description: '登录失败，账号密码不匹配，未获取到有效的登录凭证！'
							});
						}
					});
				}
			});
		},

		// 注销
		logout() {
			this.$store.dispatch('logout');

			if (this.$route.path !== '/') this.$router.push('/');
			//location.reload();
		}
	},
	watch: {
		'$auth.user': function(value) {
			this.user = value;
		}
	}
};
</script>

<style>
.ant-layout,
.ant-layout-sider {
	height: 100vh;
	overflow: hidden;
}
.ant-layout-sider {
	box-shadow: 2px 0 6px rgba(0, 21, 41, 0.35);
	z-index: 999;
}
.ant-layout-sider .logo {
	text-align: center;
	margin: 10px;
}

.ant-layout-header {
	background-color: #ffffff;
	padding: 0;
}
.ant-layout-header .logo {
	padding-left: 10px;
}
.ant-layout-header .right {
	float: right;
	height: 100%;
	margin-left: auto;
	overflow: hidden;
	margin-right: 0.5rem;
}

.ant-layout-header .trigger {
	font-size: 18px;
	line-height: 64px;
	padding: 0 24px;
	cursor: pointer;
	transition: color 0.3s;
}
.ant-layout-header .trigger:hover {
	color: #1890ff;
}

.ant-layout-content {
	overflow-y: scroll;
	overflow-x: hidden;
}
.ant-layout-content .ant-page-header {
	background-color: #ffffff;
	margin: 3px 0;
}
.ant-layout-content .ant-page-content {
	padding: 16px;
}

.ant-layout-footer {
	text-align: center;
}
</style>
