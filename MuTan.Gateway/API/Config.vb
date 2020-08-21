'--------------------------------------------------   
'   
'   参数设置   
'   
'   namespace: API.Config
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 参数设置
'   release: 2020-08-19
'   
'-------------------------------------------------- 

Imports BeetleX.FastHttpApi

Namespace API

	''' <summary>参数设置</summary>
	<Controller(BaseUrl:="_gateway/config", SkipPublicFilter:=True)>
	<Options(AllowOrigin:="*", AllowHeaders:="*", AllowMethods:="OPTIONS,GET,POST", AllowMaxAge:="86400")>
	Public Class Config

		''' <summary>网关对象</summary>
		Private ReadOnly Gateway As Bumblebee.Gateway

		Public Sub New(Gateway As Bumblebee.Gateway)
			Me.Gateway = Gateway
		End Sub

		''' <summary>加载参数</summary>
		Public Function List() As Utils.Config
			Return xConfig
		End Function

		<Post>
		Public Function SaveCache(Header As String(), Data As String(), Cookies As String()) As Boolean
			Try
				Utils.UpdateArray(Header)
				Utils.UpdateArray(Data)
				Utils.UpdateArray(Cookies)

				xConfig.CACHE_HEADER.Clear()
				xConfig.CACHE_DATA.Clear()
				xConfig.CACHE_COOKIES.Clear()

				If Header.Length > 0 Then xConfig.CACHE_HEADER.AddRange(Header)
				If Data.Length > 0 Then xConfig.CACHE_DATA.AddRange(Data)
				If Cookies.Length > 0 Then xConfig.CACHE_COOKIES.AddRange(Cookies)

				xConfig.Save()

				Return True
			Catch ex As Exception
				Return False
			End Try
		End Function

		<Post>
		Public Function SaveAuth(Header As String(), Data As String(), Cookies As String()) As Boolean
			Try
				Utils.UpdateArray(Header)
				Utils.UpdateArray(Data)
				Utils.UpdateArray(Cookies)

				xConfig.AUTH_HEADER.Clear()
				xConfig.AUTH_DATA.Clear()
				xConfig.AUTH_COOKIES.Clear()

				If Header.Length > 0 Then xConfig.AUTH_HEADER.AddRange(Header)
				If Data.Length > 0 Then xConfig.AUTH_DATA.AddRange(Data)
				If Cookies.Length > 0 Then xConfig.AUTH_COOKIES.AddRange(Cookies)

				xConfig.Save()

				Return True
			Catch ex As Exception
				Return False
			End Try
		End Function

		<Post>
		Public Function SaveToken(item As Utils.Config) As Boolean
			If item IsNot Nothing Then
				Try
					If String.IsNullOrWhiteSpace(item.TOKEN_NAME) Then
						item.TOKEN_NAME = "Token"
					Else
						item.TOKEN_NAME = item.TOKEN_NAME.Trim
					End If
					If item.TOKEN_KEY.ToLower = "authorization" Then item.TOKEN_KEY = "Token"

					If String.IsNullOrWhiteSpace(item.TOKEN_KEY) OrElse item.TOKEN_KEY.Trim.Length < 32 Then
						item.TOKEN_KEY = Guid.NewGuid.ToString
					Else
						item.TOKEN_KEY = item.TOKEN_KEY.Trim
					End If

					If String.IsNullOrWhiteSpace(item.TOKEN_ISSUER) Then
						item.TOKEN_ISSUER = Me.GetType.Assembly.GetName.Name
					Else
						item.TOKEN_ISSUER = item.TOKEN_ISSUER.Trim
					End If

					If String.IsNullOrWhiteSpace(item.TOKEN_AUDIECNCE) Then
						item.TOKEN_AUDIECNCE = Me.GetType.Assembly.GetName.Version.ToString
					Else
						item.TOKEN_AUDIECNCE = item.TOKEN_AUDIECNCE.Trim
					End If

					'-------------------------------------------------

					xConfig.TOKEN_NAME = item.TOKEN_NAME
					xConfig.TOKEN_KEY = item.TOKEN_KEY
					xConfig.TOKEN_ISSUER = item.TOKEN_ISSUER
					xConfig.TOKEN_AUDIECNCE = item.TOKEN_AUDIECNCE
					xConfig.Save()

					Return True
				Catch ex As Exception
				End Try
			End If

			Return False
		End Function

		<Post>
		Public Function SaveLog(item As Utils.Config) As Boolean
			If item IsNot Nothing Then
				Try
					If item.SYS_LOG_SIZE < 1 Then
						item.SYS_LOG_SIZE = 0
						item.SYS_LOG_SAVE = False
					End If

					xConfig.SYS_LOG_LEVEL = item.SYS_LOG_LEVEL
					xConfig.SYS_LOG_CONSOLE = item.SYS_LOG_CONSOLE
					xConfig.SYS_LOG_SAVE = item.SYS_LOG_SAVE
					xConfig.SYS_LOG_STACKTRACE = item.SYS_LOG_STACKTRACE
					xConfig.SYS_LOG_SIZE = item.SYS_LOG_SIZE
					xConfig.SYS_STATISTICS = item.SYS_STATISTICS
					xConfig.Save()

					Gateway.HttpServer.BaseServer.Options.LogLevel = item.SYS_LOG_LEVEL
					Gateway.HttpServer.Options.LogLevel = item.SYS_LOG_LEVEL
					Gateway.HttpServer.Options.LogToConsole = item.SYS_LOG_CONSOLE
					Gateway.HttpServer.Options.OutputStackTrace = item.SYS_LOG_STACKTRACE
					Gateway.HttpServer.Options.CacheLogMaxSize = item.SYS_LOG_SIZE
					Gateway.HttpServer.SaveOptions()

					Gateway.StatisticsEnabled = item.SYS_STATISTICS
					Gateway.SaveConfig()

					Return True
				Catch ex As Exception
				End Try
			End If

			Return False
		End Function

		<Post>
		Public Function SaveOther(item As Utils.Config) As Boolean
			If item IsNot Nothing Then
				Try
					If item.COUNTER_SYSTEM_DELAY < 1 Then item.COUNTER_SYSTEM_DELAY = 0

					' 统计间隔需要能被 60 整除
					If item.COUNTER_STATUS_DELAY < 1 Or item.COUNTER_STATUS_DELAY > 60 Then
						item.COUNTER_STATUS_DELAY = 60
					Else
						For Each delay In {1, 2, 3, 4, 5, 6, 10, 15, 20, 30, 60}
							If item.COUNTER_STATUS_DELAY <= delay Then
								item.COUNTER_STATUS_DELAY = delay
								Exit For
							End If
						Next
					End If

					If item.COUNTER_STATUS_BEFORE < 1 Then item.COUNTER_STATUS_BEFORE = 1

					'-------------------------------------------------

					xConfig.COUNTER_SYSTEM_DELAY = item.COUNTER_SYSTEM_DELAY
					xConfig.COUNTER_STATUS_DELAY = item.COUNTER_STATUS_DELAY
					xConfig.COUNTER_STATUS_BEFORE = item.COUNTER_STATUS_BEFORE
					xConfig.Save()

					Return True
				Catch ex As Exception
				End Try
			End If

			Return False
		End Function

	End Class

End Namespace