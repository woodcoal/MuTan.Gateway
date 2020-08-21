'--------------------------------------------------   
'   
'   访问日志
'   
'   namespace: Model.Log
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 访问日志
'   release: 2020-07-23
'   
'-------------------------------------------------- 


Imports LiteDB

Namespace Model
	''' <summary>访问日志</summary>
	Public Class Log

		''' <summary>唯一标识</summary>
		Public Property ID As ObjectId

		''' <summary>请求地址</summary>
		Public Property Url As String

		''' <summary>请求类型</summary>
		Public Property Method As String

		''' <summary>IP</summary>
		Public Property IP As String

		''' <summary>客户端</summary>
		Public Property Client As String

		''' <summary>头部信息</summary>
		Public Property Header As String

		''' <summary>请求数据</summary>
		Public Property Data As String

		''' <summary>Cookies</summary>
		Public Property Cookies As String

		''' <summary>更新时间</summary>
		Public Property Update As Date

		''' <summary>耗时</summary>
		Public Property Time As Long

		''' <summary>结果状态</summary>
		Public Property Code As Integer

	End Class
End Namespace
