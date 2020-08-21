'--------------------------------------------------   
'   
'   缓存项目
'   
'   namespace: Model.Cache.Data
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 缓存项目
'   release: 2020-08-16
'   
'-------------------------------------------------- 

Imports System.Text
Imports BeetleX
Imports BeetleX.FastHttpApi

Namespace Model.Cache

	''' <summary>缓存项目</summary>
	Public Class Data

		Public Property ResponseStatus As String

		Public Property Header As Header

		Public Property Body As Byte()

		Public Property Count As Integer

		Public Property Finish As Boolean

		Public Sub New(ByVal responseStatus As String, ByVal header As Header)
			Me.ResponseStatus = responseStatus
			Me.Header = header
			Me.Header.Add("Cache-hit", "Mutan.Gateway/Caching")
		End Sub

		Public Sub Response(ByVal Request As HttpRequest)
			Try
				Dim Stream = StreamHelper.ToPipeStream(Request.Session.Stream)
				Stream.Write(Encoding.ASCII.GetBytes(Me.ResponseStatus))
				Stream.Write(HeaderTypeFactory.LINE_BYTES)
				Me.Header.Write(Stream)
				Stream.Write(HeaderTypeFactory.LINE_BYTES)
				Stream.Write(Me.Body, 0, Me.Count)
				Request.Session.Stream.Flush()
			Finally
				Request.Recovery()
			End Try
		End Sub

		Public Sub Write(ByVal Data As ArraySegment(Of Byte), ByVal Completed As Boolean)
			If Me.Count = 0 Then
				Me.Body = New Byte(Data.Count - 1) {}
				Data.CopyTo(Me.Body)
			Else
				Dim Array = New Byte(Me.Count + Data.Count - 1) {}
				Me.Body.CopyTo(Array, 0)
				Data.CopyTo(Array, Me.Count)
				Me.Body = Array
			End If

			Me.Count += Data.Count
			Me.Finish = Completed
		End Sub
	End Class

End Namespace
