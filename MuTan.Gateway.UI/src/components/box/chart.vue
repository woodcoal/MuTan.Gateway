<template>
	<div :id="chartId" class="boxChart"></div>
</template>

<script>
import moment from 'moment';
import { EleResize } from '../../static/js/eleresize.js';

export default {
	name: 'boxChart',
	props: ['id', 'data', 'dateformat', 'color', 'count', 'bar', 'showtip', 'unit'],
	data() {
		return {
			chart: null,
			chartId:
				this.id ||
				Math.random()
					.toString(36)
					.slice(-8),
			chartData: [],
			chartCount: 100,
			hasCache: false
		};
	},
	created() {
		// this.chartId =
		// 	this.id ||
		// 	Math.random()
		// 		.toString(36)
		// 		.slice(-8);

		// 如果未设置 id 则不缓存
		if (this.id) {
			this.hasCache = true;

			// 从缓存获取数据
			this.chartData = this.$cache.get('boxChart_' + this.id) || [];
		} else {
			this.hasCache = false;
		}

		// 显示数据，缓存数据条数
		this.chartCount = this.count || this.chartCount;
		if (this.chartCount < 10) this.chartCount = 10;
		if (this.chartCount > 120) this.chartCount = 120;
	},
	mounted() {
		// 时间格式化
		const fmt = this.dateformat || 'HH:mm:ss';

		// 单位
		const unit = this.unit || '';

		// 图标样式
		const chartType = this.bar ? 'bar' : 'line';

		// 图标对象
		const chartId = document.getElementById(this.chartId);

		// 显示提示信息
		const chartTip = !(this.showtip === false);

		// 初始化图表
		this.chart = this.$chart.init(chartId);
		// 开始渲染
		this.chart.setOption({
			tooltip: {
				trigger: 'axis',
				position: [10, 10],
				backgroundColor: 'rgba(250,250,250,0.7)',
				textStyle: { color: '#888' },
				formatter: function(datas) {
					return moment(datas[0].value[0]).format(fmt) + '：' + datas[0].value[1].toFixed(2) + unit;
				},
				show: chartTip
			},
			grid: {
				left: '5px',
				right: '5px',
				top: '0',
				bottom: '0'
			},

			xAxis: {
				type: 'time',
				splitLine: {
					show: false
				}
			},
			yAxis: {
				type: 'value',
				show: false
			},
			series: [
				{
					type: chartType,
					showSymbol: false,
					hoverAnimation: false,
					data: this.chartData,
					smooth: true,
					symbol: 'circle',
					symbolSize: 0,
					itemStyle: {
						normal: {
							color: this.color || '#ff5050',
							areaStyle: {
								type: 'default',
								opacity: 0.1
							},
							lineStyle: {
								opacity: 0.5,
								width: 2
							}
						}
					}
				}
			]
		});

		// 自动调整大小
		EleResize.on(chartId, () => {
			this.chart.resize();
		});
	},
	watch: {
		data() {
			// if (!this.data && this.data !== 0) return;
			let data = this.data;
			if (!data && data !== 0) return;
			if (Array.isArray(data)) {
				if (data.length < 1) return;
				data = data[0];
			}

			this.chartData.push([new Date(), data]);

			while (this.chartData.length > this.chartCount) {
				this.chartData.shift();
			}

			this.chart.setOption({
				series: [{ data: this.chartData }]
			});

			// 缓存数据
			if (this.hasCache) {
				this.$cache.set('boxChart_' + this.id, this.chartData);
			}
		}
	}
};
</script>

<style scoped>
.boxChart {
	width: 100%;
	height: 100%;
}
</style>
