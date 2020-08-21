<template>
	<div>
		<a-page-header title="跳转" />
		<a-result status="warning" v-if="!auth" title="无权访问此页面，请点击右上角登录按钮，登录后再访问" />
		<div class="ant-page-content" v-else>
			<a-card :bodyStyle="{ padding: 0 }">
				<a-form layout="inline" slot="title" :form="form" @submit="addRewrite">
					<a-form-item>
						<a-input placeholder="HOST" v-decorator="['host']" type="text">
							<a-icon slot="prefix" type="cloud-server" />
							<a-tooltip slot="suffix" title="主机头，非必填项。"><a-icon type="info-circle" style="color: rgba(0,0,0,.45)" /></a-tooltip>
						</a-input>
					</a-form-item>
					<a-form-item>
						<a-input placeholder="URL (必填)" v-decorator="['url', { rules: [{ required: true, min: 1, message: '请输入有效的路径' }] }]" type="text">
							<a-icon slot="prefix" type="share-alt" />
							<a-tooltip slot="suffix" title="需要跳转的地址，可以使用变量，用大括号包含，如：/path/{id}.html">
								<a-icon type="info-circle" style="color: rgba(0,0,0,.45)" />
							</a-tooltip>
						</a-input>
					</a-form-item>
					<a-form-item>
						<a-input placeholder="REWRITE (必填)" v-decorator="['rewrite', { rules: [{ required: true, min: 1, message: '请输入有效的转向地址' }] }]" type="text">
							<a-icon slot="prefix" type="pull-request" />
							<a-tooltip slot="suffix" title="跳转后的地址，可以使用来自跳转地址的变量，用大括号包含，如：/new/{id}.html">
								<a-icon type="info-circle" style="color: rgba(0,0,0,.45)" />
							</a-tooltip>
						</a-input>
					</a-form-item>
					<a-form-item><a-button html-type="submit">添加</a-button></a-form-item>
				</a-form>
				<a-table :columns="columns" :data-source="data" :rowKey="(record, i) => i" :pagination="false">
					<div slot="action" slot-scope="text, record">
						<a-popconfirm title="确定删除此转向信息吗？" ok-text="确定" cancel-text="取消" @confirm="delRewrite(record)">
							<a><a-icon style="color:red" type="delete" /></a>
						</a-popconfirm>
					</div>
				</a-table>
			</a-card>
		</div>
	</div>
</template>

<script>
export default {
	data() {
		return {
			auth: this.$auth.isAuth,
			form: this.$form.createForm(this, { name: 'rewrite' }),
			columns: [
				{
					title: '主机',
					dataIndex: 'Host',
					key: 'host',
					sorter: (a, b) => (a.Host || '').localeCompare(b.Host || '')
				},
				{
					title: '路径',
					dataIndex: 'Url',
					sorter: (a, b) => (a.Url || '').localeCompare(b.Url || '')
				},
				{
					title: '跳转',
					dataIndex: 'Rewrite',
					sorter: (a, b) => (a.Rewrite || '').localeCompare(b.Rewrite || '')
				},
				{
					title: '',
					key: 'action',
					align: 'right',
					scopedSlots: { customRender: 'action' }
				}
			],
			data: null
		};
	},
	mounted() {
		this.reload();
	},
	methods: {
		// 重载列表
		reload() {
			
			this.$axios.get('_gateway/rewrite/list').then(res => {
				if (res && res.Data) {
					this.data = res.Data;
				}
			});
		},
		addRewrite(e) {
			e.preventDefault();
			this.form.validateFields((err, values) => {
				if (!err) {
					// 表单正确
					
					this.$axios.post('_gateway/rewrite/insert', values).then(res => {
						if (res) {
							// 添加成功
							if (res.Data) {
								this.$notification.success({
									message: '添加成功',
									description: '跳转信息已经保存'
								});

								values = {};

								// 刷新状态
								this.form.resetFields();
								this.reload();
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
		delRewrite(record) {
			// 表单正确
			
			this.$axios.post('_gateway/rewrite/remove', record).then(res => {
				if (res) {
					// 删除成功
					if (res.Data) {
						this.$notification.success({
							message: '删除成功',
							description: '跳转信息已经删除'
						});

						// 刷新状态
						this.reload();
					} else {
						this.$notification.warn({
							message: '删除失败',
							description: '删除跳转信息发生异常，请稍后再尝试'
						});
					}
				}
			});
		}
	}
};
</script>

<style></style>
