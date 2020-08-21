<template>
	<div class="editable-cell">
		<div v-if="editable" class="editable-cell-input-wrapper">
			<a-date-picker :showTime="showTime" :format="format" v-model="dataValue" />
			<a-icon type="check" class="editable-cell-icon-check" @click="check" />
		</div>
		<div v-else class="editable-cell-text-wrapper">
			{{ dataValue.format(this.dataFormat) }}
			<a-icon type="edit" class="editable-cell-icon" @click="edit" />
		</div>
	</div>
</template>

<script>
import moment from 'moment';

export default {
	props: {
		value: [String, Date],
		format: String,
		showTime: Boolean,
		defaultValue: String
	},
	data() {
		return { dataValue: moment(this.value), dataFormat: this.format || this.showTime ? 'YYYY-MM-DD HH:mm:ss' : 'YYYY-MM-DD', editable: false };
	},
	methods: {
		check() {
			this.editable = false;

			// 如果原始内容与当前内容不同则标识有变化
			if (!this.dataValue.isSame(this.value)) {
				this.$emit('change', this.dataValue.format(this.dataFormat));
				this.$emit('input', this.dataValue.format(this.dataFormat));
			}
		},
		edit() {
			this.editable = true;
		}
	},
	watch: {
		value(value) {
			if (!this.dataValue.isSame(value)) this.dataValue = moment(value);
		}
	}
};
</script>

<style></style>
