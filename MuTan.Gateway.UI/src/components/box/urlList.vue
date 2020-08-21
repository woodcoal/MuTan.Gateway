<template>
	<div v-if="data">
		<h3 style="text-align: center;padding: 24px 0 8px;">{{ title }}</h3>
		<a-list item-layout="horizontal" :data-source="data" class="item">
			<a-list-item slot="renderItem" slot-scope="item, index">
				<a-list-item-meta>
					<a-avatar size="large" :style="avatarColor(index)" slot="avatar">{{ index + 1 }}</a-avatar>
					<div slot="title">{{ item.url }}</div>
					<a-progress slot="description" :percent="(item.count / max) * 100" :show-info="false" />
				</a-list-item-meta>
				<h4 style="padding-left:16px;">{{ item.count }} 次</h4>
			</a-list-item>
		</a-list>
	</div>
</template>

<script>
import moment from 'moment';
export default {
	name: 'boxUrlList',
	props: ['date', 'name', 'count', 'interval'],
	data() {
		return {
			isRun: false, //是否已经在请求，防止重复请求
			title: '',
			data: null,
			max: 0
		};
	},
	mounted() {
		this.showUrls();
		this.isRun = false;
	},
	methods: {
		// 获取网址列表
		showUrls() {
			if (this.isRun) return;
			this.isRun = true;

			this.title = '';
			this.subtitle = '';
			this.data = null;
			this.max = 0;

			if (this.date) {
				let count = parseInt(this.count);
				if (count < 1) count = 10;

				const now = this.makeTime(this.date);
				const dt = now.getTime();
				// console.log(this.date, now, this.date.getTime(), dt);
				const start = moment(now).format('MM/DD HH:mm');
				const end = moment(now)
					.add(this.dateInterval, 'minute')
					.format('MM/DD HH:mm');

				this.title = start + '~' + end;

				let name = '';
				let subtitle = '';
				switch (this.name) {
					case 0:
						name = '10';
						subtitle = '耗时10ms内';
						break;
					case 1:
						name = '10_50';
						subtitle = '耗时 10-50ms';
						break;
					case 2:
						name = '50_100';
						subtitle = '耗时 50-100ms';
						break;
					case 3:
						name = '100_200';
						subtitle = '耗时 100-200ms';
						break;
					case 4:
						name = '200_500';
						subtitle = '耗时 200-500ms';
						break;
					case 5:
						name = '500_1000';
						subtitle = '耗时 0.5-1s';
						break;
					case 6:
						name = '1000_2000';
						subtitle = '耗时 1-2s';
						break;
					case 7:
						name = '2000_5000';
						subtitle = '耗时 2-5s';
						break;
					case 8:
						name = '5000';
						subtitle = '耗时 5s 以上';
						break;
					case 'all':
						name = 'ALL';
						subtitle = '所有请求';
						break;
					case 'succ':
						name = '2x';
						subtitle = '成功请求（Code:2xx）';
						break;
					case 'err':
						name = '5x';
						subtitle = '错误请求（Code:5xx）';
						break;
					default:
						this.isRun = false;
						this.title = '';
						return;
				}

				this.title += ' ' + subtitle + ' 访问排行';

				this.$axios
					.get('/_gateway/status/topurl', { d: dt, name: name, count: count })
					.then(res => {
						if (res && res.Data) {
							this.data = [];

							for (let url in res.Data) {
								const count = res.Data[url];
								this.data.push({ url, count });

								this.max += count;
							}
						}
						this.isRun = false;
					})
					.catch(() => {
						this.isRun = false;
					});
			}
		},

		// 将时间对象转换成请求时间
		makeTime(dt) {
			const mom = moment(dt);
			const min = parseInt(mom.minute() / this.dateInterval) * this.dateInterval;
			return mom
				.minute(min)
				.second(0)
				.millisecond(0)
				.toDate();
		},

		// 头像颜色
		avatarColor(index) {
			switch (index) {
				case 0:
					return { color: '#fff', backgroundColor: '#f50' };
					break;
				case 1:
					return { color: '#fff', backgroundColor: 'orange' };
					break;
				case 2:
					return { color: '#fff', backgroundColor: '#080' };
					break;
				default:
					return null;
			}
		}
	},
	computed: {
		dateInterval() {
			if (this.interval) {
				return parseInt(this.interval);
			}
			return 30;
		}
	},
	watch: {
		date() {
			this.showUrls();
		},
		name() {
			this.showUrls();
		}
	}
};
</script>

<style scoped>
.item div {
	white-space: nowrap;
	overflow: hidden;
	text-overflow: ellipsis;
}
</style>
