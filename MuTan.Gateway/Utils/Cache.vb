'--------------------------------------------------   
'   
'   缓存操作
'   
'   namespace: Utils.Cache
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 缓存操作
'   release: 2020-08-18
'   
'-------------------------------------------------- 

Imports BeetleX
Imports BeetleX.FastHttpApi
Imports Bumblebee.Events
Imports MuTan.Gateway.Model.Cache
Imports System.Runtime.Caching

Namespace Utils

	''' <summary>缓存操作</summary>
	Friend Class Cache

		''' <summary>是否启用缓存项目</summary>
		Public Enabled As Boolean

		''' <summary>缓存对象</summary>
		Private ReadOnly MemoCache As MemoryCache

		''' <summary>进程缓存</summary>
		Private ReadOnly Session As Concurrent.ConcurrentDictionary(Of Long, Data)

		Public Sub New()
			Me.MemoCache = New MemoryCache(Me.GetType.Assembly.GetName.Name)
			Me.Session = New Concurrent.ConcurrentDictionary(Of Long, Data)
		End Sub

		''' <summary>获取缓存 KEY</summary>
		Private Function CacheKey(Request As HttpRequest) As String
			Return Model.Match.Rule.MakeKey(Request, xConfig.CACHE_HEADER, xConfig.CACHE_DATA, xConfig.CACHE_COOKIES, True)
		End Function

		''' <summary>读取缓存</summary>
		Public Sub Read(e As EventRequestingArgs)
			Dim g = e.Gateway
			Dim s = g.HttpServer

			Try
				Dim Time = xDatabase.CacheTime(e.Request)
				If Time > 0 Then
					Dim Item As Data = MemoCache.Get(CacheKey(e.Request))
					If Item?.Finish Then
						' 存在缓存
						If s.EnableLog(EventArgs.LogType.Info) Then
							s.Log(EventArgs.LogType.Info, e.Request.Session, String.Format("网关服务【{0}】 {1} {2} {3} {4} 缓存命中", "读缓存", e.Request.ID, e.Request.RemoteIPAddress, e.Request.Method, e.Request.GetSourceUrl))
						End If

						g.RequestIncrementCompleted(e.Request, 311, TimeWatch.GetTotalMilliseconds() - e.Request.RequestTime, Nothing)

						If (g.Pluginer.RequestedEnabled) Then g.Pluginer.Requested(New EventRequestCompletedArgs(Nothing, e.Request, e.Response, g, 311, Nothing, 0, e.Request.ID, Nothing))

						Item.Response(e.Request)
						e.Cancel = True
						e.ResultType = ResultType.Completed
					End If
				End If
			Catch ex As Exception
				If s.EnableLog(EventArgs.LogType.Warring) Then
					s.Log(EventArgs.LogType.Warring, e.Request.Session, "网关服务【读缓存】 插件处理时发生异常：" & ex.Message & "@" & ex.StackTrace)
				End If

				Dim innerErrorResult As New InnerErrorResult(500, "网关服务【读缓存】 插件处理时发生异常：" + ex.Message)
				g.Response(e.Response, innerErrorResult)

				e.Cancel = True
				e.ResultType = 1
			End Try
		End Sub

		''' <summary>写入缓存</summary>
		Public Sub Write(e As EventRespondingArgs)
			Dim s = e.Gateway.HttpServer

			Try
				Dim Time = xDatabase.CacheTime(e.Request)
				If Time > 0 Then
					Dim Key = CacheKey(e.Request)

					Dim Item As Data = Nothing

					' 第一次接收数据
					If e.FirstReceive Then
						' 创建进程
						Session.TryAdd(e.Request.ID, New Data(e.ResponseStatus, e.Header))

						' 移除原来的缓存
						MemoCache.Remove(Key)
					End If

					' 获取当前进程的缓存数据
					Session.TryGetValue(e.Request.ID, Item)
					If Item Is Nothing Then
						' 获取不到进程数据，删除掉原始进程，此次缓存失败
						Session.TryRemove(e.Request.ID, Nothing)
					Else
						' 存在进程，缓存数据
						Item.Write(e.Data, e.Completed)
					End If

					' 接收完成
					If e.Completed Then
						' 缓存数据
						MemoCache.Set(Key, Item, DateTimeOffset.Now.AddSeconds(Time))

						' 移除进程
						Session.TryRemove(e.Request.ID, Nothing)
					End If
				End If

				If s.EnableLog(EventArgs.LogType.Info) Then
					s.Log(EventArgs.LogType.Info, e.Request.Session, String.Format("网关服务【{0}】 {1} {2} {3} {4} 缓存数据", "写缓存", e.Request.ID, e.Request.RemoteIPAddress, e.Request.Method, e.Request.GetSourceUrl))
				End If
			Catch ex As Exception
				If s.EnableLog(EventArgs.LogType.Warring) Then
					s.Log(EventArgs.LogType.Warring, e.Request.Session, "网关服务【写缓存】 插件处理时发生异常：" & ex.Message & "@" & ex.StackTrace)
				End If
			End Try
		End Sub

	End Class
End Namespace
