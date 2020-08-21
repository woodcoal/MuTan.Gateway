<template>
	<div>
		<a-page-header title="日志" />
		<a-result status="warning" v-if="!auth" title="无权访问此页面，请点击右上角登录按钮，登录后再访问" />
		<div class="ant-page-content" v-else>
			<a-card :tab-list="menu.data" :active-tab-key="menu.item" @tabChange="key => (menu.item = key)" :bodyStyle="{ padding: 0 }">
				<div v-if="menu.item === menu.data[0].key">
					<!-- 系统日志 -->
					<a-table :columns="sys.columens" :data-source="sys.data" :rowKey="(record, i) => '_' + i" :pagination="sys.pages" :loading="sys.loading">
						<div slot="message" slot-scope="text" class="textBreak">
							<span v-if="text && text.length > 100">{{ text.substr(0, 100) }}...</span>
							<span v-else>{{ text }}</span>
						</div>
						<pre slot="expandedRowRender" slot-scope="record" class="textBreak">{{ record.Message }}</pre>
					</a-table>
				</div>
				<div v-else>
					<!-- 访问记录 -->
					<a-form layout="inline" style="padding: 16px;">
						<a-form-item label="文件">
							<a-select
								show-search
								placeholder="记录文件"
								option-filter-prop="children"
								:filter-option="(input, option) => option.componentOptions.children[0].text.toLowerCase().indexOf(input.toLowerCase()) >= 0"
								style="min-width: 160px;"
								v-model="rec.file"
								@change="loadRec"
							>
								<a-select-option v-for="f in rec.files" :key="f">{{ f }}</a-select-option>
							</a-select>
						</a-form-item>
						<a-form-item><a-divider type="vertical" /></a-form-item>
						<a-form-item label="搜索">
							<a-input v-model="rec.search" placeholder="关键词" @pressEnter="loadRec">
								<div slot="suffix">
									<a title="搜索" @click="loadRec()"><a-icon type="search" /></a>
									<a-divider type="vertical" />
									<a
										title="重置搜索"
										@click="
											rec.search = '';
											loadRec();
										"
									>
										<a-icon type="reload" />
									</a>
								</div>
							</a-input>
						</a-form-item>
					</a-form>

					<a-table :columns="rec.columens" :data-source="rec.data" :rowKey="(record, i) => i" :pagination="rec.pages" :loading="rec.loading">
						<a-row type="flex" align="middle" slot="update" slot-scope="text, record">
							<div style="text-align: center;width: 50px;font-size: 10px;">
								<a-progress
									type="dashboard"
									:width="30"
									:percent="speedPercent(record.Time)"
									:strokeWidth="20"
									:showInfo="false"
									:strokeColor="speedColor(record.Time)"
								/>
								<div>{{ record.Time }} ms</div>
							</div>
							<div>
								<a-divider type="vertical" />
								<strong>{{ moment(text).format('YYYY-MM-DD HH:mm:ss') }}</strong>
							</div>
						</a-row>
						<template slot="url" slot-scope="text, record">
							<a-tag :style="{ backgroundColor: codeColor(record.Code), color: 'white' }">{{ record.Code }}</a-tag>
							<span :style="{ color: codeColor(record.Code) }">{{ text }}</span>
						</template>
						<div slot="expandedRowRender" slot-scope="record" class="textBreak">
							<a-divider>基本信息</a-divider>
							<p>
								<strong>时间：</strong>
								<em>{{ moment(record.Update).format('YYYY-MM-DD HH:mm:ss') }}</em>
							</p>
							<p>
								<strong>地址：</strong>
								<em>{{ record.Url }}</em>
							</p>
							<p>
								<strong>方式：</strong>
								<em>{{ record.Method }}</em>
							</p>
							<p>
								<strong>代码：</strong>
								<em>{{ record.Code }}</em>
							</p>
							<p>
								<strong>速度：</strong>
								<em>{{ record.Time }} ms</em>
							</p>
							<p>
								<strong>ＩＰ：</strong>
								<em>{{ record.IP }}</em>
							</p>
							<p>
								<strong>客户端：</strong>
								<em>{{ record.Client || '-' }}</em>
							</p>
							<a-divider>头部数据</a-divider>
							<pre>{{ record.Header || '暂无' }}</pre>
							<a-divider>请求参数</a-divider>
							<pre>{{ record.Data || '暂无' }}</pre>
							<a-divider>Cookies</a-divider>
							<pre>{{ record.Cookies || '暂无' }}</pre>
						</div>
					</a-table>
				</div>
			</a-card>
		</div>
	</div>
</template>

<script>
import moment from 'moment';

