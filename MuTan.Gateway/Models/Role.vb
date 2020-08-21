'--------------------------------------------------   
'   
'   权限
'   
'   namespace: Model.Role
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 权限
'   release: 2020-07-27
'   
'-------------------------------------------------- 


Imports LiteDB

Namespace Model

	''' <summary>权限</summary>
	Public Class Role

		''' <summary>唯一标识</summary>
		Public Property ID As ObjectId

		''' <summary>名称</summary>
		Public Property Name As String

		''' <summary>授权API</summary>
		Public Property APIs As String()

		''' <summary>创建时间</summary>
		Public Property Created As Date

		''' <summary>更新时间</summary>
		Public Property Update As Date

	End Class

End Namespace
