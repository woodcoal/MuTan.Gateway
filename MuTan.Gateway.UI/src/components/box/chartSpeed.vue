<template>
	<div id="chartTimes"></div>
</template>

<script>
import moment from 'moment';
import { EleResize } from '../../static/js/eleresize.js';

export default {
	name: 'boxChartSpeed',
	props: ['datas', 'title'],
	data() {
		return {
			chart: null
		};
	},
	mounted() {
		// 图标对象
		const chartId = document.getElementById('chartTimes');

		// 初始化图表
		this.chart = this.$chart.init(chartId);

		// Y 轴标识
		const chartY = ['<10ms', '10-50ms', '50-100ms', '100-200ms', '200-500ms', '0.5-1s', '1-2s', '2-5s', '>5s'];

		// 开始渲染
		this.chart.setOption({
			title: { text: this.title || '速度统计', x: 'center', y: 'top' },
			tooltip: {
				position: 'top',
				formatter: function(p) {
					if (p && p.data && p.data.value && p.data.value.length > 2) {
						const value = p.data.value;
						if (value[2]) return value[0] + ' / ' + chartY[value[1]] + '：' + value[2];
					}
					return '';
				}
			},
			grid: {
				left: '80px',
				right: '48px',
				top: '48px',
				bottom: '64px'
			},
			xAxis: {
				axisTick: {
					show: false
				},
				type: 'category',
				splitArea: {
					show: true
				}
			},
			yAxis: {
				axisTick: {
					show: false
				},
				type: 'category',
				data: chartY,
				splitArea: {
					show: true
				}
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
					type: 'heatmap',
					emphasis: {
						itemStyle: {
							shadowBlur: 10,
							shadowColor: 'rgba(0, 0, 0, 0.5)'
						}
					}
				}
			]
		});

		// 点击事件
		this.chart.on('click', pars => {
			if (pars.value[2]) this.$emit('chartClick', { date: pars.value[3], name: pars.value[1] });
		});

		// 自动调整大小
		EleResize.on(chartId, () => {
			this.chart.resize();
		});
	},
	methods: {
		makeColor(chartData, data) {
			const im = { value: data, itemStyle: { color: '#FFFFFF', opacity: 0.8, borderWidth: 1, borderColor: 'rgba(255, 255, 255, 0.5)' } };

			if (!im.value[2]) {
				im.itemStyle.color = '#FCFCFC';
			} else {
				switch (im.value[1]) {
					case 0:
						im.itemStyle.color = '#00A505';
						break;
					case 1:
						im.itemStyle.color = '#2CDD00';
						break;
					case 2:
						im.itemStyle.color = '#99FF00';
						break;
					case 3:
						im.itemStyle.color = '#FFFF00';
						break;
					case 4:
						im.itemStyle.color = '#FFCC00';
						break;
					case 5:
						im.itemStyle.color = '#FF9900';
						break;
					case 6:
						im.itemStyle.color = '#FF6600';
						break;
					case 7:
						im.itemStyle.color = '#FF3300';
						break;
					case 8:
						im.itemStyle.color = '#771800';
						break;
				}
			}
			chartData.push(im);
		},

		// 将请求时间转成时间对象
		makeDate(dt) {
			dt = 202000000000 + parseInt(dt);
			let dn = dt.toString();
			dn = dn.substr(0, 8) + 'T' + dn.substr(8);
			return moment(dn).toDate();
		}
	},
	watch: {
		datas(values) {
			const chartData = [];

			if (values && values.length > 0) {
				values.forEach(s => {
					if (s) {
						const d = new Date(s['Time']);
						const ds = moment(d).format('MM/DD HH:mm');

						this.makeColor(chartData, [ds, 0, s['10'], d]);
						this.makeColor(chartData, [ds, 1, s['10_50'], d]);
						this.makeColor(chartData, [ds, 2, s['50_100'], d]);
						this.makeColor(chartData, [ds, 3, s['100_200'], d]);
						this.makeColor(chartData, [ds, 4, s['200_500'], d]);
						this.makeColor(chartData, [ds, 5, s['500_1000'], d]);
						this.makeColor(chartData, [ds, 6, s['1000_2000'], d]);
						this.makeColor(chartData, [ds, 7, s['2000_5000'], d]);
						this.makeColor(chartData, [ds, 8, s['5000'], d]);
					}
				});

				this.chart.setOption({
					series: [{ data: chartData }]
				});
				this.chart.resize();
			}
		},
		title(value) {
			this.chart.setOption({
				title: { text: value || '速度统计' }
			});
		}
	}
};
</script>

<style scoped>
#chartTimes {
	min-height: 480px;
}
</style>
