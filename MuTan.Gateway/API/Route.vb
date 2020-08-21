'--------------------------------------------------   
'   
'   路由配置   
'   
'   namespace: API.Route
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 路由配置
'   release: 2020-07-22
'   
'-------------------------------------------------- 

Imports BeetleX.FastHttpApi

Namespace API

	''' <summary>路径重写</summary>
	<Controller(BaseUrl:="_gateway/route", SkipPublicFilter:=True)>
	<Options(AllowOrigin:="*", AllowHeaders:="*", AllowMethods:="OPTIONS,GET,POST", AllowMaxAge:="86400")>
	Public Class Route

		''' <summary>网关对象</summary>
		Private ReadOnly Gateway As Bumblebee.Gateway

		Public Sub New(Gateway As Bumblebee.Gateway)
			Me.Gateway = Gateway
		End Sub

		Public Function List() As List(Of Model.Route)
			Dim Ret As New List(Of Model.Route)

			Dim Rs = Gateway.Routes.Urls.Where(Function(x) x.ApiLoader).ToList
			Rs = If(Rs, New List(Of Bumblebee.Routes.UrlRoute))
			Rs.Add(Gateway.Routes.Default)

			For Each R In Rs.OrderBy(Function(x) x.Url)
				'Dim item = New Model.Route(R)
				'item.Servers = ServerList(item.Url)
				Ret.Add(New Model.Route(R))
			Next

			Return Ret
		End Function

		<Post>
		Public Function Insert(ByVal Url As String, ByVal Remark As String, ByVal hashPattern As String) As Boolean
			If Not String.IsNullOrWhiteSpace(Url) Then
				Try
					Gateway.SetRoute(Url, Remark, hashPattern)
					Gateway.SaveConfig()

					Return True
				Catch ex As Exception
				End Try
			End If

			Return False
		End Function

		<Post>
		Public Function Update(ByVal item As Model.Route) As Boolean
			If item IsNot Nothing AndAlso Not String.IsNullOrEmpty(item.Url) Then
				Try
					Dim Route = Gateway.Routes.GetRoute(item.Url)
					If Route IsNot Nothing Then
						' 移除原来的服务器列表
						If Route.Servers?.Length > 0 Then
							For Each s In Route.Servers
								Route.RemoveServer(s.Agent.Uri.ToString)
							Next
						End If

						' 添加新服务器
						If item.Servers?.Length > 0 Then
							For Each s In item.Servers
								Route.AddServer(s.Host, s.Weight, s.MaxRps, s.Standby)
							Next
						End If

						' 更新为新参数
						With Route
							.HashPattern = item.HashPattern
							.Remark = item.Remark
							.MaxRps = item.MaxRps
							.TimeOut = item.TimeOut

							.AccessControlAllowHeaders = item.AccessControlAllowHeaders
							.AccessControlAllowMethods = item.AccessControlAllowMethods
							.AccessControlAllowOrigin = item.AccessControlAllowOrigin
							.AccessControlMaxAge = item.AccessControlMaxAge
							.AccessControlAllowCredentials = item.AccessControlAllowCredentials
							.Vary = item.Vary
						End With

						Gateway.SaveConfig()

						Return True
					End If
				Catch ex As Exception
				End Try
			End If

			Return False
		End Function

		Public Function Remove(ByVal Url As String) As Boolean
			If Not String.IsNullOrWhiteSpace(Url) Then
				Gateway.RemoveRoute(Url)
				Gateway.SaveConfig()

				Return True
			Else
				Return False
			End If
		End Function

	End Class

End Namespace