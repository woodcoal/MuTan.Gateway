'--------------------------------------------------   
'   
'   公共入口   
'   
'   namespace: Program
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 公共入口
'   release: 2020-07-20
'   
'-------------------------------------------------- 

Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Hosting
Imports System.Threading

Friend Module Program

	''' <summary>客户端操作对象</summary>
	Friend xDatabase As Utils.Database

	''' <summary>JWT操作对象</summary>
	Friend xJWT As Utils.JWT

	''' <summary>授权验证对象</summary>
	Friend xAuthorization As Utils.Authorization

	''' <summary>缓存对象</summary>
	Friend xCache As Utils.Cache

	''' <summary>参数对象</summary>
	Friend xConfig As Utils.Config

	''' <summary>系统信息统计</summary>
	Friend xSystemInforamtion As Counter.System

	''' <summary>超级管理员权限名称</summary>
	Friend Const Super As String = "_.SUPER._"

	''' <summary>自动检测并获取实际路径</summary>
	''' <param name="source"></param>
	''' <param name="tryCreate">是否尝试创建此路径的上级目录，如：d:\a\b\c True 则自动创建 d:\a\b 的目录</param>
	''' <param name="isFolder">当前获取的是目录还是文件地址，以便建立对应的目录</param>
	Public Function Root(ByVal source As String, Optional tryCreate As Boolean = False, Optional isFolder As Boolean = False) As String
		Dim R = AppDomain.CurrentDomain.BaseDirectory

		If Not String.IsNullOrWhiteSpace(source) Then
			Dim sp = IO.Path.DirectorySeparatorChar

			source = source.Replace("\", sp)
			source = source.Replace("/", sp)

			If R.Substring(1, 1) = ":" AndAlso source.StartsWith(sp) Then
				R = R.Substring(0, 2) & source
			Else
				R = IO.Path.Combine(R, source)
			End If

			If tryCreate Then
				Dim f = If(isFolder, R, IO.Path.GetDirectoryName(R))
				If Not IO.Directory.Exists(f) Then IO.Directory.CreateDirectory(f)
			End If
		End If

		Return R
	End Function


	Friend Sub Main()
		Dim builder = New HostBuilder().ConfigureServices(Sub(hostContext, services) services.AddHostedService(Of HttpServerHost))
		builder.Build().Run()
	End Sub

End Module

Friend Class HttpServerHost
	Implements IHostedService

	Private g As Bumblebee.Gateway

	Public Function StartAsync(cancellationToken As CancellationToken) As Task Implements IHostedService.StartAsync
		' 加载数据库对象
		xDatabase = New Utils.Database

		' 加载参数对象
		xConfig = Utils.Config.Default

		' 加载JWT操作对象
		xJWT = New Utils.JWT(xConfig.TOKEN_ISSUER, xConfig.TOKEN_AUDIECNCE, xConfig.TOKEN_KEY)

		' 授权验证对象
		xAuthorization = New Utils.Authorization

		' 缓存对象
		xCache = New Utils.Cache

		' 网关对象
		g = New Bumblebee.Gateway
		g.HttpOptions(Sub(o)
						  o.Host = xConfig.SYS_HOST
						  o.Port = xConfig.SYS_PORT
						  o.SSL = xConfig.SYS_SSL
						  o.SSLPort = xConfig.SYS_SSL_PORT
						  o.CertificateFile = xConfig.SYS_SSL_FILE
						  o.CertificatePassword = xConfig.SYS_SSL_FILE

						  o.LogLevel = xConfig.SYS_LOG_LEVEL
						  o.LogToConsole = xConfig.SYS_LOG_CONSOLE
						  o.WriteLog = xConfig.SYS_LOG_SAVE
						  o.OutputStackTrace = xConfig.SYS_LOG_STACKTRACE
					  End Sub)
		g.StatisticsEnabled = xConfig.SYS_STATISTICS
		g.Open()
		g.LoadPlugin(Me.GetType.Assembly)

		'Dim Files = IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
		'If Files?.Length > 0 Then
		'    For Each File In Files
		'        Try
		'            Me.g.LoadPlugin(Assembly.LoadFile(File))
		'        Catch ex As Exception
		'        End Try
		'    Next
		'End If

		' 系统信息统计
		xSystemInforamtion = New Counter.System(g)

		Return Task.CompletedTask
	End Function

	Public Function StopAsync(cancellationToken As CancellationToken) As Task Implements IHostedService.StopAsync
		xSystemInforamtion.Dispose()
		xDatabase.Dispose()

		g.Dispose()
		Return Task.CompletedTask
	End Function

End Class