<template>
	<div class="editable-cell">
		<div v-if="editable" class="editable-cell-input-wrapper">
			<a-input-number v-model="dataValue" :min="min" :max="max" :precision="precision" />
			<a-icon type="check" class="editable-cell-icon-check" @click="check" />
		</div>
		<div v-else class="editable-cell-text-wrapper">
			{{ dataValue || '0' }}
			<a-icon type="edit" class="editable-cell-icon" @click="edit" />
		</div>
	</div>
</template>

<script>
export default {
	props: {
		value: Number,
		min: Number,
		max: Number,
		precision: Number
	},
	data() {
		return {
			dataValue: this.value,
			editable: false
		};
	},
	methods: {
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
		}
	}
};
</script>

<style></style>
