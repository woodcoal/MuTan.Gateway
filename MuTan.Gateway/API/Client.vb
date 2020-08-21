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
	<Controller(BaseUrl:="_gateway/client", SkipPublicFilter:=True)>
	<Options(AllowOrigin:="*", AllowHeaders:="*", AllowMethods:="OPTIONS,GET,POST", AllowMaxAge:="86400")>
	Public Class Client

		''' <summary>添加客户端</summary>
		<Post>
		Public Function Insert(name As String, role As String) As Boolean
			Return xDatabase.ClientInsert(name, role)
		End Function

		''' <summary>更新客户端</summary>
		<Post>
		Public Function Update(client As Model.Client) As Boolean
			Return xDatabase.ClientUpdate(client)
		End Function

		''' <summary>重置Key</summary>
		Public Function UpdateKey(name As String) As Boolean
			Dim R = xDatabase.ClientChangeKey(name)
			Return String.IsNullOrEmpty(R)
		End Function

		''' <summary>客户端信息</summary>
		Public Function Info(name As String) As Model.Client
			Return xDatabase.ClientItem(name)
		End Function

		''' <summary>移除用户</summary>
		Public Function Remove(name As String) As Boolean
			Return xDatabase.ClientRemove(name)
		End Function

		''' <summary>用户列表</summary>
		Public Function List(keyword As String, pageIndex As Integer, pageCount As Integer) As Object
			Dim R = xDatabase.ClientPages(keyword, pageIndex, pageCount)
			Return New With {R.Result, R.Count}
		End Function

	End Class

End Namespace