export default {
	data() {
		return {
			auth: this.$auth.isAuth,

			moment,

			menu: {
				data: [
					{
						key: 'sys',
						tab: '系统日志'
					},
					{
						key: 'rec',
						tab: '访问记录'
					}
				],
				item: 'sys'
			},

			sys: {
				columens: [
					{
						title: '时间',
						dataIndex: 'Time'
					},
					{
						title: '类型',
						dataIndex: 'Type'
					},
					{
						title: 'IP',
						dataIndex: 'RemoveIP'
					},
					{
						title: '消息',
						dataIndex: 'Message',
						scopedSlots: { customRender: 'message' }
					}
				],
				data: [],
				loading: false,
				pages: {
					current: 0,
					pageSize: 10,
					total: 0,
					pageSizeOptions: ['10', '20', '50', '100'],
					showSizeChanger: true,
					onShowSizeChange: this.onPageSYS.bind(this), // 改变每页数量时更新显示
					onChange: this.onPageSYS.bind(this), //点击页码事件
					showTotal: (total, range) => `共 ${total} 条 (${range[0]}-${range[1]}) `,
					style: { marginRight: '16px' }
				}
			},

			rec: {
				columens: [
					{
						title: '时间',
						dataIndex: 'Update',
						width: 250,
						scopedSlots: { customRender: 'update' }
					},
					{
						title: '请求地址',
						dataIndex: 'Url',
						ellipsis: true,
						scopedSlots: { customRender: 'url' }
					},
					{
						title: '访问IP',
						dataIndex: 'IP'
					},
					{
						title: '客户端',
						dataIndex: 'Client'
					}
				],
				files: [],
				file: '',
				search: '',
				data: [],
				loading: false,
				pages: {
					current: 0,
					pageSize: 10,
					total: 0,
					pageSizeOptions: ['10', '20', '50', '100'],
					showSizeChanger: true,
					onShowSizeChange: this.onPageREC.bind(this), // 改变每页数量时更新显示
					onChange: this.onPageREC.bind(this), //点击页码事件
					showTotal: (total, range) => `共 ${total} 条 (${range[0]}-${range[1]}) `,
					style: { marginRight: '16px' }
				}
			}
		};
	},
	mounted() {
		this.loadSYS();
		this.loadRecFiles();
	},
	methods: {
		onPageSYS(current, pagesize) {
			if (current > 0) this.sys.pages.current = current;
			if (pagesize > 0) this.sys.pages.pageSize = pagesize;
			this.loadSYS();
		},
		onPageREC(current, pagesize) {
			if (current > 0) this.rec.pages.current = current;
			if (pagesize > 0) this.rec.pages.pageSize = pagesize;
			this.loadRec();
		},

		loadSYS() {
			this.sys.data = [];
			this.sys.pages.total = 0;
			this.sys.loading = true;

			this.$axios.get('_gateway/log/sys', { pageIndex: this.sys.pages.current, pageCount: this.sys.pages.pageSize }).then(res => {
				if (res && res.Data && res.Data.Result) {
					this.sys.pages.total = res.Data.Count;
					this.sys.data = res.Data.Result;
				}
				this.sys.loading = false;
			});
		},

		// 加载日志数据文件
		loadRecFiles() {
			this.rec.files = [];

			this.$axios.get('_gateway/log/recfiles').then(res => {
				if (res && res.Data && res.Data.length > 0) {
					this.rec.files = res.Data;
					this.rec.file = res.Data[0];

					this.loadRec();
				}
			});
		},

		// 切换日志文件
		loadRec() {
			this.rec.data = [];
			this.rec.pages.total = 0;
			this.rec.loading = true;

			this.$axios
				.get('_gateway/log/rec', { file: this.rec.file, keyword: this.rec.search, pageIndex: this.rec.pages.current, pageCount: this.rec.pages.pageSize })
				.then(res => {
					if (res && res.Data && res.Data.Result) {
						this.rec.pages.total = res.Data.Count;
						this.rec.data = res.Data.Result;
					}
					this.rec.loading = false;
				});
		},

		// 代码颜色
		codeColor(code) {
			switch (parseInt(code / 100)) {
				case 2:
					return 'green';
					break;
				case 3:
					return 'blue';
					break;
				case 4:
					return 'orange';
					break;
				case 5:
					return 'red';
					break;
				default:
					return '';
					break;
			}
		},
		// 速度颜色
		speedColor(speed) {
			if (speed <= 10) {
				return '#00A505';
			} else if (speed > 10 && speed <= 50) {
				return '#2CDD00';
			} else if (speed > 50 && speed <= 100) {
				return '#99FF00';
			} else if (speed > 100 && speed <= 200) {
				return '#FFFF00';
			} else if (speed > 200 && speed <= 500) {
				return '#FFCC00';
			} else if (speed > 500 && speed <= 1000) {
				return '#FF9900';
			} else if (speed > 1000 && speed <= 2000) {
				return '#FF6600';
			} else if (speed > 2000 && speed <= 5000) {
				return '#FF3300';
			} else {
				return '#771800';
			}
		},

		// 速度百分比，超过5秒为100%
		speedPercent(speed) {
			const p = speed / 50;
			return p > 100 ? 100 : p;
		}
	}
};
</script>

<style></style>
