<template>
	<div>
		<a-page-header title="设置" />
		<a-result status="warning" v-if="!login" title="无权访问此页面，请点击右上角登录按钮，登录后再访问" />
		<div class="ant-page-content" v-else>
			<a-card :tab-list="menu.data" :active-tab-key="menu.item" @tabChange="key => (menu.item = key)">
				<div v-if="menu.item === menu.data[1].key">
					<a-form
						:label-col="{
							xs: { span: 24 },
							sm: { span: 4 }
						}"
						:wrapper-col="{
							xs: { span: 24 },
							sm: { span: 20 }
						}"
					>
						<a-form-item label="浏览器头">
							<a-tooltip title="用于授权验证的浏览器头部参数名，如果访问数据包含此头部参数名，则此头部参数值将作为验证识别参数。多个信息用逗号间隔">
								<a-textarea autoSize allow-clear v-model="auth.header" />
							</a-tooltip>
						</a-form-item>
						<a-form-item label="请求参数">
							<a-tooltip title="用于授权验证的请求参数名，如果请求数据包含此参数名，则此参数值将作为验证识别参数。多个信息用逗号间隔">
								<a-textarea autoSize allow-clear v-model="auth.data" />
							</a-tooltip>
						</a-form-item>
						<a-form-item label="Cookies">
							<a-tooltip title="用于授权验证的Cookies参数名，如果Cookies数据包含此参数名，则此Cookies值将作为验证识别参数。多个信息用逗号间隔">
								<a-textarea autoSize allow-clear v-model="auth.cookies" />
							</a-tooltip>
						</a-form-item>
						<a-form-item
							:wrapper-col="{
								xs: { span: 24, offset: 0 },
								sm: { span: 20, offset: 4 }
							}"
						>
							<a-button type="primary" @click="saveAuth">保存设置</a-button>
						</a-form-item>
					</a-form>
					<a-divider>Token 参数设置（JWT 认证）</a-divider>
					<a-form
						:label-col="{
							xs: { span: 24 },
							sm: { span: 4 }
						}"
						:wrapper-col="{
							xs: { span: 24 },
							sm: { span: 20 }
						}"
					>
						<a-form-item label="字段名称">
							<a-tooltip
								title="用于检测认证的字段名，支持 Header 和 Cookies 检测。如果 Header 或 Cookies 中存在此字段，则此字段的值即为 Token；不存在则支持标准 JWT 认证，获取 Authorization 的 Bearer 值。"
							>
								<a-input v-model="config.TOKEN_NAME" />
							</a-tooltip>
						</a-form-item>
						<a-form-item label="加密密匙">
							<a-tooltip title="Token 值加密密匙，少于32位将自动生成"><a-input v-model="config.TOKEN_KEY" /></a-tooltip>
						</a-form-item>
						<a-form-item label="颁布者">
							<a-tooltip title="Token 颁发方名称"><a-input v-model="config.TOKEN_ISSUER" /></a-tooltip>
						</a-form-item>
						<a-form-item label="授予方">
							<a-tooltip title="Token 授予方名称"><a-input v-model="config.TOKEN_AUDIECNCE" /></a-tooltip>
						</a-form-item>
						<a-form-item
							:wrapper-col="{
								xs: { span: 24, offset: 0 },
								sm: { span: 20, offset: 4 }
							}"
						>
							<a-button type="primary" @click="saveToken">保存设置</a-button>
						</a-form-item>
					</a-form>
				</div>
				<div v-else-if="menu.item === menu.data[2].key">
					<a-form
						:label-col="{
							xs: { span: 24 },
							sm: { span: 4 }
						}"
						:wrapper-col="{
							xs: { span: 24 },
							sm: { span: 20 }
						}"
					>
						<a-form-item label="浏览器头">
							<a-tooltip title="用于缓存检测的浏览器头部参数名，如果访问数据包含此头部参数名，则此头部参数值将作为缓存识别参数。多个信息用逗号间隔">
								<a-textarea autoSize allow-clear v-model="cache.header" />
							</a-tooltip>
						</a-form-item>
						<a-form-item label="请求参数">
							<a-tooltip title="用于缓存检测的请求参数名，如果请求数据包含此参数名，则此参数值将作为缓存识别参数。多个信息用逗号间隔">
								<a-textarea autoSize allow-clear v-model="cache.data" />
							</a-tooltip>
						</a-form-item>
						<a-form-item label="Cookies">
							<a-tooltip title="用于缓存检测的Cookies参数名，如果Cookies数据包含此参数名，则此Cookies值将作为缓存识别参数。多个信息用逗号间隔">
								<a-textarea autoSize allow-clear v-model="cache.cookies" />
							</a-tooltip>
						</a-form-item>
						<a-form-item
							:wrapper-col="{
								xs: { span: 24, offset: 0 },
								sm: { span: 20, offset: 4 }
							}"
						>
							<a-button type="primary" @click="saveCache">保存设置</a-button>
						</a-form-item>
					</a-form>
				</div>
				<div v-else-if="menu.item === menu.data[3].key">
					<a-form
						:label-col="{
							xs: { span: 24 },
							sm: { span: 4 }
						}"
						:wrapper-col="{
							xs: { span: 24 },
							sm: { span: 20 }
						}"
					>
						<a-form-item label="日志级别">
							<a-select v-model="config.SYS_LOG_LEVEL">
								<a-select-option v-for="t in level" :value="t.key" :key="t.key">{{ t.value }}</a-select-option>
							</a-select>
						</a-form-item>
						<a-form-item label="控制台">
							<a-tooltip title="是否将日志信息展示到控制台界面上"><a-switch v-model="config.SYS_LOG_CONSOLE" /></a-tooltip>
						</a-form-item>
						<a-form-item label="保存日志">
							<a-tooltip title="是否将日志内容保存到本地文件"><a-switch v-model="config.SYS_LOG_SAVE" /></a-tooltip>
						</a-form-item>
						<a-form-item label="日志数量">
							<a-tooltip title="每次最多保留日志的记录数"><a-input-number v-model="config.SYS_LOG_SIZE" :min="10" /></a-tooltip>
						</a-form-item>
						<a-form-item label="详细错误">
							<a-tooltip title="是否将错误的具体信息记录到日志中"><a-switch v-model="config.SYS_LOG_STACKTRACE" /></a-tooltip>
						</a-form-item>
						<a-form-item label="访问统计">
							<a-tooltip title="是否开启访问统计概要，此统计每次重启系统将丢失，非系统的访问统计信息"><a-switch v-model="config.SYS_STATISTICS" /></a-tooltip>
						</a-form-item>
						<a-form-item
							:wrapper-col="{
								xs: { span: 24, offset: 0 },
								sm: { span: 20, offset: 4 }
							}"
						>
							<a-button type="primary" @click="saveLog">保存设置</a-button>
						</a-form-item>
					</a-form>
				</div>
				<div v-else>
					<a-form
						:label-col="{
							xs: { span: 24 },
							sm: { span: 4 }
						}"
						:wrapper-col="{
							xs: { span: 24 },
							sm: { span: 20 }
						}"
					>
						<a-form-item label="统计间隔">
							<a-tooltip title="访问统计间隔时段，即？分钟内的访问作为一个时段综合统计，仅允许：1，2，3，4，5，6，10，15，20，30，60。效果请查看首页的时段统计"><a-input-number v-model="config.COUNTER_STATUS_DELAY" :min="1" /></a-tooltip>
						</a-form-item>
						<a-form-item label="统计时长">
							<a-tooltip title="统计最近？小时的访问数据。效果请查看首页的时段统计"><a-input-number v-model="config.COUNTER_STATUS_BEFORE" :min="1" /></a-tooltip>
						</a-form-item>
						<a-form-item label="记录间隔">
							<a-tooltip title="记录系统CPU，内存，访问频率等数据的间隔时间，单位：秒"><a-input-number v-model="config.COUNTER_SYSTEM_DELAY" :min="1" /></a-tooltip>
						</a-form-item>
						<a-form-item
							:wrapper-col="{
								xs: { span: 24, offset: 0 },
								sm: { span: 20, offset: 4 }
							}"
						>
							<a-button type="primary" @click="saveOther">保存设置</a-button>
						</a-form-item>
					</a-form>
				</div>
			</a-card>
		</div>
	</div>
