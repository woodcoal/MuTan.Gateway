<template>
	<a-config-provider :locale="locale"><BasicLayout /></a-config-provider>
</template>

<script>
import BasicLayout from './layouts/BasicLayout.vue';

import zh_CN from 'ant-design-vue/lib/locale-provider/zh_CN';

import 'moment/locale/zh-cn';
import moment from 'moment';
moment.locale('zh-cn');

export default {
	components: { BasicLayout },
	data() {
		return {
			locale: zh_CN
		};
	}
};
</script>

<style></style>
