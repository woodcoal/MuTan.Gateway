<template>
	<div>
		<a-page-header title="首页" subTitle="网关基本状态">
			<a-row type="flex" justify="space-between" align="bottom">
				<div>
					<div>
						运行端口：
						<strong>{{ ServerHostPort }}</strong>
					</div>
					<div>
						网关名称：
						<strong>{{ Server.Name }}</strong>
					</div>
					<div>
						操作系统：
						<strong>{{ System.OSDescription }}</strong>
					</div>
				</div>
				<a-row type="flex" :gutter="[32, 0]" style="margin-top: 16px;">
					<a-col :span="8">
						<a-statistic :value="CounterSucc">
							<a-tooltip slot="title" title="系统启动以后访问成功次数">
								有效访问
								<a-icon type="info-circle" style="margin-left: 4px;color: rgba(0,0,0,.3)" />
							</a-tooltip>
						</a-statistic>
					</a-col>
					<a-col :span="8">
						<a-statistic :value="CounterALL">
							<a-tooltip slot="title" title="系统启动以后累计访问次数">
								累计访问
								<a-icon type="info-circle" style="margin-left: 4px;color: rgba(0,0,0,.3)" />
							</a-tooltip>
						</a-statistic>
					</a-col>
					<a-col :span="8">
						<a-statistic :value="SystemRuntime">
							<a-tooltip slot="title" :title="ServerStartTime">
								运行时长
								<a-icon type="info-circle" style="margin-left: 4px;color: rgba(0,0,0,.3)" />
							</a-tooltip>
						</a-statistic>
					</a-col>
				</a-row>
			</a-row>
		</a-page-header>
		<div class="ant-page-content">
			<a-row :gutter="[16, 16]">
				<a-col :sm="12" :xl="6">
					<div class="box">
						<a-statistic title="CPU" suffix="%" :precision="2" :value="Now.CPU" />
						<div class="chart"><boxChart id="chartCPU" :count="60" :datas="chartCPU" color="#008000" unit="%" /></div>
						<a-divider />
						<div class="info">{{ MaxValue(Max.CPU, 'CPU', '%') }}</div>
					</div>
				</a-col>
				<a-col :sm="12" :xl="6" class="mini">
					<div class="box">
						<a-statistic title="内存" suffix="MB" :precision="2" :value="Now.Memory" />
						<div class="chart"><boxChart id="chartMemo" :count="60" :datas="chartMemo" unit="MB" /></div>
						<a-divider />
						<div class="info">{{ MaxValue(Max.Memo, '内存', 'MB') }}</div>
					</div>
				</a-col>
				<a-col :sm="12" :xl="6">
					<div class="box">
						<a-row type="flex" justify="space-between">
							<a-statistic title="接收" suffix="MB" :precision="2" :value="GetMB(Now.ReceivBytes)" />
							<a-statistic title="发送" suffix="MB" :precision="2" :value="GetMB(Now.SendBytes)" />
						</a-row>
						<div class="chart">
							<br />
							<a-tooltip :title="'本次启动后累计接收数据：' + Now.ReceivBytes + ' Byte'"><a-progress :percent="BytesPercent(1)" :show-info="false" /></a-tooltip>
							<a-tooltip :title="'本次启动后累计发送数据：' + Now.SendBytes + ' Byte'">
								<a-progress :percent="BytesPercent(2)" :show-info="false" status="success" />
							</a-tooltip>
						</div>
						<a-divider />
						<div class="info">发送接收比：1：{{ BytesPercent(0) }}</div>
					</div>
				</a-col>
				<a-col :sm="12" :xl="6" class="mini">
					<div class="box">
						<a-row type="flex" justify="space-between">
							<a-statistic title="请求率" suffix="rps" :precision="2" :value="lastRequest" />
							<a-statistic title="连接率" suffix="rps" :precision="2" :value="lastConnections" />
						</a-row>
						<div class="info">
							<a-tooltip>
								<span slot="title">请求（{{ Now.Request }}）：连接（{{ Now.Connections }}）</span>
								<a-progress status="exception" :percent="100" :success-percent="RequestVsConnections" :show-info="false" />
							</a-tooltip>
						</div>
						<a-divider />
						<div class="chart"><boxChart id="chartRequest" :count="100" :showtip="false" :datas="chartRequest" color="#00aaff" /></div>
					</div>
				</a-col>
			</a-row>
			<a-row :gutter="[16, 16]">
				<a-col :span="24">
					<a-card title="请求统计">
						<a-row>
							<a-col :lg="18"><boxChartRequest :datas="Counter" :title="Config.COUNTER_STATUS_BEFORE + '小时请求统计'" @chartClick="loadCodeUrls" /></a-col>
							<a-col :lg="6"><boxUrlList count="5" :date="Url.codes.date" :name="Url.codes.name" :interval="Config.COUNTER_STATUS_DELAY" /></a-col>
						</a-row>
					</a-card>
				</a-col>
				<a-col :span="24">
					<a-card title="速度统计">
						<a-row>
							<a-col :lg="18"><boxChartSpeed :datas="Counter" :title="Config.COUNTER_STATUS_BEFORE + '小时速度统计'" @chartClick="loadTimeUrls" /></a-col>
							<a-col :lg="6"><boxUrlList count="5" :date="Url.times.date" :name="Url.times.name" :interval="Config.COUNTER_STATUS_DELAY" /></a-col>
						</a-row>
					</a-card>
				</a-col>
			</a-row>
		</div>
	</div>
