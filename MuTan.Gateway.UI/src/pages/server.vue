<template>
	<div>
		<a-page-header title="服务器" />
		<a-result status="warning" v-if="!auth" title="无权访问此页面，请点击右上角登录按钮，登录后再访问" />
		<div class="ant-page-content" v-else>
			<a-card :bodyStyle="{ padding: 0 }">
				<a-form layout="inline" slot="title" :form="form" @submit="addServer">
					<a-form-item>
						<a-input placeholder="服务器网址" v-decorator="['url', { rules: [{ required: true, type: 'url', min: 8, message: '请输入有效的网址' }] }]" type="url">
							<a-icon slot="prefix" type="link" />
							<a-tooltip slot="suffix" title="服务器网址，包含协议部分。如：http://host:port/path">
								<a-icon type="info-circle" style="color: rgba(0,0,0,.45)" />
							</a-tooltip>
						</a-input>
					</a-form-item>
					<a-form-item><a-button html-type="submit">添加</a-button></a-form-item>
				</a-form>
				<a-table :columns="columns" :data-source="data" :rowKey="record => record.Host" :pagination="false" :loading="loading">
					<span slot="host" slot-scope="text, record">
						<a-icon type="check-circle" style="color:#008000" v-if="record.Available" />
						<a-icon type="exclamation-circle" style="color:#e00" v-else />
						<strong style="margin-left: 8px;">{{ record.Host }}</strong>
					</span>

					<xText slot="cate" slot-scope="text, record" :value="text" @change="updateServer(record, $event, 'Category')" />

					<xNumber slot="conn" slot-scope="text, record" :value="text" :min="1" @change="updateServer(record, $event, 'MaxConnections')" />

					<span slot="status" slot-scope="record">{{ record.Count }} / {{ record.WaitQueue }} / {{ record.Rps }}</span>

					<a slot="action" slot-scope="text, record">
						<a-popconfirm title="确定删除此服务器吗？" ok-text="确定" cancel-text="取消" @confirm="delServer(record.Host)">
							<a-icon style="color:red" type="delete" />
						</a-popconfirm>
					</a>

					<div slot="expandedRowRender" slot-scope="record">
						<xText :value="record.Remark" @change="updateServer(record, $event, 'Remark')" defaultValue="暂无备注" />
					</div>
				</a-table>
			</a-card>
		</div>
	</div>
</template>

<script>
import xText from '../components/editor/text.vue';
import xNumber from '../components/editor/number.vue';

export default {
	components: { xText, xNumber },
	data() {
		return {
			delay: 5000,
			auth: this.$auth.isAuth,
			form: this.$form.createForm(this, { name: 'server' }),
			loading: false,
			columns: [
				{
					title: '主机',
					dataIndex: 'Host',
					scopedSlots: { customRender: 'host' },
					sorter: (a, b) => (a.Host || '').localeCompare(b.Host || '')
				},
				{
					title: '分类',
					dataIndex: 'Category',
					scopedSlots: { customRender: 'cate' },
					sorter: (a, b) => (a.Category || '').localeCompare(b.Category || '')
				},
				{
					title: '最大链接数',
					dataIndex: 'MaxConnections',
					scopedSlots: { customRender: 'conn' },
					sorter: (a, b) => a.MaxConnections - b.MaxConnections
				},
				{
					title: '连接/等待/频率',
					key: 'status',
					scopedSlots: { customRender: 'status' }
				},
				{
					title: '',
					key: 'action',
					align: 'right',
					scopedSlots: { customRender: 'action' }
				}
			],
			data: null,
			isDestroyed: false // 是否已经退出，防止重复创建定时器
		};
	},
	created() {
		this.isDestroyed = false;
		this.loadServer();
	},
	destroyed() {
		this.isDestroyed = true;
		this.ticks();
	},
	methods: {
		// 定时操作
		ticks(succ) {
			if (this.timer) clearTimeout(this.timer);
			if (this.isDestroyed) return;

			this.loading = false;

			// 重新定时
			this.timer = setTimeout(
				() => {
					this.loadServer();
				},
				succ ? this.delay : this.delay * 10
			);
		},

		// 重载列表
		loadServer() {
			this.loading = true;

			this.$axios
				.get('_gateway/server/status')
				.then(res => {
					if (res && res.Data) {
						this.data = res.Data;
					}

					// 重新定时
					this.ticks(true);
				})
				.catch(res => {
					this.ticks(false);
				});
		},
		addServer(e) {
			e.preventDefault();
			this.form.validateFields((err, values) => {
				if (!err) {
					this.$axios.post('_gateway/server/update', { item: { maxConnections: 300, Host: values.url, Remark: values.url } }).then(res => {
						if (res) {
							// 添加成功
							if (res.Data) {
								this.$notification.success({
									message: '添加成功',
									description: '服务器信息已经保存'
								});

								// 刷新状态
								this.form.resetFields();
								this.loadServer();
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
		updateServer(data, NewValue, Type) {
			if (data && data.Host) {
				if (Type && data.hasOwnProperty(Type)) {
					data[Type] = NewValue;
				} else {
					return;
				}

				this.$axios.post('_gateway/server/update', { item: data }).then(res => {
					if (res) {
						// 更新成功
						if (res.Data) {
							this.$notification.success({
								message: '更新成功',
								description: '服务器信息已经更新'
							});

							// 刷新状态
							this.loadServer();
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
		delServer(host) {
			this.$axios.get('_gateway/server/remove', { host: host }).then(res => {
				if (res) {
					// 删除成功
					if (res.Data) {
						this.$notification.success({
							message: '删除成功',
							description: '服务器信息已经删除'
						});

						// 刷新状态
						this.loadServer();
					} else {
						this.$notification.warn({
							message: '删除失败',
							description: '删除服务器信息发生异常，请稍后再尝试'
						});
					}
				}
			});
		}
	}
};
</script>

<style></style>
