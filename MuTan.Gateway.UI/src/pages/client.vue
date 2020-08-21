<template>
	<div>
		<a-page-header title="客户端" />
		<a-result status="warning" v-if="!auth" title="无权访问此页面，请点击右上角登录按钮，登录后再访问" />
		<div class="ant-page-content" v-else>
			<a-card :tab-list="menu.data" :active-tab-key="menu.item" @tabChange="key => (menu.item = key)" :bodyStyle="{ padding: 0 }">
				<!-- 客户端 -->
				<div v-if="menu.item === menu.data[0].key">
					<a-form layout="inline" :form="client.form" style="padding: 16px;">
						<a-form-item>
							<a-input v-model="client.search" placeholder="搜索" @pressEnter="loadClient">
								<div slot="suffix">
									<a title="搜索" @click="loadClient()"><a-icon type="search" /></a>
									<a-divider type="vertical" />
									<a
										title="重置搜索"
										@click="
											client.search = '';
											loadClient();
										"
									>
										<a-icon type="reload" />
									</a>
								</div>
							</a-input>
						</a-form-item>
						<a-form-item><a-divider type="vertical" /></a-form-item>
						<a-form-item>
							<a-input
								placeholder="账号"
								:maxLength="24"
								v-decorator="[
									'name',
									{ rules: [{ required: true, pattern: '^[a-zA-Z]{1,1}[0-9a-zA-Z_\-]{2,23}$', min: 3, max: 24, message: '请输入有效的账号' }] }
								]"
								type="text"
							>
								<a-tooltip slot="suffix" title="账号，仅允许字母、数字、下划线，且必须字母开头，最少3个字符，最多24个字符">
									<a-icon type="info-circle" style="color: rgba(0,0,0,.45)" />
								</a-tooltip>
							</a-input>
						</a-form-item>

						<a-form-item>
							<a-select
								show-search
								placeholder="权限"
								option-filter-prop="children"
								:filter-option="(input, option) => option.componentOptions.children[0].text.toLowerCase().indexOf(input.toLowerCase()) >= 0"
								style="min-width: 160px;"
								v-decorator="['role', { rules: [{ required: true, message: '请选择有效的权限' }] }]"
							>
								<a-select-option v-for="r in role.data" :key="r.Name">{{ r.Name }}</a-select-option>
							</a-select>
						</a-form-item>

						<a-form-item><a-button @click="addClient">添加</a-button></a-form-item>
					</a-form>

					<a-table :columns="client.columens" :data-source="client.data" :rowKey="record => record.Name" :pagination="client.pages" :loading="client.loading">
						<span slot="name" slot-scope="text, record">
							<a-icon type="check-circle" style="color:#008000" v-if="record.IsEnabled" />
							<a-icon type="exclamation-circle" style="color:#e00" v-else />
							<strong style="margin-left: 8px;">{{ record.Name }}</strong>
						</span>

						<template slot="role" slot-scope="text">
							<strong v-if="text == superRole">系统管理员</strong>
							<span v-else>{{ text }}</span>
						</template>

						<template slot="open" slot-scope="text">
							<span v-if="text">启用</span>
							<span v-else>禁用</span>
						</template>

						<div slot="action" slot-scope="text, record">
							<a-popconfirm
								okText="重置"
								cancelText="复制"
								@confirm="resetKey(record.Name)"
								@cancel="copyKey"
								@visibleChange="v => (v ? loadKey(record.Name) : null)"
							>
								<span slot="title">
									当前密匙为：
									<a title="点击复制密匙到剪贴板" @click="copyKey">
										<strong>{{ client.key }}</strong>
									</a>

									<br />
									请妥善保管好密匙，不要随意泄露！
									<br />
									您可以点击
									<a-tag @click="copyKey">复制</a-tag>
									按钮复制此密匙并关闭；
									<br />
									如需要重置密匙，请单击
									<a-tag @click="resetKey(record.Name)">重置</a-tag>
									按钮。
								</span>
								<a title="获取密匙"><a-icon style="color:green" type="key" /></a>
							</a-popconfirm>

							<template v-if="record.Role !== superRole">
								<a-divider type="vertical" />
								<a title="编辑" @click="editClient(record)"><a-icon type="edit" /></a>
								<a-divider type="vertical" />
								<a-popconfirm title="确定删除此权限吗？" ok-text="确定" cancel-text="取消" @confirm="delClient(record.Name)" v-if="record.Role !== superRole">
									<a><a-icon style="color:red" type="delete" /></a>
								</a-popconfirm>
							</template>
						</div>
					</a-table>
				</div>

				<!-- 权限 -->
				<div v-else-if="menu.item === menu.data[1].key">
					<a-form layout="inline" :form="role.form" style="padding: 16px;" @submit="addRole">
						<a-form-item>
							<a-input
								placeholder="权限名称"
								:maxLength="24"
								v-decorator="[
									'name',
									{ rules: [{ required: true, pattern: '^[a-zA-Z]{1,1}[0-9a-zA-Z_\-]{2,23}$', min: 3, max: 24, message: '请输入有效的权限名称' }] }
								]"
								type="text"
							>
								<a-tooltip slot="suffix" title="权限名称，仅允许字母、数字、下划线，且必须字母开头，最少3个字符，最多24个字符">
									<a-icon type="info-circle" style="color: rgba(0,0,0,.45)" />
								</a-tooltip>
							</a-input>
						</a-form-item>
						<a-form-item>
							<a-textarea
								autoSize
								placeholder="API"
								v-decorator="['apis', { rules: [{ required: true, min: 2, message: '请输入授权API列表' }] }]"
								type="text"
								style="width: 320px;"
							/>
						</a-form-item>
						<a-form-item><a-button html-type="submit">添加</a-button></a-form-item>
					</a-form>

					<a-table :columns="role.columens" :data-source="role.data" :rowKey="record => record.Name" :pagination="role.pages" :loading="role.loading">
						<xTextArea isArray slot="apis" slot-scope="text, record" :value="text" @change="updateRole(record, $event)" />
						<div slot="action" slot-scope="text, record">
							<a-popconfirm title="确定删除此权限吗？" ok-text="确定" cancel-text="取消" @confirm="delRole(record.Name)">
								<a><a-icon style="color:red" type="delete" /></a>
							</a-popconfirm>
						</div>
					</a-table>
				</div>

				<!-- API -->
				<div v-else>
					<div style="padding: 16px;">
						<a-input v-model="api.search" placeholder="查询或者匹配的地址">
							<div slot="suffix">
								<a-tooltip title="搜索模式：搜索权限中是否存在包含此地址的数据。">
									<a @click="loadAPI(-1)"><a-icon type="search" /></a>
								</a-tooltip>
								<a-divider type="vertical" />
								<a-tooltip title="匹配模式：检查当前网址是否在权限限制当中。">
									<a @click="loadAPI(1)"><a-icon type="api" /></a>
								</a-tooltip>
								<a-divider type="vertical" />
								<a-tooltip title="无需权限API列表">
									<a @click="loadIgnoreAPI"><a-icon type="history" /></a>
								</a-tooltip>
							</div>
						</a-input>
					</div>

					<a-divider>
						<span v-if="api.mode == -1">API 搜索</span>
						<span v-else-if="api.mode == 1">API 匹配查询</span>
						<span v-else>无需权限 API 列表</span>
					</a-divider>

					<a-list :data-source="api.data" style="padding: 16px;" v-if="api.data.length > 0">
						<a-row slot="header" type="flex" justify="space-between" v-if="api.data.length > 0">
							<strong>网址</strong>
							<strong>权限</strong>
						</a-row>
						<a-list-item slot="renderItem" slot-scope="item, index">
							<strong>{{ item.url }}</strong>
							<template v-for="r in item.roles">
								<span slot="actions" :key="r">
									<em>{{ r }}</em>
								</span>
							</template>
						</a-list-item>
					</a-list>
					<a-empty v-else :description="api.mode == -1 ? '暂无搜索结果' : api.mode == 1 ? '暂无匹配结果' : '暂无 API'" style="padding: 32px;" />
				</div>
			</a-card>
		</div>

		<a-modal
			:title="'客户端：' + client.item.Name"
			v-model="client.edit"
			width="640px"
			okText="更新"
			cancelText="关闭"
			destroyOnClose
			@ok="updateClient"
			:okButtonProps="client.updateButton"
			v-if="client.item.Role != superRole"
		>
			<a-alert
				message="客户端参数已经被修改，请点击右下角 [更新] 按钮，以便将修改后的数据上传到后台，上传后的数据将实时生效！"
				type="info"
				style="margin-bottom: 16px;"
				showIcon
				v-if="client.hasEdit"
			/>
			<a-descriptions title="" :column="2" bordered>
				<a-descriptions-item label="权限"><xSelect v-model="client.item.Role" :datas="role.names" @change="hasEdit(true)" /></a-descriptions-item>
				<a-descriptions-item label="启用"><xSwitch v-model="client.item.Open" @change="hasEdit(true)" /></a-descriptions-item>
				<a-descriptions-item label="临时开启"><xDateTime v-model="client.item.TimeStart" showTime @change="hasEdit(true)" /></a-descriptions-item>
				<a-descriptions-item label="临时关闭"><xDateTime v-model="client.item.TimeStop" showTime @change="hasEdit(true)" /></a-descriptions-item>
				<a-descriptions-item label="关闭消息" :span="2"><xTextArea v-model="client.item.CloseMessage" defaultValue="暂无" @change="hasEdit(true)" /></a-descriptions-item>
			</a-descriptions>
			<a-divider>客户端参数</a-divider>
			<JsonEditor v-model="client.item.Config" @change="hasEdit(true)" />
		</a-modal>
	</div>
