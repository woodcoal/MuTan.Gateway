'--------------------------------------------------   
'   
'   服务器配置   
'   
'   namespace: API.Server
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 服务器配置
'   release: 2020-07-22
'   
'-------------------------------------------------- 

Imports BeetleX.FastHttpApi

Namespace API

	''' <summary>路径重写</summary>
	<Controller(BaseUrl:="_gateway/server", SkipPublicFilter:=True)>
	<Options(AllowOrigin:="*", AllowHeaders:="*", AllowMethods:="OPTIONS,GET,POST", AllowMaxAge:="86400")>
	Public Class Server

		''' <summary>网关对象</summary>
		Private ReadOnly Gateway As Bumblebee.Gateway

		Public Sub New(Gateway As Bumblebee.Gateway)
			Me.Gateway = Gateway
		End Sub

		Public Function List() As Model.Server()
			Return Gateway.Agents.Servers.OrderBy(Function(x) x.Category).Select(Function(x) New Model.Server(x)).ToArray
		End Function

		<Post>
		Public Function Update(Item As Model.Server) As Boolean
			If Item IsNot Nothing AndAlso Not String.IsNullOrEmpty(Item.Host) Then
				Try
					If String.IsNullOrEmpty(Item.Category) Then
						Try
							Item.Category = New Uri(Item.Host).Host
						Catch ex As Exception
						End Try
					End If

					Gateway.SetServer(Item.Host, Item.Category, Item.Remark, Item.MaxConnections)
					Gateway.SaveConfig()

					Return True
				Catch ex As Exception
				End Try
			End If
			Return False
		End Function

		Public Function Remove(ByVal Host As String) As Boolean
			If Not String.IsNullOrEmpty(Host) Then
				Gateway.RemoveServer(Host)
				Gateway.SaveConfig()

				Return True
			Else
				Return False
			End If
		End Function

		Public Function Categories() As String()
			Return Gateway.Agents.Servers.GroupBy(Function(x) x.Category).Select(Function(x) x.Key).ToArray
		End Function

		Public Function Status() As Object
			Return Gateway.Agents.Servers.Select(Function(x) New With {
				.Host = x.Uri,
				x.Category,
				x.MaxConnections,
				x.Remark,
				x.WaitQueue,
				x.Available,
				x.Statistics.All.Count,
				x.Statistics.All.GetData().Rps,
				._2xCount = x.Statistics.Status_2xx.Count,
				._2xRps = x.Statistics.Status_2xx.GetData().Rps,
				._5xCount = x.Statistics.Status_5xx.Count,
				._5xRps = x.Statistics.Status_5xx.GetData().Rps,
				._4xCount = x.Statistics.Status_4xx.Count,
				._4xRps = x.Statistics.Status_4xx.GetData().Rps
			})
		End Function

	End Class

End Namespace