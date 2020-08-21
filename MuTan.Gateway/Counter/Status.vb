'--------------------------------------------------   
'   
'   状态统计
'   
'   namespace: Counter.Status
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 状态统计
'   release: 2020-07-23
'   
'-------------------------------------------------- 

Imports System.Collections.Concurrent
Imports Bumblebee.Events
Imports LiteDB

Namespace Counter

	''' <summary>状态统计</summary>
	Public Class Status

		''' <summary>统计项</summary>
		Public Class Urls

			''' <summary>对象名称</summary>
			Public ReadOnly Name As String

			''' <summary>访问次数</summary>
			Private _Count As Long

			''' <summary>访问次数</summary>
			Public ReadOnly Property Count As Long
				Get
					Return _Count
				End Get
			End Property

			''' <summary>内置对象</summary>
			Public ReadOnly Instance As ConcurrentDictionary(Of String, Long)

			Public Sub New(Name As String)
				Me.Name = Name
				Instance = New ConcurrentDictionary(Of String, Long)(StringComparer.OrdinalIgnoreCase)
			End Sub

			''' <summary>统计</summary>
			Public Sub Counter(Url As String)
				Threading.Interlocked.Increment(_Count)
				Instance.AddOrUpdate(Url, 1, Function(k, v) v + 1)
			End Sub

			''' <summary>更新统计</summary>
			Public Sub Update(Url As String, Count As Long)
				_Count += Count
				Instance.AddOrUpdate(Url, Count, Function(k, v) Count)
			End Sub

			''' <summary>排序</summary>
			Public Function Top(Optional Count As Integer = 10) As Dictionary(Of String, Long)
				If Count < 1 Then Count = 10
				Return Instance.OrderByDescending(Function(x) x.Value).Take(Count).ToDictionary(Function(x) x.Key, Function(x) x.Value)
			End Function

		End Class

		''' <summary>状态统计</summary>
		Public Class Item

			''' <summary>内置对象</summary>
			Public ReadOnly Instance As New Dictionary(Of String, Urls)

			''' <summary>访问次数</summary>
			Private _Count As Integer

			''' <summary>访问次数</summary>
			Public ReadOnly Property Count As Integer
				Get
					Return _Count
				End Get
			End Property

			Public Sub New()
				Instance = New Dictionary(Of String, Urls) From {
					{"ALL", New Urls("ALL")},               ' 所有统计
					{"2x", New Urls("2x")},                 ' 200
					{"3x", New Urls("3x")},                 ' 300
					{"4x", New Urls("4x")},                 ' 400
					{"5x", New Urls("5x")},                 ' 500
					{"Cache", New Urls("Cache")},           ' 来自缓存
					{"10", New Urls("10")},                 ' 10ms
					{"10_50", New Urls("10_50")},           ' 10-50ms
					{"50_100", New Urls("50_100")},         ' 50-100ms
					{"100_200", New Urls("100_200")},       ' 100-200ms
					{"200_500", New Urls("200_500")},       ' 200-500ms
					{"500_1000", New Urls("500_1000")},     ' 500-1000ms
					{"1000_2000", New Urls("1000_2000")},   ' 1-2s
					{"2000_5000", New Urls("2000_5000")},   ' 2-5s
					{"5000", New Urls("5000")}              ' 5s 以上
				}
			End Sub

			''' <summary>统计</summary>
			Public Sub Counter(Url As String, Code As Integer, Time As Integer)
				Threading.Interlocked.Increment(_Count)

				Instance("ALL").Counter(Url)

				Select Case Code
					Case 311
						Instance("Cache").Counter(Url)
					Case 200 To 299
						Instance("2x").Counter(Url)
					Case 300 To 399
						Instance("3x").Counter(Url)
					Case 400 To 499
						Instance("4x").Counter(Url)
					Case >= 500
						Instance("5x").Counter(Url)
				End Select

				Select Case Time
					Case <= 10
						Instance("10").Counter(Url)
					Case 11 To 50
						Instance("10_50").Counter(Url)
					Case 51 To 100
						Instance("50_100").Counter(Url)
					Case 101 To 200
						Instance("100_200").Counter(Url)
					Case 201 To 500
						Instance("200_500").Counter(Url)
					Case 501 To 1000
						Instance("500_1000").Counter(Url)
					Case 1001 To 2000
						Instance("1000_2000").Counter(Url)
					Case 2001 To 5000
						Instance("2000_5000").Counter(Url)
					Case Else
						Instance("5000").Counter(Url)
				End Select
			End Sub

			''' <summary>统计</summary>
			Public Sub Counter(ByVal e As EventRequestIncrementArgs)
				Call Counter(e.Request.GetSourceBaseUrl, e.Code, e.Time)
			End Sub

		End Class

		'''' <summary>时间段间隔（分钟），建议 1、5、10、15、20、30、60 能被整除的数，访问量小的时候间隔时间可以拉长</summary>
		'Private Shared TimeDelay As Integer = 30

		''' <summary>统计</summary>
		Public ReadOnly Instance As ConcurrentDictionary(Of Long, Item)

		''' <summary>最后时间</summary>
		Private Update As Long

		Public Sub New()
			Instance = New ConcurrentDictionary(Of Long, Item)
			Call Load()
		End Sub

		''' <summary>当前时间段</summary>
		Public Shared Function Now() As Long
			Return GetTime(Date.Now)
		End Function

		''' <summary>获取时间段</summary>
		Public Shared Function GetTime(t As Date) As Long
			Dim d = Math.Floor(t.Minute / xConfig.COUNTER_STATUS_DELAY) * xConfig.COUNTER_STATUS_DELAY
			t = New Date(t.Year, t.Month, t.Day, t.Hour, d, 0)
			Return Utils.JsTicks(t)
		End Function

		''' <summary>统计</summary>
		Public Sub Counter(e As EventRequestIncrementArgs)
			Dim key = Now()

			If Update <> key Then
				' 仅仅保留 COUNTER_STATUS_BEFORE 小时统计
				Dim before = GetTime(Date.Now.AddHours(0 - xConfig.COUNTER_STATUS_BEFORE))

				Dim Keys = Instance.Where(Function(x) x.Key < before).OrderBy(Function(x) x.Key).Select(Function(x) x.Key).ToList
				If Keys?.Count > 0 Then
					For Each k In Keys
						Instance.TryRemove(k, Nothing)
					Next
				End If

				If Not Instance.ContainsKey(key) Then Instance.TryAdd(key, New Item)
				Update = key
			End If

			Instance(key).Counter(e)
		End Sub

		''' <summary>加载数据</summary>
		Private Sub Load()
			Instance.Clear()

			' 按月保存
			Dim file = Root("recoreds/" & Date.Now.ToString("yyyy-MM") & ".db")
			If IO.File.Exists(file) Then
				Dim Conn = "Connection=shared;Filename=" & file
				Using db As New LiteDatabase(Conn)
					Dim Table = db.GetCollection(Of Model.Log)
					Table.EnsureIndex(Function(x) x.Update)

					Dim Logs = Table.Find(Function(x) x.Update > Date.Now.AddDays(-1))
					If Logs.Count > 0 Then
						For Each Log In Logs
							Dim d = GetTime(Log.Update)

							If Not Instance.ContainsKey(d) Then Instance.TryAdd(d, New Item)
							Instance(d).Counter(Log.Url, Log.Code, Log.Time)
						Next
					End If
				End Using
			End If
		End Sub

	End Class
End Namespace
