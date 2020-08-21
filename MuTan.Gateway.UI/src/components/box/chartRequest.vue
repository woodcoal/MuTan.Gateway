<template>
	<div id="chartCodes"></div>
</template>

<script>
import moment from 'moment';
import { EleResize } from '../../static/js/eleresize.js';

export default {
	name: 'boxChartRequest',
	props: ['datas', 'title'],
	data() {
		return {
			chart: null
		};
	},
	mounted() {
		// 图标对象
		const chartId = document.getElementById('chartCodes');

		// 初始化图表
		this.chart = this.$chart.init(chartId);

		// 开始渲染
		this.chart.setOption({
			title: { text: this.title || '请求统计', x: 'center', y: 'top' },
			legend: { top: '48px' },
			tooltip: {
				trigger: 'axis'
			},
			grid: {
				left: '48px',
				right: '48px',
				top: '80px',
				bottom: '80px'
			},
			xAxis: {
				splitLine: {
					show: false
				},
				axisTick: {
					show: false
				},
				axisLine: {
					show: false
				},
				type: 'time'
			},
			yAxis: {
				splitLine: {
					style: ''
				},
				axisTick: {
					show: false
				},
				axisLine: {
					show: false
				},
				type: 'value',
				scale: false
			},
			dataZoom: [
				{
					type: 'slider',
					show: true,
					xAxisIndex: [0]
				},
				{
					type: 'inside',
					xAxisIndex: [0]
				}
			],
			series: [
				{
					id: 'all',
					name: '所有',
					type: 'line',
					showSymbol: false,
					hoverAnimation: false,
					smooth: true,
					symbol: 'circle',
					symbolSize: 10,
					color: '#55aaff',
					itemStyle: {
						normal: {
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
				},
				{
					id: 'succ',
					name: '成功',
					type: 'line',
					showSymbol: false,
					hoverAnimation: false,
					smooth: true,
					symbol: 'circle',
					symbolSize: 10,
					color: '#008000'
				},
				{
					id: 'err',
					name: '错误',
					type: 'line',
					showSymbol: false,
					hoverAnimation: false,
					smooth: true,
					symbol: 'circle',
					symbolSize: 10,
					color: '#dd0000'
				}
			]
		});

		// 点击事件
		this.chart.on('click', pars => {
			this.$emit('chartClick', { date: pars.data[0], name: pars.seriesId });
		});

		// 自动调整大小
		EleResize.on(chartId, () => {
			this.chart.resize();
		});
	},
	watch: {
		datas(values) {
			const _all = [];
			const _succ = [];
			const _err = [];

			if (values && values.length > 0) {
				values.forEach(s => {
					if (s) {
						const d = new Date(s['Time']);

						_all.push([d, s['ALL']]);
						_succ.push([d, s['2x']]);
						_err.push([d, s['5x']]);
					}
				});

				this.chart.setOption({
					series: [{ data: _all }, { data: _succ }, { data: _err }]
				});
				this.chart.resize();
			}
		},
		title(value) {
			this.chart.setOption({
				title: { text: value || '请求统计' }
			});
		}
	}
};
</script>

<style scoped>
#chartCodes {
	min-height: 480px;
}
</style>
