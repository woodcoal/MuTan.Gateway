'--------------------------------------------------   
'   
'   非法请求
'   
'   namespace: Plugin.InvalidRequest
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 非法请求
'   release: 2020-08-21
'   
'-------------------------------------------------- 

Imports System.Reflection
Imports BeetleX.FastHttpApi.Data
Imports Bumblebee
Imports Bumblebee.Events
Imports Bumblebee.Plugins
Imports Newtonsoft.Json.Linq

Namespace Plugin

	''' <summary>非法请求</summary>
	<RouteBinder(ApiLoader:=False)>
	Public Class InvalidRequest
		Implements IRequestingHandler, Model.IPlugin, IPluginStatus

		''' <summary>网关对象</summary>
		Private Gateway As Bumblebee.Gateway

		''' <summary>无效地址请求</summary>
		Private InvalidUrls As New InvalidDataFilter

		''' <summary>无效参数请求</summary>
		Private InvalidData As New InvalidDataFilter

		''' <summary>强制启用</summary>
		Public Property Enabled As Boolean = True Implements IPluginStatus.Enabled

		''' <summary>名称</summary>
		Public ReadOnly Property Name As String = Me.GetType.Name Implements IPlugin.Name

		''' <summary>描述</summary>
		Public ReadOnly Property Description As String = "拦击非法参数，决绝非法请求" Implements IPlugin.Description

		''' <summary>插件级别</summary>
		Public ReadOnly Property Level As PluginLevel = PluginLevel.High9 Implements IPlugin.Level

		''' <summary>是否需要设置参数</summary>
		Public ReadOnly Property EnSetting As Boolean = True Implements Model.IPlugin.EnSetting

		''' <summary>扩展展示信息</summary>
		Public ReadOnly Property Information As Dictionary(Of String, String) = Nothing Implements Model.IPlugin.Information

		''' <summary>初始化</summary>
		Public Sub Init(Gateway As Bumblebee.Gateway, assembly As Assembly) Implements IPlugin.Init
			Me.Gateway = Gateway
		End Sub

#Region "参数"

		Private Urls As String() = Array.Empty(Of String)
		Private Datas As String() = Array.Empty(Of String)

		''' <summary>禁止访问提示消息</summary>
		Private _StopMessage As String

		''' <summary>禁止访问提示消息</summary>
		Public Property StopMessage As String
			Get
				If String.IsNullOrWhiteSpace(_StopMessage) Then
					Return "非法请求"
				Else
					Return _StopMessage
				End If
			End Get
			Set(value As String)
				_StopMessage = value
			End Set
		End Property

#End Region

#Region "操作"

		''' <summary>执行操作</summary>
		Public Sub Execute(e As EventRequestingArgs) Implements IRequestingHandler.Execute
			If Not Enabled Then Exit Sub

			Dim Pass = True

			' 存在错误地址
			If Urls.Count > 0 Then
				Dim Url = e.Request.GetSourceBaseUrl
				If InvalidUrls.IsMatch(Url) > 0 Then
					Pass = False
				End If
			End If

			' 存在异常参数
			If Pass AndAlso Datas.Count > 0 Then
				Dim Data = e.Request.Data.ToString
				If InvalidUrls.IsMatch(Data) > 0 Then
					Pass = False
				End If
			End If

			' 未通过，禁止访问
			If Not Pass Then
				e.Cancel = True
				e.ResultType = ResultType.Completed

				' 加入跨域属性
				e.Response.Header.Add("Access-Control-Allow-Origin", "*")
				e.Response.Header.Add("Access-Control-Allow-Headers", "*")
				e.Response.Header.Add("Access-Control-Allow-Methods", "*")
				e.Response.Header.Add("Access-Control-Max-Age", "86400")

				e.Response.Result(New BadGateway(StopMessage, 502))
			End If
		End Sub

		''' <summary>加载配置</summary>
		Public Sub LoadSetting(Setting As JToken) Implements IPlugin.LoadSetting
			Urls = If(Setting("Urls")?.ToObject(Of String()), Array.Empty(Of String))
			Datas = If(Setting("Datas")?.ToObject(Of String()), Array.Empty(Of String))

			InvalidUrls = New InvalidDataFilter
			If Urls.Length > 0 Then InvalidUrls.Add(Urls)

			InvalidData = New InvalidDataFilter
			If Datas.Length > 0 Then InvalidUrls.Add(Datas)

			StopMessage = Setting("StopMessage")
		End Sub

		''' <summary>保存配置</summary>
		Public Function SaveSetting() As Object Implements IPlugin.SaveSetting
			Return New With {Urls, Datas, StopMessage}
		End Function

#End Region

	End Class

End Namespace