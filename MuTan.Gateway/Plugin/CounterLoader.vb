'--------------------------------------------------   
'   
'   统计加载
'   
'   namespace: Plugin.CounterLoader
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 统计加载
'   release: 2020-07-23
'   
'-------------------------------------------------- 

Imports System.Reflection
Imports Bumblebee.Events
Imports Bumblebee.Plugins
Imports Newtonsoft.Json.Linq

Namespace Plugin

	''' <summary>统计加载</summary>
	Public Class CounterLoader
		Implements IGatewayLoader, IPluginStatus, Model.IPlugin

		Private Log As Counter.Log
		Private Status As Counter.Status

		''' <summary>强制启用</summary>
		Public Property Enabled As Boolean = True Implements IPluginStatus.Enabled

		''' <summary>名称</summary>
		Public ReadOnly Property Name As String = Me.GetType.Name Implements IPlugin.Name

		''' <summary>描述</summary>
		Public ReadOnly Property Description As String = "网关访问记录与统计" Implements IPlugin.Description

		''' <summary>插件级别</summary>
		Public ReadOnly Property Level As PluginLevel = PluginLevel.None Implements IPlugin.Level

		''' <summary>是否需要设置参数</summary>
		Public ReadOnly Property EnSetting As Boolean = False Implements Model.IPlugin.EnSetting

		''' <summary>扩展展示信息</summary>
		Public ReadOnly Property Information As Dictionary(Of String, String) = Nothing Implements Model.IPlugin.Information

		''' <summary>初始化</summary>
		Public Sub Init(Gateway As Bumblebee.Gateway, assembly As Assembly) Implements IPlugin.Init
			Log = New Counter.Log(Gateway)
			Status = New Counter.Status()
			AddHandler Gateway.RequestIncrement, AddressOf OnRequestIncrement

			' 加载状态统计 API
			Gateway.HttpServer.ActionFactory.Register(New API.Status(Gateway, Status))
			Gateway.HttpServer.ActionFactory.Register(New API.Log(Gateway))
		End Sub

		''' <summary>加载配置</summary>
		Public Sub LoadSetting(setting As JToken) Implements IPlugin.LoadSetting
		End Sub

		''' <summary>保存配置</summary>
		Public Function SaveSetting() As Object Implements IPlugin.SaveSetting
			Return Nothing
		End Function

		Private Sub OnRequestIncrement(sender As Object, e As EventRequestIncrementArgs)
			If Enabled Then
				Status.Counter(e)
				Log.Add(e)
			End If
		End Sub

	End Class

End Namespace