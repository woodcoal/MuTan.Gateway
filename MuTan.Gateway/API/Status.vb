'--------------------------------------------------   
'   
'   服务器状态   
'   
'   namespace: API.Status
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 服务器状态
'   release: 2020-07-22
'   
'-------------------------------------------------- 

Imports System.Collections.Concurrent
Imports System.Runtime.InteropServices
Imports BeetleX.FastHttpApi

Namespace API

	''' <summary>服务器状态</summary>
	<Controller(BaseUrl:="_gateway/status", SkipPublicFilter:=True)>
	<Options(AllowOrigin:="*", AllowHeaders:="*", AllowMethods:="OPTIONS,GET,POST", AllowMaxAge:="86400")>
	Public Class Status

		''' <summary>统计数据</summary>
		Private ReadOnly Instance As ConcurrentDictionary(Of Long, Counter.Status.Item)

		''' <summary>网关对象</summary>
		Private ReadOnly Gateway As Bumblebee.Gateway

		Public Sub New(Gateway As Bumblebee.Gateway, Status As Counter.Status)
			Me.Gateway = Gateway
			Me.Instance = Status.Instance
		End Sub

		''' <summary>环境基本信息</summary>
		Public Function Environment(ByVal context As IHttpContext) As Object
			Return New With {
				.Server = New With {
					.BeetleX = GetType(BeetleX.ServerHandlerBase).Assembly.GetName().Version.ToString,
					.FastHttpApi = GetType(Header).Assembly.GetName().Version.ToString,
					.Bumblebee = GetType(Bumblebee.Gateway).Assembly.GetName().Version.ToString,
					.Gateway = Me.GetType.Assembly.GetName().Version.ToString,
					context.Server.Options.Host,
					context.Server.Options.Port,
					context.Server.Options.SSL,
					context.Server.Options.SSLPort,
					context.Server.Name,
					context.Server.StartTime
				},
				.System = New With {
					System.Environment.MachineName,
					RuntimeInformation.OSDescription,
					.OSVersion = System.Environment.OSVersion.VersionString,
					.Version = System.Environment.Version.ToString,
					System.Environment.ProcessorCount
				},
				.Client = xJWT.Current(context.Request).Name,
				.Config = New With {
					xConfig.COUNTER_STATUS_BEFORE,
					xConfig.COUNTER_STATUS_DELAY
				}
			}
		End Function

		''' <summary>状态统计</summary>
		Public Function Information() As Object
			Dim Counter As New List(Of Dictionary(Of String, Long))

			For Each d In Instance.OrderBy(Function(x) x.Key).Select(Function(x) x.Key).ToList
				Dim items = Instance(d).Instance
				Counter.Add(New Dictionary(Of String, Long) From {
					{"Time", d},
					{"ALL", items("ALL").Count},
					{"2x", items("2x").Count},
					{"3x", items("3x").Count},
					{"4x", items("4x").Count},
					{"5x", items("5x").Count},
					{"Cache", items("Cache").Count},
					{"10", items("10").Count},
					{"10_50", items("10_50").Count},
					{"50_100", items("50_100").Count},
					{"100_200", items("100_200").Count},
					{"200_500", items("200_500").Count},
					{"500_1000", items("500_1000").Count},
					{"1000_2000", items("1000_2000").Count},
					{"2000_5000", items("2000_5000").Count},
					{"5000", items("5000").Count}
				})
			Next

			Dim Span = Date.Now.Subtract(Gateway.HttpServer.StartTime)
			Dim RunTime = New With {Span.Days, Span.Hours, Span.Minutes, Span.Seconds}

			Dim Data = Gateway.Statistics.GetData()
			Dim Statistics = New With {
				.Times = Data.Statistics.Times.ToDictionary(Function(x) x.Name, Function(x) x.Count),
				.Counter = {Data.All.Count, Data._1xx.Count, Data._2xx.Count, Data._3xx.Count, Data._4xx.Count, Data._5xx.Count, Data.Other.Count},
				.Rps = {Data.All.Rps, Data._1xx.Rps, Data._2xx.Rps, Data._3xx.Rps, Data._4xx.Rps, Data._5xx.Rps, Data.Other.Rps}
			}

			Dim Now = New With {
				xSystemInforamtion.Now.CPU,
				xSystemInforamtion.Now.Memory,
				xSystemInforamtion.Now.Request,
				xSystemInforamtion.Now.Connections,
				Gateway.HttpServer.BaseServer.ReceivBytes,
				Gateway.HttpServer.BaseServer.SendBytes
			}

			Return New With {Counter, Statistics, .Status = xSystemInforamtion.List(100), Now, RunTime}
		End Function

		''' <summary>访问最多的地址</summary>
		Public Function TopUrl(d As Long, name As String, Optional Count As Integer = 10) As Dictionary(Of String, Long)
			If Instance.ContainsKey(d) Then
				Dim items = Instance(d).Instance
				If items.ContainsKey(name) Then
					Return items(name).Top(Count)
				End If
			End If

			Return Nothing
		End Function

		'Public Function GetUrlSummary() As Object
		'	If Not Gateway.StatisticsEnabled Then
		'		Return New With {
		'			.Total = New Object(-1) {},
		'			.Rps = New Object(-1) {},
		'			.[Error] = New Object(-1) {}
		'		}
		'	End If

		'	Dim Source = Gateway.Routes.UrlStatisticsDB.GetStatistics.Select(Function(x) x.GetResult).ToArray

		'	Dim Source2 = Source.Where(Function(x) x.All.All.Count > 0).OrderByDescending(Function(x) x.All.All.Count).Take(10).Select(Function(x) New With {x.All.Url, .Value = x.All.All.Count}).ToArray
		'	Dim totalMax = Source2.Sum(Function(x) x.Value)

		'	Dim Source3 = Source.Where(Function(x) x.All.All.Rps > 0).OrderByDescending(Function(x) x.All.All.Rps).Take(10).Select(Function(x) New With {x.All.Url, .Value = x.All.All.Rps}).ToArray
		'	Dim rpsMax As Long = Source3.Sum(Function(x) x.Value)

		'	Dim Source4 = Source.Where(Function(x) x.All._5xx.Rps > 0).OrderByDescending(Function(x) x.All._5xx.Rps).Take(10).Select(Function(x) New With {x.All.Url, .Value = x.All._5xx.Rps, x.Statistics}).ToArray
		'	Dim errorMax As Long = Source4.Sum(Function(x) x.Value)

		'	Return New With {
		'		.Total = Source2.Select(Function(x) New With {
		'			x.Url,
		'			x.Value,
		'			.Percent = x.Value / totalMax * 100
		'		}),
		'		.Rps = Source3.Select(Function(x) New With {
		'			x.Url,
		'			x.Value,
		'			.Percent = x.Value / rpsMax * 100
		'		}),
		'		.Error = Source4.Select(Function(x) New With {
		'			x.Url,
		'			x.Value,
		'			.Percent = x.Value / errorMax * 100,
		'			.Codes = x.Statistics.ListStatisticsData(500, 600, Function(y) New With {y.Rps, y.Name})
		'		}).ToArray
		'	}
		'End Function

		'Public Function Status(ByVal context As IHttpContext) As Object
		'	Dim Stat = Gateway.HttpServer.ServerCounter.Next(False)
		'	Dim Data = Gateway.Statistics.GetData()
		'	Dim Ret = New With {
		'		.Statistics = New With {
		'			.Times = Data.Statistics.Times.ToDictionary(Function(x) x.Name, Function(x) x.Count),
		'			.Counter = {Data.All.Count, Data._1xx.Count, Data._2xx.Count, Data._3xx.Count, Data._4xx.Count, Data._5xx.Count, Data.Other.Count},
		'			.Rps = {Data.All.Rps, Data._1xx.Rps, Data._2xx.Rps, Data._3xx.Rps, Data._4xx.Rps, Data._5xx.Rps, Data.Other.Rps}
		'		},
		'		.Status = Stat,
		'		.Queue = Gateway.IOQueue.Count,
		'		.Memory = Stat.Memory / 1024,
		'		.Buffers = BufferMonitor.CreateCount - BufferMonitor.FreeCount,
		'		.BufferSize = BufferMonitor.Size,
		'		.System = New With {
		'			Environment.MachineName,
		'			RuntimeInformation.OSDescription,
		'			.OSVersion = System.Environment.OSVersion.VersionString,
		'			.Version = System.Environment.Version.ToString,
		'			System.Environment.ProcessorCount
		'		},
		'		.Client = xJWT.Current(context.Request).Name
		'	}

		'	' 需要隐藏的数据
		'	Ret.Status.Actions = Nothing

		'	Return Ret
		'End Function


	End Class

End Namespace