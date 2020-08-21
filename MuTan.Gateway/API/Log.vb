'--------------------------------------------------   
'   
'   日志
'   
'   namespace: API.Log
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 日志
'   release: 2020-07-27
'   
'-------------------------------------------------- 

Imports BeetleX.FastHttpApi
Imports LiteDB

Namespace API

	''' <summary>日志</summary>
	<Controller(BaseUrl:="_gateway/log", SkipPublicFilter:=True)>
	<Options(AllowOrigin:="*", AllowHeaders:="*", AllowMethods:="OPTIONS,GET,POST", AllowMaxAge:="86400")>
	Public Class Log

		''' <summary>网关对象</summary>
		Private ReadOnly Gateway As Bumblebee.Gateway

		Public Sub New(Gateway As Bumblebee.Gateway)
			Me.Gateway = Gateway
		End Sub

		''' <summary>系统日志</summary>
		Public Function Sys(pageIndex As Integer, pageCount As Integer) As Object
			If pageIndex < 1 Then pageIndex = 1
			If pageCount < 1 Then pageCount = 100
			If pageCount > 100 Then pageCount = 100

			Dim Logs = Gateway.HttpServer.GetCacheLog
			Dim MaxCount = Logs.Length

			If MaxCount > 0 Then
				Dim PageMax = Math.Ceiling(MaxCount / pageCount)
				If pageIndex > PageMax Then pageIndex = PageMax

				Dim Result = Logs.Reverse.Skip((pageIndex - 1) * pageCount).Take(pageCount).ToList
				Return New With {.Count = MaxCount, Result}
			End If

			Return Nothing
		End Function

		Public Function RecFiles() As List(Of String)
			Dim Ret As New List(Of String)

			Dim Dir = Root("recoreds")
			If IO.Directory.Exists(Dir) Then
				Return IO.Directory.GetFiles(Dir, "*.db")?.OrderByDescending(Function(x) x).Select(Function(x) IO.Path.GetFileNameWithoutExtension(x)).ToList
			End If

			Return Nothing
		End Function

		''' <summary>系统日志</summary>
		Public Function Rec(file As String, Optional keyword As String = "", Optional pageIndex As Integer = 1, Optional pageCount As Integer = 10) As Object
			If pageIndex < 1 Then pageIndex = 1
			If pageCount < 1 Then pageCount = 100
			If pageCount > 100 Then pageCount = 100

			file = Root("recoreds/" & file & ".db")
			If IO.File.Exists(file) Then
				Dim Conn = "Connection=shared;Filename=" & file
				Using db As New LiteDatabase(Conn)
					Dim table = db.GetCollection(Of Model.Log)
					Dim MaxCount = table.Count
					If MaxCount > 0 Then
						If String.IsNullOrWhiteSpace(keyword) Then
							Dim PageMax = Math.Ceiling(MaxCount / pageCount)
							If pageIndex > PageMax Then pageIndex = PageMax

							'Dim Result = table.Find(New Query, (pageIndex - 1) * pageCount, pageCount).OrderByDescending(Function(x) x.ID).ToList
							Dim Result = table.FindAll.OrderByDescending(Function(x) x.ID).Skip((pageIndex - 1) * pageCount).Take(pageCount).ToList
							If Result?.Count > 0 Then Return New With {.Count = MaxCount, Result}
						Else
							' 最多查询 1000 条
							Dim Result = table.Find(Function(x) x.Url.Contains(keyword, StringComparison.OrdinalIgnoreCase), 0, 1000).OrderByDescending(Function(x) x.ID)

							MaxCount = Result.Count
							If MaxCount > 0 Then
								Dim PageMax = Math.Ceiling(MaxCount / pageCount)
								If pageIndex > PageMax Then pageIndex = PageMax

								Dim Ret = Result.Skip((pageIndex - 1) * pageCount).Take(pageCount).ToList

								If Result?.Count > 0 Then Return New With {.Count = MaxCount, .Result = Ret}
							End If
						End If
					End If
				End Using
			End If

			Return Nothing
		End Function

	End Class

End Namespace