<template>
	<div class="editable-cell" style="word-wrap:break-word;word-break:break-all;">
		<div v-if="editable" class="editable-cell-input-wrapper">
			<a-textarea autoSize :value="isArray ? dataValue.join('\n') : dataValue" @change="handleChange" />
			<a-icon type="check" class="editable-cell-icon-check" @click="check" />
		</div>
		<div v-else class="editable-cell-text-wrapper">
			<template v-if="isArray">
				<a-tag v-for="(item, index) in dataValue" :key="index">{{ item }}</a-tag>
			</template>
			<span v-else>{{ dataValue || defaultValue || ' ' }}</span>
			<a-icon type="edit" class="editable-cell-icon" @click="edit" />
		</div>
	</div>
</template>

<script>
export default {
	props: {
		value: [String, Array],
		defaultValue: String,
		isArray: Boolean
	},
	data() {
		return {
			dataValue: this.value,
			editable: false
		};
	},
	methods: {
		handleChange(e) {
			const text = e.target.value || '';
			this.dataValue = this.isArray ? text.split('\n') : text;
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
		}
	}
};
</script>

<style></style>
