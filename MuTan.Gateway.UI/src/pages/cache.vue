<template>
	<div>
		<a-page-header title="缓存" />
		<a-result status="warning" v-if="!auth" title="无权访问此页面，请点击右上角登录按钮，登录后再访问" />
		<div class="ant-page-content" v-else>
			<a-card :bodyStyle="{ padding: 0 }">
				<a-form layout="inline" slot="title" :form="form" @submit="addCache">
					<a-form-item>
						<a-input placeholder="缓存地址" v-decorator="['api', { rules: [{ required: true, min: 2, message: '请输入有效的缓存规则' }] }]" type="text">
							<a-icon slot="prefix" type="link" />
							<a-tooltip slot="suffix" title="缓存地址"><a-icon type="info-circle" style="color: rgba(0,0,0,.45)" /></a-tooltip>
						</a-input>
					</a-form-item>
					<a-form-item>
						<a-input-number placeholder="缓存时长" :min="1" v-decorator="['time', { rules: [{ required: true, message: '缓存时长最少1秒' }] }]" />
					</a-form-item>
					<a-form-item><a-button html-type="submit">添加</a-button></a-form-item>
				</a-form>
				<a-table :columns="columns" :data-source="data" :rowKey="record => record.api" :pagination="false" :loading="loading">
					<xNumber :value="text" :min="1" @change="updateCache(record.api, $event)" slot="time" slot-scope="text, record" />
					<div slot="action" slot-scope="text, record">
						<a-popconfirm title="确定删除此缓存吗？" ok-text="确定" cancel-text="取消" @confirm="delCache(record.api)">
							<a><a-icon style="color:red" type="delete" /></a>
						</a-popconfirm>
					</div>
				</a-table>
			</a-card>
		</div>
	</div>
</template>

<script>
import xNumber from '../components/editor/number.vue';

export default {
	components: { xNumber },
	data() {
		return {
			auth: this.$auth.isAuth,
			form: this.$form.createForm(this, { name: 'Cache' }),
			loading: false,
			columns: [
				{
					title: '缓存规则',
					dataIndex: 'api',
					sorter: (a, b) => (a.api || '').localeCompare(b.api || '')
				},
				{
					title: '时长',
					dataIndex: 'time',
					scopedSlots: { customRender: 'time' }
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
			this.loading = true;
			this.data = [];

			this.$axios.get('_gateway/cache/list').then(res => {
				if (res && res.Data) {
					for (var api in res.Data) {
						this.data.push({ api, time: res.Data[api] });
					}
					this.loading = false;
				}
			});
		},
		addCache(e) {
			e.preventDefault();
			this.form.validateFields((err, values) => {
				if (!err) {
					// 表单正确
					this.$axios.post('_gateway/cache/update', values).then(res => {
						if (res) {
							// 添加成功
							if (res.Data) {
								this.$notification.success({
									message: '添加成功',
									description: '缓存信息已经保存'
								});

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
		updateCache(api, time) {
			if (api && time && time > 0) {
				this.$axios.post('_gateway/cache/update', { api, time }).then(res => {
					if (res) {
						// 更新成功
						if (res.Data) {
							this.$notification.success({
								message: '更新成功',
								description: '缓存信息已经更新'
							});

							// 刷新状态
							this.reload();
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
		delCache(api) {
			// 表单正确

			this.$axios.get('_gateway/cache/remove', { api: api }).then(res => {
				if (res) {
					// 删除成功
					if (res.Data) {
						this.$notification.success({
							message: '删除成功',
							description: '缓存信息已经删除'
						});

						// 刷新状态
						this.reload();
					} else {
						this.$notification.warn({
							message: '删除失败',
							description: '删除缓存信息发生异常，请稍后再尝试'
						});
					}
				}
			});
		}
	}
};
</script>

<style></style>
