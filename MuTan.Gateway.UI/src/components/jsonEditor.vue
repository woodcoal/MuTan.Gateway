<template>
	<div :id="editorID" style="width: auto;height: 400px;"></div>
</template>

<script>
import JsonEditor from 'jsoneditor';
export default {
	props: { id: String, value: [String, Object, Array], readonly: Boolean, editmode: Boolean },
	data() {
		return {
			editorID:
				this.id ||
				Math.random()
					.toString(36)
					.slice(-8),

			jsonEditor: null,
			jsonValue: '' // 提交的 Json 内容，防止提交后又 Watch 到相同数据，导致编辑框刷新，光标位置变化
		};
	},
	mounted() {
		const it = this;
		const container = document.getElementById(this.editorID);
		const options = {
			mode: this.readonly ? 'preview' : this.editmode ? 'code' : 'preview',
			modes: this.readonly ? null : ['code', 'tree', 'preview'],
			indentation: 2,
			search: true,
			onChange() {
				it.$emit('change', it);

				try {
					it.jsonValue = it.jsonEditor.get();
					it.$emit('input', it.jsonValue);
				} catch (e) {
				}
			},
			onError(error) {
				it.$emit('error', error);
			}
		};

		this.jsonEditor = new JsonEditor(container, options);
		this.jsonEditor.set(this.value || {});
	},

	watch: {
		value: {
			handler(newVal) {
				if (this.jsonValue == newVal) return;
				if (!this.jsonEditor) return;

				this.jsonEditor.set(newVal || {});
			},
			deep: true,
			immediate: true
		}
	}
};
</script>

<style></style>
