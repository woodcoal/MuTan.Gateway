# MuTan.Gateway
.Net Core API 网关系统，基于 Bumblebee 扩展


本系统基于 IKende 的 Bumblebee 扩展开发，将基本插件集成于网关系统中。分离原有系统的管理后台，采用 Ant Design of Vue 制作新后台。

## 目前集成插件有：
1. 访问统计，含详细统计数据
2. 基于 JWT 的访问授权
3. 基于内存的API缓存
4. Rewrite 跳转
5. 非法请求拦截

## 扩展功能：
丰富了 API 授权、缓存、拦截规则，支持域名，路径、请求参数、浏览器头、Cookies 验证。

### 扩展规则格式：Host@Path|h:Header|d:Data|c:Cookies
· Host/Path 之间用 @ 间隔
· Header/Data/Cookies 分别用|h: |d: |c: 间隔区分
· Host/Path/Header/Data/Cookies 每段都支持正则表达式
### 举例：
· good => 所有路径中包含 good 的地址
· a.com@^/api => a.com 下所有以 api 开头的地址
· ^/api|h:name => 所有以 api 开头，请求头部数据包含 name 的请求
· ^/api|c:id\= => 所有以 api 开头，Cookies 包含 id 字段的请求

 

## 开发环境
· 网关系统：VB.Net
· 管理后台：Antd Vue
· 如果需要了解 Bumblebee API 网关请移步：https://github.com/IKende/Bumblebee
· 如果想简化 VUE 开发，减少参数设置，对于初学者建议熟悉 hey-cli，具体请移步：https://github.com/heyui/hey-cli


## 补充说明
· Wild Coder，没专业学习过开发设计，水平有限
· 对 Github 不是很熟悉，功能在慢慢了解中，操作不熟练敬请谅解
· .Net 开发目前 C# 还是主流，由于本人 C# 能力仅限于可以看懂，能修改片段的级别。所以采用的是自己熟悉的 VB.Net 制作，其实 .Net 各语言之间相通性，基本上都可以无损自由转换，有兴趣的可以自行改成 C#。
· 管理后台使用 hey-cli 环境，由于其简化大部分环境设置，所以很适合初学者。如果您已经有配置好的环境，直接将 src 中代码复制过去基本上就可以用了。
