'--------------------------------------------------   
'   
'   路径重写   
'   
'   namespace: API.Rewrite
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 路径重写
'   release: 2020-07-22
'   
'-------------------------------------------------- 

Imports BeetleX.FastHttpApi

Namespace API

	''' <summary>路径重写</summary>
	<Controller(BaseUrl:="_gateway/rewrite", SkipPublicFilter:=True)>
	<Options(AllowOrigin:="*", AllowHeaders:="*", AllowMethods:="OPTIONS,GET,POST", AllowMaxAge:="86400")>
	Public Class Rewrite

		''' <summary>重写对象</summary>
		Private ReadOnly RouteRewrite As RouteRewrite

		Public Sub New(Gateway As Bumblebee.Gateway)
			Me.RouteRewrite = Gateway.HttpServer.UrlRewrite
		End Sub

		Public Function List() As RouteRewrite.Config()
			Return RouteRewrite.GetRoutes.Where(Function(x) Not x.Url.StartsWith("/_gateway/", StringComparison.OrdinalIgnoreCase)).OrderByDescending(Function(x) x.Host).ToArray
		End Function

		Public Function Insert(Host As String, Url As String, Rewrite As String) As Boolean
			If Not String.IsNullOrEmpty(Url) AndAlso Not String.IsNullOrEmpty(Rewrite) AndAlso Not Url.StartsWith("/_gateway/", StringComparison.OrdinalIgnoreCase) Then
				Try
					RouteRewrite.Add(Host, Url, Rewrite, Nothing)
					RouteRewrite.Save()

					Return True
				Catch ex As Exception
				End Try
			End If

			Return False
		End Function

		Public Function Remove(ByVal Host As String, ByVal Url As String) As Boolean
			If Not String.IsNullOrEmpty(Url) AndAlso Not Url.StartsWith("/_gateway/", StringComparison.OrdinalIgnoreCase) Then
				RouteRewrite.Remove(Host, Url)
				RouteRewrite.Save()

				Return True
			Else
				Return False
			End If
		End Function
	End Class

End Namespace