'--------------------------------------------------   
'   
'   网关管理API加载
'   
'   namespace: Plugin.ManageAPI
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 网关管理API加载
'   release: 2020-07-22
'   
'-------------------------------------------------- 

Imports System.Reflection
Imports Bumblebee.Events
Imports Bumblebee.Plugins
Imports Newtonsoft.Json.Linq

Namespace Plugin

	''' <summary>网关管理API加载</summary>
	<RouteBinder(RouteUrl:="^/_gateway/*", ApiLoader:=False)>
	Public Class ManageAPI
		Implements IRequestingHandler, Model.IPlugin

		''' <summary>强制启用</summary>
		Public Property Enabled As Boolean = True

		''' <summary>名称</summary>
		Public ReadOnly Property Name As String = Me.GetType.Name Implements IPlugin.Name

		''' <summary>描述</summary>
		Public ReadOnly Property Description As String = "网关管理API加载插件" Implements IPlugin.Description

		''' <summary>插件级别</summary>
		Public ReadOnly Property Level As PluginLevel = PluginLevel.None Implements IPlugin.Level

		''' <summary>是否需要设置参数</summary>
		Public ReadOnly Property EnSetting As Boolean = False Implements Model.IPlugin.EnSetting

		''' <summary>扩展展示信息</summary>
		Public ReadOnly Property Information As Dictionary(Of String, String) = Nothing Implements Model.IPlugin.Information

		''' <summary>初始化</summary>
		Public Sub Init(Gateway As Bumblebee.Gateway, assembly As Assembly) Implements Bumblebee.Plugins.IPlugin.Init
			Gateway.HttpServer.ActionFactory.Register(New API.Config(Gateway))
			Gateway.HttpServer.ActionFactory.Register(New API.Plugin(Gateway))
			Gateway.HttpServer.ActionFactory.Register(New API.Rewrite(Gateway))
			Gateway.HttpServer.ActionFactory.Register(New API.Route(Gateway))
			Gateway.HttpServer.ActionFactory.Register(New API.Server(Gateway))
		End Sub

		''' <summary>执行操作</summary>
		Public Sub Execute(e As EventRequestingArgs) Implements IRequestingHandler.Execute
			e.Cancel = True
			e.ResultType = ResultType.None
		End Sub

		''' <summary>加载配置</summary>
		Public Sub LoadSetting(setting As JToken) Implements Bumblebee.Plugins.IPlugin.LoadSetting
		End Sub

		''' <summary>保存配置</summary>
		Public Function SaveSetting() As Object Implements Bumblebee.Plugins.IPlugin.SaveSetting
			Return Nothing
		End Function

	End Class

End Namespace