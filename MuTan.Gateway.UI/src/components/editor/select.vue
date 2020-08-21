<template>
	<div class="editable-cell">
		<div v-if="editable" class="editable-cell-input-wrapper">
			<a-select v-model="dataValue">
				<a-select-option v-for="(d, i) in dataList" :value="d.key" :key="i">{{ d.value }}</a-select-option>
			</a-select>
			<a-icon type="check" class="editable-cell-icon-check" @click="check" />
		</div>
		<div v-else class="editable-cell-text-wrapper">
			{{ dataValue || defaultValue || ' ' }}
			<a-icon type="edit" class="editable-cell-icon" @click="edit" />
		</div>
	</div>
</template>

<script>
export default {
	props: {
		value: [String, Number],
		defaultValue: String,
		datas: [Array, Object]
	},
	data() {
		return {
			dataValue: this.value,
			dataList: [],
			editable: false
		};
	},
	mounted() {
		this.updateDatas();
	},
	methods: {
		//判断是否是数组
		isArray(obj) {
			return Object.prototype.toString.call(obj) == '[object Array]';
		},

		//判断是否是对象
		isObject(obj) {
			return Object.prototype.toString.call(obj) == '[object Object]';
		},

		// 判断是否是字符串
		isString(obj) {
			return Object.prototype.toString.call(obj) == '[object String]';
		},

		updateDatas() {
			this.dataList = [];
			if (this.datas) {
				if (this.isArray(this.datas) && this.datas.length > 0) {
					this.datas.forEach(d => {
						if (d) {
							if (this.isString(d)) {
								this.dataList.push({ key: d, value: d });
							} else if (this.isObject(d)) {
								if (d.key && d.value) {
									this.dataList.push(d);
								}
							}
						}
					});
				} else if (this.isObject(this.datas)) {
					for (let k in this.datas) {
						this.dataList.push({ key: k, value: this.datas(k) });
					}
				}
			}
		},

		check() {
			this.editable = false;

			// 如果原始内容与当前内容不同则标识有变化
			if (this.dataValue !== this.value) {
				this.$emit('change', this.dataValue);
				this.$emit('input', this.dataValue);
			}
		},
		edit() {
			this.editable = true;
		}
	},
	watch: {
		value(value) {
			if (this.dataValue !== value) this.dataValue = value;
		},

		datas() {
			this.updateDatas();
		}
	}
};
</script>

<style></style>
