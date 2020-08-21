/**
 * 本地数据存储
 * 支持时长限制
 *
 * 木炭 2020-05-09
 *
 * 参考：
 * 本地存储实现,封装 localStorage 和 sessionStorage
 * https://github.com/ustbhuangyi/storage/blob/master/src/index.js
 */

const isServer = typeof window === 'undefined';

const store = {
	/* eslint-disable no-undef */
	version: '1.20.5',
	storage: !isServer ? window.localStorage : null,
	session: {
		storage: !isServer ? window.sessionStorage : null
	}
};

// 操作接口
const api = {
	// 设置缓存
	set(key, val, exp) {
		if (this.disabled) return;

		//调整 key
		key = makeKey(key);

		if (val === undefined || val === null) {
			return this.storage.removeItem(key);
		}

		val = serialize(val, exp);

		//console.log('缓存:', key, val);

		this.storage.setItem(key, val);
		return val;
	},
	// 获取缓存
	get(key, def) {
		if (this.disabled) return def;

		//调整 key
		key = makeKey(key);

		const val = deserialize(this.storage.getItem(key));

		//console.log('获取缓存:', key, val);

		if (val === undefined || val === null) {
			// 移除此键
			this.storage.removeItem(key);
			return def;
		} else {
			return val;
		}
	},

	has(key) {
		key = makeKey(key);
		return this.get(key, null) !== null;
	},

	remove(key) {
		if (this.disabled) return;

		key = makeKey(key);

		this.storage.removeItem(key);
	},

	clear() {
		if (this.disabled) return;
		this.storage.clear();
	},

	getAll() {
		if (this.disabled) return null;

		let ret = {};
		this.forEach((key, val) => {
			ret[key] = val;
		});
		return ret;
	},

	forEach(callback) {
		if (this.disabled) return;

		for (let i = 0; i < this.storage.length; i++) {
			let key = this.storage.key(i);
			callback(key, this.get(key));
		}
	}
};

Object.assign(store, api);

Object.assign(store.session, api);

function serialize(val, exp) {
	if (val === undefined || val === null) return;

	const obj = {
		val: val
	};

	exp = parseInt(exp);
	if (isNaN(exp) || exp < 1) {
		// 永久缓存
		obj.exp = new Date(2050, 1, 1, 0, 0, 0, 0).getTime();
	} else {
		obj.exp = new Date().getTime() + exp * 1000;
	}

	return JSON.stringify(obj);
}

function deserialize(val) {
	if (typeof val === 'string') {
		try {
			const obj = JSON.parse(val);
			return new Date().getTime() <= obj.exp ? obj.val : null;
		} catch (e) {}
	}

	return null;
}

// 调整 key ，如果 key 为对象则取 hash 值
function makeKey(key) {
	if (key === undefined || key === null) return '_err_';
	if (typeof val === 'string') return key;

	key = JSON.stringify(key);

	let hash = 0;
	for (let i = 0; i < key.length; i++) {
		let character = key.charCodeAt(i);
		hash = (hash << 5) - hash + character;
		hash = hash & hash; // Convert to 32bit integer
	}
	return '_' + hash;
}

try {
	const testKey = '__store_check__';
	store.set(testKey, testKey, 3);
	store.disabled = store.get(testKey) !== testKey;
	store.remove(testKey);
} catch (e) {
	store.disabled = true;
}

export default store;