</template>

<script>
export default {
	data() {
		return {
			login: this.$auth.isAuth,

			menu: {
				data: [
					{
						key: 'sys',
						tab: '系统设置'
					},
					{
						key: 'auth',
						tab: '授权设置'
					},
					{
						key: 'cache',
						tab: '缓存设置'
					},
					{
						key: 'log',
						tab: '日志设置'
					}
				],
				item: 'sys'
			},

			config: {},

			level: [
				{ key: 1, value: '记录' },
				{ key: 2, value: '调试' },
				{ key: 4, value: '信息' },
				{ key: 8, value: '警告' },
				{ key: 16, value: '错误' },
				{ key: 32, value: '严重异常' },
				{ key: 64, value: '禁止记录' }
			],
			cache: {
				header: '',
				cookies: '',
				data: ''
			},
			auth: {
				header: '',
				cookies: '',
				data: ''
			}
		};
	},
	created() {
		this.loadConfig();
	},
	methods: {
		loadConfig() {
			this.$axios.get('_gateway/config/list').then(res => {
				if (res && res.Data) {
					this.config = res.Data;

					this.cache.header = this.config.CACHE_HEADER ? this.config.CACHE_HEADER.join(',') : '';
					this.cache.data = this.config.CACHE_DATA ? this.config.CACHE_DATA.join(',') : '';
					this.cache.cookies = this.config.CACHE_COOKIES ? this.config.CACHE_COOKIES.join(',') : '';

					this.auth.header = this.config.AUTH_HEADER ? this.config.AUTH_HEADER.join(',') : '';
					this.auth.data = this.config.AUTH_DATA ? this.config.AUTH_DATA.join(',') : '';
					this.auth.cookies = this.config.AUTH_COOKIES ? this.config.AUTH_COOKIES.join(',') : '';
				}
			});
		},
		saveLog() {
			this.$axios.post('_gateway/config/savelog', { item: this.config }).then(res => {
				if (res) {
					if (res.Data) {
						this.$notification.success({
							message: '更新成功',
							description: '日志参数已经更新'
						});

						this.loadConfig();
					} else {
						this.$notification.warn({
							message: '更新失败',
							description: '更新参数时发生异常，请稍后再试'
						});
					}
				}
			});
		},
		saveCache() {
			const Header = this.cache.header ? this.cache.header.split(',') : [];
			const Data = this.cache.data ? this.cache.data.split(',') : [];
			const Cookies = this.cache.cookies ? this.cache.cookies.split(',') : [];

			this.$axios.post('_gateway/config/savecache', { Header, Data, Cookies }).then(res => {
				if (res) {
					if (res.Data) {
						this.$notification.success({
							message: '更新成功',
							description: '缓存参数已经更新'
						});

						this.loadConfig();
					} else {
						this.$notification.warn({
							message: '更新失败',
							description: '更新参数时发生异常，请稍后再试'
						});
					}
				}
			});
		},
		saveAuth() {
			const Header = this.auth.header ? this.auth.header.split(',') : [];
			const Data = this.auth.data ? this.auth.data.split(',') : [];
			const Cookies = this.auth.cookies ? this.auth.cookies.split(',') : [];

			this.$axios.post('_gateway/config/saveauth', { Header, Data, Cookies }).then(res => {
				if (res) {
					if (res.Data) {
						this.$notification.success({
							message: '更新成功',
							description: '授权参数已经更新'
						});

						this.loadConfig();
					} else {
						this.$notification.warn({
							message: '更新失败',
							description: '更新参数时发生异常，请稍后再试'
						});
					}
				}
			});
		},
		saveToken() {
			this.$axios.post('_gateway/config/savetoken', { item: this.config }).then(res => {
				if (res) {
					if (res.Data) {
						this.$notification.success({
							message: '更新成功',
							description: 'Token参数已经更新'
						});

						this.loadConfig();
					} else {
						this.$notification.warn({
							message: '更新失败',
							description: '更新参数时发生异常，请稍后再试'
						});
					}
				}
			});
		},
		saveOther() {
			this.$axios.post('_gateway/config/saveother', { item: this.config }).then(res => {
				if (res) {
					if (res.Data) {
						this.$notification.success({
							message: '更新成功',
							description: '系统参数已经更新'
						});

						this.loadConfig();
					} else {
						this.$notification.warn({
							message: '更新失败',
							description: '更新参数时发生异常，请稍后再试'
						});
					}
				}
			});
		}
	}
};
</script>

<style></style>
