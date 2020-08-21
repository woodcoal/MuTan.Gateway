<template>
	<div>
		<a-page-header title="路由" />
		<a-result status="warning" v-if="!auth" title="无权访问此页面，请点击右上角登录按钮，登录后再访问" />
		<a-row class="ant-page-content" :gutter="[16, 16]" v-else>
			<a-col :lg="10">
				<a-card>
					<a-form layout="inline" slot="title" :form="formRoute" @submit="addRoute">
						<a-form-item>
							<a-input placeholder="路由前缀" v-decorator="['url', { rules: [{ required: true, min: 3, message: '请输入有效的路由前缀' }] }]" type="text">
								<a-icon slot="prefix" type="link" />
								<a-tooltip
									slot="suffix"
									title="路由前缀，可以使用正则表达式。如：abc.com@/.* 或者 /api/.*；如果使用 域名@地址 格式则表示支持泛域名匹配，系统会先匹配域名，然后再匹配地址。如：xxx.abc.com@^.*；同时系统也支持 Header / Data / Cookies 校验，需要在系统设置中开启"
								>
									<a-icon type="info-circle" style="color: rgba(0,0,0,.45)" />
								</a-tooltip>
							</a-input>
						</a-form-item>
						<a-form-item><a-button html-type="submit">添加</a-button></a-form-item>
					</a-form>
					<a-list :data-source="routes">
						<a-list-item slot="renderItem" slot-scope="item, index">
							<a @click="switchRoute(index)">
								<a-list-item-meta>
									<span slot="title">{{ item.Url }}</span>
									<span slot="description">
										Server: {{ item.Servers.length }}
										<a-divider type="vertical" />
										MaxRps: {{ item.MaxRps }}
										<a-divider type="vertical" />
										TimeOut: {{ item.TimeOut }}
									</span>
									<a-avatar size="large" slot="avatar">{{ index + 1 }}</a-avatar>
								</a-list-item-meta>
							</a>
							<div slot="extra" v-if="item.Url !== '*'">
								<a-popconfirm title="确定删除此服务器吗？" ok-text="确定" cancel-text="取消" @confirm="delRoute(item.Url)">
									<a><a-icon style="color:red" type="delete" /></a>
								</a-popconfirm>
							</div>
						</a-list-item>
					</a-list>
				</a-card>
			</a-col>
			<a-col :lg="14" v-if="route">
				<a-card>
					<h3 slot="title">路由： {{ route.Url }}</h3>
					<div slot="extra" v-if="routeHasEdit">
						<a-button type="primary" icon="undo" @click="switchRoute(routeIndex)">重置参数</a-button>
						<a-button type="danger" icon="save" @click="saveRoute">上传保存</a-button>
					</div>
					<a-alert
						message="路由参数已经被修改，请点击右上角 [上传保存] 按钮，以便将修改后的数据上传到后台，上传后的数据将实时生效！"
						type="info"
						style="margin-bottom: 16px;"
						showIcon
						v-if="routeHasEdit"
					/>

					<a-descriptions title="基本信息" :column="{ xl: 3, lg: 2, md: 2, sm: 1, xs: 1 }" layout="vertical" bordered>
						<a-descriptions-item label="最大连接数"><xNumber v-model="route.MaxRps" :min="0" @change="routeHasEdit = true" /></a-descriptions-item>
						<a-descriptions-item label="超时"><xNumber v-model="route.TimeOut" :min="0" @change="routeHasEdit = true" /></a-descriptions-item>
						<a-descriptions-item label="一致性校验">
							<xText v-model="route.HashPattern" defaultValue="暂无" @change="routeHasEdit = true" />
						</a-descriptions-item>
						<a-descriptions-item label="备注"><xText v-model="route.Remark" defaultValue="暂无" @change="routeHasEdit = true" /></a-descriptions-item>
					</a-descriptions>
					<a-divider />
					<a-descriptions title="跨域参数" :column="{ xxl: 4, xl: 3, lg: 2, md: 2, sm: 1, xs: 1 }" layout="vertical" bordered>
						<a-descriptions-item label="Allow Origin">
							<xText v-model="route.AccessControlAllowOrigin" defaultValue="暂无" @change="routeHasEdit = true" />
						</a-descriptions-item>
						<a-descriptions-item label="Allow Methods">
							<xText v-model="route.AccessControlAllowMethods" defaultValue="暂无" @change="routeHasEdit = true" />
						</a-descriptions-item>
						<a-descriptions-item label="Allow Headers">
							<xText v-model="route.AccessControlAllowHeaders" defaultValue="暂无" @change="routeHasEdit = true" />
						</a-descriptions-item>
						<a-descriptions-item label="MaxAge"><xNumber v-model="route.AccessControlMaxAge" :min="0" @change="routeHasEdit = true" /></a-descriptions-item>
						<a-descriptions-item label="Allow Credentials"><xSwitch v-model="route.AccessControlAllowCredentials" @change="routeHasEdit = true" /></a-descriptions-item>
						<a-descriptions-item label="Vary"><xText v-model="route.Vary" defaultValue="暂无" @change="routeHasEdit = true" /></a-descriptions-item>
					</a-descriptions>
					<a-divider />
					<a-descriptions>
						<template slot="title">
							绑定服务器
							<a-button type="primary" shape="circle" size="small" style="margin-left: 16px;" @click="showServers">＋</a-button>
						</template>
					</a-descriptions>
					<a-table :columns="columns" :data-source="route.Servers" :pagination="false" :rowKey="record => record.Host">
						<template slot="host" slot-scope="text, record">
							<a-icon type="check-circle" style="color:#008000" v-if="record.Available" />
							<a-icon type="exclamation-circle" style="color:#e00" v-else />
							<strong style="margin-left: 8px;">{{ record.Host }}</strong>
						</template>

						<xNumber slot="weight" slot-scope="text, record" v-model="text" :min="0" @change="routeHasEdit = true" />
						<xNumber slot="maxrps" slot-scope="text, record" v-model="text" :min="1" @change="routeHasEdit = true" />
						<xSwitch slot="standby" slot-scope="text, record" v-model="text" @change="routeHasEdit = true" />

						<div slot="action" slot-scope="text, record">
							<a @click="delServer(record.Host)"><a-icon style="color:red" type="delete" /></a>
						</div>
					</a-table>
				</a-card>
			</a-col>
		</a-row>

		<a-modal v-model="serverModel" title="服务器列表" :footer="null">
			<a-list :data-source="servers">
				<a-list-item slot="renderItem" slot-scope="item, index">
					<a-list-item-meta>
						<span slot="title">{{ item.Host }}</span>
						<span slot="description">{{ item.Remark }}</span>
						<a-avatar size="large" slot="avatar" :style="{ backgroundColor: item.Available ? '#008000' : '#CC0000' }">{{ index + 1 }}</a-avatar>
					</a-list-item-meta>
					<div slot="actions" :style="{ color: item.Available ? '#008000' : '#CC0000' }">{{ item.Available ? '正常' : '故障' }}</div>
					<div slot="extra">
						<a-button type="primary" shape="circle" size="small" style="margin-left: 16px;" @click="addServer(item.Host, item.Available)">＋</a-button>
					</div>
				</a-list-item>
			</a-list>
		</a-modal>
	</div>
