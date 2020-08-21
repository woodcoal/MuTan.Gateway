<template>
	<div>
		<a-page-header title="插件" />
		<a-result status="warning" v-if="!auth" title="无权访问此页面，请点击右上角登录按钮，登录后再访问" />
		<a-row class="ant-page-content" :gutter="[16, 16]" v-else>
			<a-col :lg="10">
				<a-card>
					<a-list :data-source="plugins">
						<a-list-item slot="renderItem" slot-scope="item, index">
							<a @click="switchPlugin(index)">
								<a-list-item-meta>
									<span slot="title">{{ item.Name }}</span>
									<span slot="description">{{ item.Description }}</span>
									<a-avatar size="large" slot="avatar" :style="item.Enabled ? { backgroundColor: 'green' } : { backgroundColor: 'gray' }">
										{{ index + 1 }}
									</a-avatar>
								</a-list-item-meta>
							</a>
							<div slot="actions">Ver: {{ item.Version }}</div>
						</a-list-item>
					</a-list>
				</a-card>
			</a-col>
			<a-col :lg="14" v-if="plugin">
				<a-card>
					<a-switch v-model="plugin.Enabled" @change="enabledPlugin" slot="extra" v-if="plugin.Status" />
					<h3 slot="title">插件： {{ plugin.Name }}</h3>
					<div :class="plugin.Enabled ? 'enabled' : 'disabled'">
						<p>
							<strong>级别：</strong>
							{{ plugin.Level }}
						</p>
						<p>
							<strong>类型：</strong>
							{{ plugin.Type }}
						</p>
						<p>
							<strong>版本：</strong>
							{{ plugin.Version }}
						</p>
						<p>
							<strong>Assembly：</strong>
							{{ plugin.Assembly }}
						</p>
						<p>
							<strong>描述：</strong>
							{{ plugin.Description }}
						</p>
						<p>
							<strong>版权：</strong>
							{{ plugin.Copyright }}
						</p>

						<template v-if="plugin.EnSetting">
							<a-divider>插件参数</a-divider>
							<p><JsonEditor v-model="plugin.Setting" /></p>
							<p><a-button @click="savePlugin" type="primary">保存参数</a-button></p>
						</template>
					</div>
				</a-card>
			</a-col>
		</a-row>
	</div>
</template>

<script>
import xSwitch from '../components/editor/switch.vue';
import JsonEditor from '../components/jsonEditor.vue';

export default {
	components: { xSwitch, JsonEditor },
	data() {
		return {
			auth: this.$auth.isAuth,
			plugins: [],
			plugin: null,
			pluginIndex: -1
		};
	},
	mounted() {
		this.loadPlugin(0);
	},
	methods: {
		loadPlugin(pluginIndex) {
			this.plugins = [];
			
			this.$axios.get('_gateway/plugin/list').then(res => {
				if (res && res.Data) {
					this.plugins = res.Data;
					this.switchPlugin(pluginIndex);
				}
			});
		},

		switchPlugin(pluginIndex) {
			if (this.plugins && this.plugins.length > pluginIndex && pluginIndex >= 0) {
				this.pluginIndex = pluginIndex;

				
				this.$axios.post('_gateway/plugin/info', { name: this.plugins[pluginIndex].Name }).then(res => {
					if (res && res.Data) {
						this.plugin = res.Data;
					}
				});
			} else {
				this.pluginIndex = -1;
				this.plugin = null;
			}
		},

		enabledPlugin() {
			if (!this.plugin) return;

			
			this.$axios.post('_gateway/plugin/switch', { name: this.plugin.Name, enabled: this.plugin.Enabled }).then(res => {
				if (res && res.Data) {
					this.$notification.success({
						message: '切换状态',
						description: '切换插件状态成功'
					});

					// 刷新状态
					this.loadPlugin(this.pluginIndex);
				} else {
					this.$notification.warn({
						message: '切换失败',
						description: '插件状态切换错误，请稍后再试'
					});
				}
			});
		},

		savePlugin() {
			if (!this.plugin) return;

			
			this.$axios.post('_gateway/plugin/save', { name: this.plugin.Name, setting: this.plugin.Setting }).then(res => {
				if (res && res.Data) {
					this.$notification.success({
						message: '保存参数',
						description: '插件参数保存成功'
					});

					// 刷新状态
					this.loadPlugin(this.pluginIndex);
				} else {
					this.$notification.warn({
						message: '保存失败',
						description: '插件参数保存失败，请稍后再试'
					});
				}
			});
		}
	}
};
</script>

<style scoped>
.enabled {
	cursor: auto;
}
.enabled * {
	pointer-events: auto;
}
.disabled {
	cursor: not-allowed;
	opacity: 0.6;
}
.disabled * {
	pointer-events: none;
}
</style>
