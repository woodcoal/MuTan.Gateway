'--------------------------------------------------   
'   
'   权限操作   
'   
'   namespace: API.Role
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 权限操作
'   release: 2020-07-22
'   
'-------------------------------------------------- 

Imports BeetleX.FastHttpApi

Namespace API

	''' <summary>路径重写</summary>
	<Controller(BaseUrl:="_gateway/role", SkipPublicFilter:=True)>
	<Options(AllowOrigin:="*", AllowHeaders:="*", AllowMethods:="OPTIONS,GET,POST", AllowMaxAge:="86400")>
	Public Class Role

		''' <summary>更新权限</summary>
		<Post>
		Public Function Update(name As String, APIs() As String) As Boolean
			Return xDatabase.RoleUpdate(name, APIs)
		End Function

		''' <summary>移除权限</summary>
		Public Function Remove(name As String) As Boolean
			Return xDatabase.RoleRemove(name)
		End Function

		''' <summary>权限列表</summary>
		Public Function List() As Dictionary(Of String, String())
			Return xDatabase.RoleList
		End Function

		''' <summary>权限信息</summary>
		Public Function Info(name As String) As Model.Role
			Return xDatabase.RoleItem(name)
		End Function

	End Class

End Namespace