</template>

<script>
import xText from '../components/editor/text.vue';
import xNumber from '../components/editor/number.vue';
import xSwitch from '../components/editor/switch.vue';

export default {
	components: { xText, xNumber, xSwitch },
	data() {
		return {
			auth: this.$auth.isAuth,
			formRoute: this.$form.createForm(this, { name: 'route' }),
			routes: [],
			route: null,
			routeIndex: 0,
			routeHasEdit: false,
			servers: [],
			serverModel: false,
			columns: [
				{
					title: '主机',
					dataIndex: 'Host',
					scopedSlots: { customRender: 'host' }
				},
				{
					title: '比重',
					dataIndex: 'Weight',
					scopedSlots: { customRender: 'weight' }
				},
				{
					title: '最大连接',
					dataIndex: 'MaxRps',
					scopedSlots: { customRender: 'maxrps' }
				},
				{
					title: '后备服务',
					dataIndex: 'Standby',
					scopedSlots: { customRender: 'standby' }
				},
				{
					title: '',
					key: 'action',
					align: 'right',
					scopedSlots: { customRender: 'action' }
				}
			]
		};
	},
	created() {
		this.reload(0);
	},
	methods: {
		// 重新加载路由列表
		// init 是否将第一条结果赋值给路由编辑数据
		reload(showIndex) {
			this.routes = [];
			
			this.$axios.get('_gateway/route/list').then(res => {
				if (res && res.Data) {
					this.routes = res.Data;
					this.switchRoute(showIndex);
				}
			});
		},

		switchRoute(routeIndex) {
			if (this.routes && this.routes.length > routeIndex && routeIndex >= 0) {
				// this.route = { ...this.routes[routeIndex] };
				this.route = JSON.parse(JSON.stringify(this.routes[routeIndex]));
			} else {
				routeIndex = -1;
				this.route = null;
			}

			this.routeIndex = routeIndex;
			this.routeHasEdit = false;
		},

		// 加载服务器列表，如果路由中已经存在则移除
		// 显示服务器列表
		showServers() {
			this.servers = [];
			
			this.$axios.get('_gateway/server/list').then(res => {
				if (res && res.Data && res.Data.length > 0) {
					if (this.route.Servers && this.route.Servers.length > 0) {
						// 移除包含的部分
						res.Data.forEach(d => {
							const result = this.route.Servers.some(item => {
								if (item.Host == d.Host) {
									return true;
								}
							});

							if (!result) {
								this.servers.push(d);
							}
						});
					} else {
						this.servers = res.Data;
					}

					if (this.servers && this.servers.length > 0) {
						// 存在服务器数据
						this.serverModel = true;
					} else {
						this.$notification.warn({
							message: '暂无服务器数据',
							description: '目前已经存在的服务器已经全部添加，如果需要增加服务器请先打开“服务器”页面，添加更多服务器后再进行选择！'
						});
					}
				}
			});
		},

		addRoute(e) {
			e.preventDefault();
			this.formRoute.validateFields((err, values) => {
				if (!err) {
					// 表单正确
					
					this.$axios.post('_gateway/route/insert', values).then(res => {
						if (res) {
							// 添加成功
							if (res.Data) {
								this.$notification.success({
									message: '添加成功',
									description: '路由信息已经保存，请点击项目进行具体设置'
								});

								// 刷新状态
								this.formRoute.resetFields();
								this.reload(0);
							} else {
								this.$notification.warn({
									message: '添加失败',
									description: '请检查你的信息是否有误'
								});
							}
						}
					});
				}
			});
		},

		delRoute(url) {
			
			this.$axios.post('_gateway/route/remove', { url: url }).then(res => {
				if (res) {
					// 删除成功
					if (res.Data) {
						this.$notification.success({
							message: '删除成功',
							description: '路由信息已经删除'
						});

						// 刷新状态
						this.reload(0);
					} else {
						this.$notification.warn({
							message: '删除失败',
							description: '删除路由信息发生异常，请稍后再尝试'
						});
					}
				}
			});
		},

		updateRoute(data, NewValue, Type) {
			if (data && data.Url) {
				if (Type && data.hasOwnProperty(Type)) {
					data[Type] = NewValue;

					// 数据被编辑过
					this.routeHasEdit = true;
				}
			}
		},

		// 保存修改后的路由信息
		saveRoute() {
			if (this.route) {
				
				this.$axios.post('_gateway/route/update', { item: this.route }).then(res => {
					if (res) {
						// 更新成功
						if (res.Data) {
							this.$notification.success({
								message: '更新成功',
								description: '路由信息已经更新'
							});

							// 刷新状态
							this.reload(this.routeIndex);
						} else {
							this.$notification.warn({
								message: '更新失败',
								description: '请检查你的信息是否有误'
							});
						}
					}
				});
			}
		},

		addServer(url, status) {
			if (this.route && url) {
				let inc = false;

				if (this.route.Servers && this.route.Servers.length > 0) {
					inc = this.route.Servers.some(item => {
						if (item.Host === url) {
							return true;
						}
					});
				} else {
					this.route.Servers = [];
				}

				if (!inc) {
					this.route.Servers.push({ Host: url, Available: status, MaxRps: 0, Standby: false });
					this.serverModel = false;

					// 数据被编辑过
					this.routeHasEdit = true;
				}
			}
		},
		updateServer(data, NewValue, Type) {
			if (data && data.Host) {
				if (Type && data.hasOwnProperty(Type)) {
					data[Type] = NewValue;

					// 数据被编辑过
					this.routeHasEdit = true;
				} else {
					return;
				}
			}
		},
		delServer(url) {
			if (this.route && this.route.Servers && this.route.Servers.length > 0) {
				const s = [];
				this.route.Servers.forEach(d => {
					if (d.Host !== url) {
						s.push(d);
					}
				});

				this.route.Servers = s;

				// 数据被编辑过
				this.routeHasEdit = true;
			}
		}
	}
};
</script>

<style></style>
