'--------------------------------------------------   
'   
'   客户端
'   
'   namespace: Model.Client
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 客户端
'   release: 2020-07-27
'   
'-------------------------------------------------- 


Imports LiteDB

Namespace Model

	''' <summary>客户端</summary>
	Public Class Client

		''' <summary>唯一标识</summary>
		Public Property ID As ObjectId

		''' <summary>账号</summary>
		Public Property Name As String

		''' <summary>通讯密匙</summary>
		Public Property Key As String

		''' <summary>版本要求</summary>
		Public Property Version As Single

		''' <summary>权限</summary>
		Public Property Role As String

		''' <summary>注册时间</summary>
		Public Property Created As Date

		''' <summary>更新时间</summary>
		Public Property Update As Date

		''' <summary>是否启用</summary>
		Public Property Open As Boolean

		''' <summary>启动时间</summary>
		Public Property TimeStart As Date

		''' <summary>启动时间</summary>
		Public Property TimeStop As Date

		''' <summary>关闭说明</summary>
		Public Property CloseMessage As String

		''' <summary>参数配置</summary>
		Public Property Config As Dictionary(Of String, String)

		''' <summary>系统是否允许打开</summary>
		<BsonIgnore>
		Public ReadOnly Property IsEnabled As Boolean
			Get
				Dim R = False

				If Open Then
					R = True
				Else
					If TimeStart > TimeStop Then
						' 开启时间晚于关闭时间，则中间时段禁用
						If Date.Now > TimeStart OrElse Date.Now < TimeStop Then
							R = True
						End If
					Else
						' 中间时间才可以用
						If Date.Now >= TimeStart AndAlso Date.Now <= TimeStop Then
							R = True
						End If
					End If
				End If

				Return R
			End Get
		End Property

	End Class

End Namespace
