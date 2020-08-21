'--------------------------------------------------   
'   
'   访问明细记录
'   
'   namespace: Counter.Log
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 访问明细记录
'   release: 2020-07-23
'   
'-------------------------------------------------- 

Imports BeetleX.EventArgs
Imports Bumblebee.Events
Imports LiteDB

Namespace Counter
	''' <summary>访问明细记录</summary>
	Public Class Log
		Implements IDisposable

		''' <summary>记录数据</summary>
		Private ReadOnly Data As Concurrent.ConcurrentQueue(Of Model.Log)

		''' <summary>网关对象</summary>
		Public ReadOnly Gateway As Bumblebee.Gateway

		''' <summary>定时器</summary>
		Private ReadOnly Timer As Threading.Timer

		''' <summary>是否操作中</summary>
		Private IsBusy As Boolean

		Public Sub New(Gateway As Bumblebee.Gateway)
			Me.Gateway = Gateway
			Me.IsBusy = False
			Me.Data = New Concurrent.ConcurrentQueue(Of Model.Log)
			Timer = New Threading.Timer(AddressOf OnTrack, Nothing, 1000, 1000)
		End Sub

		Public Sub Add(ByVal e As EventRequestIncrementArgs)
			Data.Enqueue(New Model.Log With {
						  .Url = e.Request.GetSourceBaseUrl,
						  .Code = e.Code,
						  .Method = e.Request.Method,
						  .Data = e.Request.Data.ToString,
						  .Header = e.Request.Header.ToString,
						  .Cookies = e.Request.Cookies.ToString,
						  .Time = e.Time,
						  .Update = Date.Now,
						  .Client = xJWT.Current(e.Request).Name,
						  .IP = Utils.GetIP(e.Request)
					})

			'' 随机添加测试数据
			'Dim Rnd = New Random
			'Dim Max = Rnd.Next(100, 500)
			'For I = 1 To Max
			'	Cache.Enqueue(New Model.Log With {
			'				  .Url = e.Request.GetSourceBaseUrl & "?" & Rnd.Next(1, 20),
			'				  .Code = Rnd.Next(200, 600),
			'				  .Time = Rnd.Next(10, 10000),
			'				  .Update = Runtimes.Timer.Now.AddMinutes(Rnd.Next(-1440, 10)),
			'				  .Client = Rnd.Next(1000, 2000)
			'			})
			'Next
		End Sub

		''' <summary>定时写入数据库 </summary>
		Private Sub OnTrack(ByVal state As Object)
			If IsBusy Then Exit Sub
			If Data.IsEmpty Then Exit Sub

			IsBusy = True
			Timer.Change(-1, -1)

			Try
				' 按月保存
				Dim Logs As New Dictionary(Of String, List(Of Model.Log))
				While Not Data.IsEmpty
					Dim Log As Model.Log = Nothing
					Data.TryDequeue(Log)
					If Log IsNot Nothing Then
						Dim Day = Log.Update.ToString("yyyy-MM")
						If Not Logs.ContainsKey(Day) Then Logs.Add(Day, New List(Of Model.Log))

						Logs(Day).Add(Log)
					End If
				End While

				If Logs.Count > 0 Then
					For Each Day As String In Logs.Keys
						Dim Conn = "Connection=shared;Filename=" & Root("recoreds/" & Day & ".db", True)
						Using db As New LiteDatabase(Conn)
							Dim table = db.GetCollection(Of Model.Log)
							table.Insert(Logs(Day))
						End Using
					Next
				End If
			Catch ex As Exception
				Dim s = Me.Gateway.HttpServer
				If s.EnableLog(LogType.Error) Then s.Log(LogType.Error, Nothing, "【存储记录】保存访问记录异常：" & ex.Message & " / " & ex.StackTrace)
			End Try

			Timer.Change(999, 999)
			IsBusy = False
		End Sub

		Public Sub Dispose() Implements IDisposable.Dispose
			' 等待保存完成
			While IsBusy
				Threading.Thread.Sleep(10)
			End While

			Call OnTrack(Nothing)
			Timer.Dispose()
			GC.SuppressFinalize(Me)
		End Sub

	End Class
End Namespace
