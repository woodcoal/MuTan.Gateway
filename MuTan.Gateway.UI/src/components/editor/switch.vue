<template>
	<div class="editable-cell">
		<a-switch v-model="dataValue" :disabled="!editable" style="float: left;" />
		<div v-if="editable" class="editable-cell-input-wrapper"><a-icon type="check" class="editable-cell-icon-check" @click="check" /></div>
		<div v-else class="editable-cell-text-wrapper"><a-icon type="edit" class="editable-cell-icon" @click="edit" /></div>
	</div>
</template>

<script>
export default {
	props: {
		value: Boolean
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
