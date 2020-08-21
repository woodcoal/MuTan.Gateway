'--------------------------------------------------   
'   
'   缓存规则
'   
'   namespace: Model.Cache.Rule
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 缓存规则
'   release: 2020-08-16
'   
'-------------------------------------------------- 

Imports LiteDB

Namespace Model.Cache

	''' <summary>缓存规则</summary>
	Public Class Rule

		''' <summary>唯一标识</summary>
		Public Property ID As ObjectId

		''' <summary>规则</summary>
		Public Property API As String

		''' <summary>缓存时长，单位：秒</summary>
		Public Property Time As Integer

		''' <summary>创建时间</summary>
		Public Property Created As Date

		''' <summary>更新时间</summary>
		Public Property Update As Date

	End Class

End Namespace