</template>

<script>
import moment from 'moment';
import boxChart from '../components/box/chartDatas.vue';
import boxChartRequest from '../components/box/chartRequest.vue';
import boxChartSpeed from '../components/box/chartSpeed.vue';
import boxUrlList from '../components/box/urlList.vue';

export default {
	components: { boxChart, boxChartRequest, boxChartSpeed, boxUrlList },
	data() {
		return {
			delay: 5000,
			Server: {}, // 网关环境信息
			System: {}, // 系统环境信息
			Config: {}, // 系统参数
			Counter: [], // 访问统计
			Statistics: {}, // 系统启动后的统计
			Status: [], // 系统信息
			Now: {}, // 最新系统状态
			RunTime: {}, // 运行时长
			Max: {
				CPU: { max: 0, date: 0 },
				Memo: { max: 0, date: 0 }
			},
			Url: { codes: { name: 'all', date: new Date() }, times: { name: 0, date: new Date() } },
			isDestroyed: false // 是否已经退出，防止重复创建定时器
		};
	},
	created() {
		this.isDestroyed = false;
		this.loadEnvironment();
		this.loadInformation();
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

			// 重新定时
			this.timer = setTimeout(
				() => {
					this.loadInformation();
				},
				succ ? this.delay : this.delay * 10
			);
		},

		loadEnvironment() {
			this.$axios.get('/_gateway/status/environment').then(res => {
				if (res && res.Data) {
					this.Server = res.Data.Server;
					this.System = res.Data.System;
					this.Config = res.Data.Config;
				}
			});
		},

		loadInformation() {
			this.$axios
				.get('/_gateway/status/information')
				.then(res => {
					if (res && res.Data) {
						this.Counter = res.Data.Counter;
						this.Statistics = res.Data.Statistics;
						this.Status = res.Data.Status;
						this.Now = res.Data.Now;
						this.RunTime = res.Data.RunTime;

						// 最大 CPU 内存
						if (this.Status && this.Status.length > 0) {
							this.Status.forEach(s => {
								if (s.CPU >= this.Max.CPU.max) {
									this.Max.CPU.max = s.CPU;
									this.Max.CPU.date = s.Time;
								}
								if (s.Memory >= this.Max.Memo.max) {
									this.Max.Memo.max = s.Memory;
									this.Max.Memo.date = s.Time;
								}
							});
						}
					}

					// 重新定时
					this.ticks(true);
				})
				.catch(res => {
					// 发生错误
					this.ticks(false);
				});
		},

		// 计算 MB
		GetMB(data) {
			return data ? data / 1024 / 1024 : 0;
		},

		// 发送接收字节比
		BytesPercent(m) {
			let ret = 0;
			if (m == 2) {
				ret = this.Now.SendBytes / (this.Now.ReceivBytes + this.Now.SendBytes);
				return Math.round(ret * 10000) / 100;
			} else if (m == 1) {
				ret = this.Now.ReceivBytes / (this.Now.ReceivBytes + this.Now.SendBytes);
				return Math.round(ret * 10000) / 100;
			} else {
				ret = this.Now.SendBytes / this.Now.ReceivBytes;
				return ret.toFixed(2);
			}
		},

		// 展示最大数据
		MaxValue(val, name, unit) {
			return '最高：' + val.max.toFixed(2) + unit + '，时间：' + moment(val.date).format('MM/DD HH:mm:ss');
		},

		loadCodeUrls(data) {
			this.Url.codes = data;
		},

		loadTimeUrls(data) {
			this.Url.times = data;
		}
	},
	computed: {
		// 服务器端口
		ServerHostPort() {
			const port = this.Server.SSL ? this.Server.SSLPort : this.Server.Port;
			const host = this.Server.Host || '0.0.0.0';
			const ssl = this.Server.SSL ? '(SSL)' : '';

			return host + ':' + port + ssl;
		},

		// 系统启动时间
		ServerStartTime() {
			if (this.Server.StartTime) {
				return '系统启动时间：' + moment(this.Server.StartTime).format('YYYY年MM月DD日HH时mm分ss秒');
			}
			return '';
		},

		// 系统运行时长
		SystemRuntime() {
			if (this.RunTime && this.RunTime.Seconds && this.RunTime.Seconds >= 0) {
				let ret = this.RunTime.Hours + ':' + this.RunTime.Minutes + ':' + this.RunTime.Seconds;
				if (this.RunTime.Days) ret = this.RunTime.Days + 'd / ' + ret;
				return ret;
			}
			return '';
		},

		// 系统启动后累计访问数
		CounterALL() {
			if (this.Statistics && this.Statistics.Counter && this.Statistics.Counter.length == 7) {
				return this.Statistics.Counter[0];
			}
			return 0;
		},

		// 系统启动后累计访问成功数
		CounterSucc() {
			if (this.Statistics && this.Statistics.Counter && this.Statistics.Counter.length == 7) {
				return this.Statistics.Counter[2];
			}
			return 0;
		},

		// CPU 图表数据
		chartCPU() {
			const ret = [];

			if (this.Status && this.Status.length > 0) {
				this.Status.forEach(s => {
					ret.push([s.Time, s.CPU]);
				});
			}

			return ret;
		},

		// 内存图表数据
		chartMemo() {
			const ret = [];

			if (this.Status && this.Status.length > 0) {
				this.Status.forEach(s => {
					ret.push([s.Time, s.Memory]);
				});
			}

			return ret;
		},

		// 请求图表数据
		chartRequest() {
			const ret = [];

			if (this.Status && this.Status.length > 0) {
				this.Status.forEach(s => {
					ret.push([s.Time, s.Request]);
				});
			}

			return ret;
		},

		// 最新请求率
		lastRequest() {
			if (this.Status && this.Status.length > 0) {
				return this.Status[this.Status.length - 1].Request;
			}
			return 0;
		},

		// 最新连接率
		lastConnections() {
			if (this.Status && this.Status.length > 0) {
				return this.Status[this.Status.length - 1].Connections;
			}
			return 0;
		},

		// 请求与连接占比
		RequestVsConnections() {
			return (this.Now.Request / (this.Now.Request + this.Now.Connections)) * 100;
		}
	}
};
</script>

<style scoped>
.ant-divider-horizontal {
	margin: 16px 0;
}
.box {
	background-color: #ffffff;
	padding: 16px;
	border: solid 1px #efefef;
}
.box .chart {
	height: 60px;
	overflow: hidden;
}
.box .info {
	white-space: nowrap;
	overflow: hidden;
	text-overflow: ellipsis;
}
.sysInfo {
	min-width: 120px;
}
</style>
