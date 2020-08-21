'--------------------------------------------------   
'   
'   请求缓存   
'   
'   namespace: Plugin.RequestCache
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 缓存指定的请求
'   release: 2020-08-16
'   
'-------------------------------------------------- 

Imports System.Reflection
Imports Bumblebee.Events
Imports Bumblebee.Plugins
Imports Newtonsoft.Json.Linq

Namespace Plugin

	<RouteBinder(ApiLoader:=False)>
	Public Class RequestCache
		Implements IRequestingHandler, Model.IPlugin, IPluginStatus

		''' <summary>启用</summary>
		Public Property Enabled As Boolean Implements IPluginStatus.Enabled
			Get
				Return xCache.Enabled
			End Get
			Set(value As Boolean)
				xCache.Enabled = value
			End Set
		End Property

		''' <summary>名称</summary>
		Public ReadOnly Property Name As String = Me.GetType.Name Implements IPlugin.Name

		''' <summary>描述</summary>
		Public ReadOnly Property Description As String = "API 缓存（读缓存）" Implements IPlugin.Description

		''' <summary>插件级别</summary>
		Public ReadOnly Property Level As PluginLevel = PluginLevel.None Implements IPlugin.Level

		''' <summary>是否需要设置参数</summary>
		Public ReadOnly Property EnSetting As Boolean = False Implements Model.IPlugin.EnSetting

		''' <summary>扩展展示信息</summary>
		Public ReadOnly Property Information As Dictionary(Of String, String) = Nothing Implements Model.IPlugin.Information

		''' <summary>执行操作，读取缓存</summary>
		Public Sub Execute(e As EventRequestingArgs) Implements IRequestingHandler.Execute
			If Not Enabled Then Exit Sub
			xCache.Read(e)
		End Sub

		''' <summary>初始化</summary>
		Public Sub Init(gateway As Global.Bumblebee.Gateway, assembly As Assembly) Implements IPlugin.Init
			gateway.HttpServer.ActionFactory.Register(New API.Cache())
		End Sub

		''' <summary>加载配置</summary>
		Public Sub LoadSetting(setting As JToken) Implements IPlugin.LoadSetting
		End Sub

		''' <summary>保存配置</summary>
		Public Function SaveSetting() As Object Implements IPlugin.SaveSetting
			Return Nothing
		End Function
	End Class

End Namespace