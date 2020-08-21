'--------------------------------------------------   
'   
'   插件接口
'   
'   namespace: Model.IPlugin
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 插件接口，在原接口的基础上扩展了是否需要设置的属性
'   release: 2020-08-12
'   
'-------------------------------------------------- 

Namespace Model

	''' <summary>插件接口</summary>
	Public Interface IPlugin
		Inherits Bumblebee.Plugins.IPlugin

		''' <summary>是否允许设置</summary>
		ReadOnly Property EnSetting As Boolean

		''' <summary>扩展展示信息</summary>
		ReadOnly Property Information As Dictionary(Of String, String)

	End Interface
End Namespace
