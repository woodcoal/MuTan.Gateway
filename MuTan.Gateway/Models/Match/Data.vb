'--------------------------------------------------   
'   
'   匹配数据
'   
'   namespace: Model.Match.Data
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 匹配数据
'   release: 2020-08-18
'   
'-------------------------------------------------- 

Imports System.Text.RegularExpressions
Imports BeetleX.FastHttpApi

Namespace Model.Match

	''' <summary>匹配数据</summary>
	Public Class Data(Of T)
		Inherits Rule

		''' <summary>数据值</summary>
		Public ReadOnly Value As T

		Public Sub New(Rule As String, Value As T)
			MyBase.New(Rule)
			Me.Value = Value
		End Sub

		''' <summary>从请求获取值</summary>
		Public Function GetResult(request As HttpRequest) As T
			Return If(MyBase.Match(request), Value, Nothing)
		End Function

		''' <summary>从网址获取值</summary>
		Public Function GetResult(mUrl As String) As T
			Return If(MyBase.Match(mUrl), Value, Nothing)
		End Function

		''' <summary>从参数获取值</summary>
		Public Function GetResult(Optional mHost As String = "", Optional mPath As String = "", Optional mData As String = "", Optional mHeader As String = "") As T
			Return If(MyBase.Match(mHost, mPath, mData, mHeader), Value, Nothing)
		End Function
	End Class

End Namespace