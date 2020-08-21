'--------------------------------------------------   
'   
'   系统信息记录
'   
'   namespace: Counter.System
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 系统信息记录
'   release: 2020-08-19
'   
'-------------------------------------------------- 

Imports System.Collections.Concurrent

Namespace Counter
	''' <summary>系统信息记录</summary>
	Public Class System
		Implements IDisposable

		''' <summary>记录结构</summary>
		Public Class Item
			Public Time As Long
			Public CPU As Single
			Public Memory As Single
			Public Request As Single
			Public Connections As Single
		End Class

		''' <summary>记录数据</summary>
		Private ReadOnly Data As List(Of Item)

		''' <summary>当前最新状态</summary>
		Public ReadOnly Now As Item

		''' <summary>最后数据</summary>
		Private ReadOnly Last As Item

		''' <summary>网关对象</summary>
		Public ReadOnly Gateway As Bumblebee.Gateway

		''' <summary>定时器</summary>
		Private ReadOnly Timer As Threading.Timer

		''' <summary>是否操作中</summary>
		Private IsBusy As Boolean

		''' <summary>每记录30条数据，存一次文件</summary>
		Private SaveDelay As Integer = 30

		''' <summary>允许最多记录数量</summary>
		Private Const MaxCount As Integer = 10000

		''' <summary>获取统计数据</summary>
		Public ReadOnly Property List(Count As Integer) As List(Of Item)
			Get
				If Count > 0 Then
					Return Data.TakeLast(Count).ToList
				Else
					Return Data.ToList
				End If
			End Get
		End Property

		Public Sub New(Gateway As Bumblebee.Gateway)
			Me.Gateway = Gateway

			' 加载参数
			Dim Path = Root("recoreds/system.json", True)
			Me.Data = If(Utils.ReadJson(Of List(Of Item))(Path), New List(Of Item))
			Me.Now = New Item
			Me.Last = New Item

			' 定时器
			Timer = New Threading.Timer(AddressOf OnTrack, Nothing, 1000, 1000)
		End Sub

		''' <summary>定时写入数据库 </summary>
		Private Sub OnTrack(ByVal state As Object)
			If xConfig.COUNTER_SYSTEM_DELAY < 1 Then Exit Sub
			If IsBusy Then Exit Sub

			IsBusy = True
			Timer.Change(-1, -1)

			Try
				Dim CurrentProcess = Process.GetCurrentProcess

				' 存在数据才开始计算，第一次直接跳过
				If Last.Time > 0 Then
					' 前后两次请求时长，单位：秒
					Dim TimeLong = (Date.Now.Ticks - Last.Time) / 10000000

					' 记录当前数据
					Dim Item As New Item

					' 用 JS 时间戳
					Now.Time = Utils.JsTicks(Date.Now)
					Item.Time = Now.Time

					'-----------------------------------------------

					Now.Request = Gateway.HttpServer.TotalRequest
					Dim Per = Now.Request - Last.Request
					If Per > 0 Then
						Item.Request = Per / TimeLong
					Else
						Item.Request = 0
					End If

					'-----------------------------------------------

					Now.Connections = Gateway.HttpServer.TotalConnections
					Per = Now.Connections - Last.Connections
					If Per > 0 Then
						Item.Connections = Per / TimeLong
					Else
						Item.Connections = 0
					End If

					'-----------------------------------------------

					Now.Memory = Environment.WorkingSet / 1024 / 1024
					Item.Memory = Now.Memory

					'-----------------------------------------------

					Per = CurrentProcess.TotalProcessorTime.Ticks - Last.CPU
					Now.CPU = Per / 10000000 / TimeLong / Environment.ProcessorCount * 100
					Item.CPU = Now.CPU

					SyncLock Data
						Data.Add(Item)

						' 移除多余数据，每次移除10%
						If Data.Count > MaxCount Then
							Data.RemoveRange(0, MaxCount * 10%)
						End If
					End SyncLock

					If SaveDelay > 0 Then
						SaveDelay -= 1
					Else
						SaveDelay = 30
						Call Save()
					End If
				End If

				Last.Time = Date.Now.Ticks
				Last.CPU = CurrentProcess.TotalProcessorTime.Ticks
				Last.Request = Gateway.HttpServer.TotalRequest
				Last.Connections = Gateway.HttpServer.TotalConnections
			Catch ex As Exception
			End Try

			Dim Delay = xConfig.COUNTER_SYSTEM_DELAY * 1000
			If Delay < 1 Then Delay = 1000

			Timer.Change(Delay, Delay)
			IsBusy = False
		End Sub

		Public Sub Save()
			Dim Path = Root("recoreds/system.json", True)
			Utils.SaveJson(Path, Data)
		End Sub

		Public Sub Dispose() Implements IDisposable.Dispose
			' 等待保存完成
			While IsBusy
				Threading.Thread.Sleep(10)
			End While

			Call OnTrack(Nothing)
			Timer.Dispose()

			Call Save()

			GC.SuppressFinalize(Me)
		End Sub

	End Class
End Namespace