</template>

<script>
import moment from 'moment';
import xDateTime from '../components/editor/datetime.vue';
import xTextArea from '../components/editor/textarea.vue';
import xSwitch from '../components/editor/switch.vue';
import xSelect from '../components/editor/select.vue';
import JsonEditor from '../components/jsonEditor.vue';

export default {
	components: { xDateTime, xTextArea, xSwitch, xSelect, JsonEditor },
	data() {
		return {
			superRole: '_.SUPER._', // 超级管理员权限名称

			auth: this.$auth.isAuth,

			menu: {
				data: [
					{
						key: 'client',
						tab: '客户端'
					},
					{
						key: 'role',
						tab: '权限'
					},
					{
						key: 'api',
						tab: '规则检测'
					}
				],
				item: 'client'
			},

			client: {
				form: this.$form.createForm(this, { name: 'client' }),
				columens: [
					{
						title: '名称',
						dataIndex: 'Name',
						scopedSlots: { customRender: 'name' }
					},
					{
						title: '权限',
						dataIndex: 'Role',
						scopedSlots: { customRender: 'role' }
					},
					{
						title: '状态',
						dataIndex: 'IsEnabled',
						scopedSlots: { customRender: 'open' }
					},
					{
						title: '',
						key: 'action',
						align: 'right',
						scopedSlots: { customRender: 'action' }
					}
				],
				data: [],
				item: {},
				loading: false,
				edit: false,
				hasEdit: false,
				updateButton: { props: { disabled: true } },
				pages: {
					current: 0,
					pageSize: 10,
					total: 0,
					pageSizeOptions: ['10', '20', '50', '100'],
					showSizeChanger: true,
					onShowSizeChange: this.onPageChange.bind(this), // 改变每页数量时更新显示
					onChange: this.onPageChange.bind(this), //点击页码事件
					showTotal: (total, range) => `共 ${total} 条 (${range[0]}-${range[1]}) `,
					style: { marginRight: '16px' }
				},
				search: '',
				key: ''
			},

			role: {
				form: this.$form.createForm(this, { name: 'route' }),
				columens: [
					{
						title: '名称',
						dataIndex: 'Name',
						sorter: (a, b) => (a.Name || '').localeCompare(b.Name || '')
					},
					{
						title: 'API 列表',
						dataIndex: 'Apis',
						scopedSlots: { customRender: 'apis' }
					},
					{
						title: '',
						key: 'action',
						align: 'right',
						scopedSlots: { customRender: 'action' }
					}
				],
				data: [],
				names: [],
				loading: false,
				pages: {
					current: 0,
					pageSize: 10,
					total: 0,
					pageSizeOptions: ['10', '20', '50', '100'],
					showSizeChanger: true,
					onChange: (page, pagesize) => {
						this.role.pages.current = page;
						this.role.pages.pageSize = pagesize;
					}, //点击页码事件
					showTotal: (total, range) => `共 ${total} 条 (${range[0]}-${range[1]}) `,
					style: { marginRight: '16px' }
				}
			},

			api: { search: '', data: [], mode: 0 }
		};
	},
	mounted() {
		this.loadClient();
		this.loadRole();
		this.loadIgnoreAPI();
	},
	methods: {
		onPageChange(current, pagesize) {
			if (current > 0) this.client.pages.current = current;
			if (pagesize > 0) this.client.pages.pageSize = pagesize;
			this.loadClient();
		},

		loadKey(name) {
			this.client.key = '';

			if (name) {
				this.$axios.get('_gateway/client/info', { Name: name }).then(res => {
					if (res && res.Data) {
						this.client.key = res.Data.Key;
					}
				});
			}
		},

		resetKey(name, e) {
			this.client.key = '';

			if (name) {
				this.$axios.get('_gateway/client/updatekey', { Name: name }).then(res => {
					if (res && res.Data) {
						this.client.key = res.Data;

						this.$notification.success({
							message: '重置成功',
							description: '客户端密匙已经重置，原密匙已经生效，请及时替换客户端密匙，否则将导致原客户端无法访问 API'
						});
					}
				});
			}
		},

		copyKey() {
			var input = document.createElement('input'); // 直接构建input
			input.value = this.client.key; // 设置内容
			document.body.appendChild(input); // 添加临时实例
			input.select(); // 选择实例内容
			document.execCommand('Copy'); // 执行复制
			document.body.removeChild(input); // 删除临时实例

			this.$notification.success({
				message: '密匙已经复制'
			});
		},

		hasEdit(flag) {
			this.client.hasEdit = flag;
			this.client.updateButton.props.disabled = !flag;
		},

		// -----------------------------------
		// 客户端相关操作
		// -----------------------------------

		editClient(item) {
			if (item) {
				this.client.item = JSON.parse(JSON.stringify(item));
				this.client.edit = true;
				this.hasEdit(false);
			}
		},

		loadClient() {
			this.client.data = [];
			this.client.pages.total = 0;
			this.client.loading = true;

			this.$axios
				.get('_gateway/client/list', { pageIndex: this.client.pages.current, pageCount: this.client.pages.pageSize, keyword: this.client.search })
				.then(res => {
					if (res && res.Data && res.Data.Result) {
						this.client.pages.total = res.Data.Count;
						this.client.data = res.Data.Result;
					}
					this.client.loading = false;
				});
		},
		addClient(e) {
			e.preventDefault();
			this.client.form.validateFields((err, values) => {
				if (!err) {
					// 表单正确

					this.$axios.post('_gateway/client/insert', values).then(res => {
						if (res) {
							// 添加成功
							if (res.Data) {
								this.$notification.success({
									message: '添加成功',
									description: '账号信息已经保存'
								});

								// 刷新状态
								this.client.form.resetFields();
								this.loadClient();
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

		updateClient() {
			if (!this.client.item) return;

			this.$axios.post('_gateway/client/update', { client: this.client.item }).then(res => {
				if (res) {
					// 更新成功
					if (res.Data) {
						this.$notification.success({
							message: '更新成功',
							description: '客户端信息已经更新'
						});

						// 刷新状态
						this.loadClient();
						this.client.edit = false;
					} else {
						this.$notification.warn({
							message: '更新失败',
							description: '请检查你的信息是否有误'
						});
					}
				}
			});
		},

		delClient(name) {
			this.$axios.get('_gateway/client/remove', { name: name }).then(res => {
				if (res) {
					// 删除成功
					if (res.Data) {
						this.$notification.success({
							message: '删除成功',
							description: '客户端已经删除'
						});

						// 刷新状态
						this.loadClient();
					} else {
						this.$notification.warn({
							message: '删除失败',
							description: '删除客户端发生异常，请稍后再尝试'
						});
					}
				}
			});
		},

		// -----------------------------------
		// 权限相关操作
		// -----------------------------------
		loadRole() {
			this.role.data = [];
			this.role.names = [];
			this.role.loading = true;

			this.$axios.get('_gateway/role/list').then(res => {
				if (res && res.Data) {
					for (let k in res.Data) {
						this.role.data.push({ Name: k, Apis: res.Data[k] });
						this.role.names.push(k);
					}
					this.role.loading = false;
				}
			});
		},
		addRole(e) {
			e.preventDefault();
			this.role.form.validateFields((err, values) => {
				if (!err) {
					// 字符转数组
					const apis = values.apis.split('\n');
					values.apis = apis;

					// 表单正确
					this.$axios.post('_gateway/role/update', values).then(res => {
						if (res) {
							// 添加成功
							if (res.Data) {
								this.$notification.success({
									message: '添加成功',
									description: '权限信息已经保存'
								});

								// 刷新状态
								this.role.form.resetFields();
								this.loadRole();
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
		updateRole(data, NewValue) {
			if (data && data.Name && NewValue) {
				data.Apis = NewValue;

				this.$axios.post('_gateway/role/update', { Name: data.Name, Apis: data.Apis }).then(res => {
					if (res) {
						// 更新成功
						if (res.Data) {
							this.$notification.success({
								message: '更新成功',
								description: '权限信息已经更新'
							});

							// 刷新状态
							this.loadRole();
						} else {
							this.$notification.warn({
								message: '更新失败',
								description: '请检查你的信息是否有误'
							});
						}
					}
				});
			} else {
				// 刷新状态
				this.loadRole();

				this.$notification.warn({
					message: '更新失败',
					description: '请检查你的信息是否有误'
				});
			}
		},
		delRole(name) {
			this.$axios.get('_gateway/role/remove', { name: name }).then(res => {
				if (res) {
					// 删除成功
					if (res.Data) {
						this.$notification.success({
							message: '删除成功',
							description: '权限已经删除'
						});

						// 刷新状态
						this.loadRole();
					} else {
						this.$notification.warn({
							message: '删除失败',
							description: '删除权限发生异常，请稍后再尝试'
						});
					}
				}
			});
		},

		// -----------------------------------
		// API相关操作
		// -----------------------------------

		loadAPI(searchMode) {
			this.api.data = [];
			this.api.mode = searchMode;

			if (!this.api.search) return;

			const postURL = searchMode == -1 ? '_gateway/rule/search' : '_gateway/rule/match';

			this.$axios.post(postURL, { url: this.api.search }).then(res => {
				if (res && res.Data) {
					for (var u in res.Data) {
						if (res.Data[u] != this.superRole) {
							this.api.data.push({ url: u, roles: res.Data[u] });
						}
					}
				}
			});
		},

		loadIgnoreAPI() {
			this.api.data = [];
			this.api.mode = 0;

			this.$axios.get('_gateway/rule/ignores').then(res => {
				if (res && res.Data) {
					for (var u in res.Data) {
						this.api.data.push({ url: res.Data[u], roles: ['N/A'] });
					}
				}
			});
		}
	}
};
</script>

<style></style>
