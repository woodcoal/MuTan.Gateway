'--------------------------------------------------   
'   
'   授权认证插件   
'   
'   namespace: Plugin.Authorization
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 授权认证插件
'   release: 2020-07-20
'   
'-------------------------------------------------- 

Imports System.Reflection
Imports BeetleX
Imports BeetleX.FastHttpApi
Imports Bumblebee.Events
Imports Bumblebee.Plugins
Imports Newtonsoft.Json.Linq

Namespace Plugin

	''' <summary>授权认证插件</summary>
	<RouteBinder(ApiLoader:=False)>
	Public Class Authorization
		Implements IRequestingHandler, Model.IPlugin

		''' <summary>网关对象</summary>
		Private Gateway As Bumblebee.Gateway

		''' <summary>缓存数据</summary>
		Private ReadOnly Cache As Utils.LRU(Of String, List(Of String))

		''' <summary>构造</summary>
		Public Sub New()
			Me.Cache = New Utils.LRU(Of String, List(Of String))(1000)
		End Sub

		''' <summary>强制启用</summary>
		Public Property Enabled As Boolean = True

		''' <summary>名称</summary>
		Public ReadOnly Property Name As String = Me.GetType.Name Implements IPlugin.Name

		''' <summary>描述</summary>
		Public ReadOnly Property Description As String = "访问认证插件" Implements IPlugin.Description

		''' <summary>插件级别</summary>
		Public ReadOnly Property Level As PluginLevel = PluginLevel.High5 Implements IPlugin.Level

		''' <summary>是否需要设置参数</summary>
		Public ReadOnly Property EnSetting As Boolean = True Implements Model.IPlugin.EnSetting

		''' <summary>扩展展示信息</summary>
		Public ReadOnly Property Information As Dictionary(Of String, String) = Nothing Implements Model.IPlugin.Information

		''' <summary>初始化</summary>
		Public Sub Init(Gateway As Bumblebee.Gateway, assembly As Assembly) Implements IPlugin.Init
			Me.Gateway = Gateway

			Gateway.HttpServer.ActionFactory.Register(New API.Authorization(Gateway))
			Gateway.HttpServer.ActionFactory.Register(New API.Client)
			Gateway.HttpServer.ActionFactory.Register(New API.Role)
		End Sub

#Region "参数"

		''' <summary>缓存数量，一个以下表示不缓存</summary>
		Private Property CacheCount As Integer
			Get
				Return Cache.Size
			End Get
			Set(value As Integer)
				If value < 1 Then value = 0
				Cache.Size = value
			End Set
		End Property

		''' <summary>禁止访问提示消息</summary>
		Private _StopMessage As String

		''' <summary>禁止访问提示消息</summary>
		Public Property StopMessage As String
			Get
				If String.IsNullOrWhiteSpace(_StopMessage) Then
					Return "无效授权，禁止访问"
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
			' 无规则直接跳出
			If xAuthorization.Empty Then Exit Sub

			' 忽略 Option 请求
			If e.Request.Method = "OPTIONS" Then Exit Sub

			Dim Url = e.Request.GetSourceBaseUrl
			If Not String.IsNullOrEmpty(Url) Then
				Dim Key = Model.Match.Rule.MakeKey(e.Request, xConfig.AUTH_HEADER, xConfig.AUTH_DATA, xConfig.AUTH_COOKIES, True)

				' 从缓存分析是否存在匹配的权限
				Dim Roles = Cache.Get(Key)

				If Roles Is Nothing Then
					Roles = xAuthorization.Match(e.Request)
					Cache.Put(Key, Roles)
				End If

				' 存在匹配项目，需要校验权限
				If Roles?.Count > 0 Then
					Dim Cancel = True

					Dim User = xJWT.Current(e.Request)
					If User.IsSuper OrElse Roles.Contains(User.Role, StringComparer.OrdinalIgnoreCase) Then Cancel = False

					If Cancel Then
						e.Cancel = True
						e.ResultType = ResultType.Completed

						' 加入跨域属性
						e.Response.Header.Add("Access-Control-Allow-Origin", "*")
						e.Response.Header.Add("Access-Control-Allow-Headers", "*")
						e.Response.Header.Add("Access-Control-Allow-Methods", "*")
						e.Response.Header.Add("Access-Control-Max-Age", "86400")

						e.Gateway.Response(e.Response, New UnauthorizedResult(StopMessage))
						e.Gateway.RequestIncrementCompleted(e.Request, 401, TimeWatch.GetTotalMilliseconds - e.Request.RequestTime, Nothing)
					End If
				End If

				' 记录日志
				Dim s = Gateway.HttpServer
				If s.EnableLog(EventArgs.LogType.Info) Then s.Log(EventArgs.LogType.Info, e.Request.Session, $"【JWT】 {e.Request.ID} {e.Request.RemoteIPAddress} {e.Request.Method} {e.Request.Url} 验证通过！")
			End If
		End Sub

		''' <summary>加载配置</summary>
		Public Sub LoadSetting(Setting As JToken) Implements IPlugin.LoadSetting
			If Setting IsNot Nothing Then
				StopMessage = Setting("StopMessage")
				CacheCount = If(Setting("CacheCount"), 0)
			End If
		End Sub

		''' <summary>保存配置</summary>
		Public Function SaveSetting() As Object Implements IPlugin.SaveSetting
			Return New With {StopMessage, CacheCount}
		End Function

#End Region

	End Class

End Namespace