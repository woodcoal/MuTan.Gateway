'--------------------------------------------------   
'   
'   参数操作
'   
'   namespace: Utils.Config
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 参数操作
'   release: 2020-08-19
'   
'-------------------------------------------------- 

Imports BeetleX.EventArgs

Namespace Utils

	''' <summary>参数操作</summary>
	Public Class Config
		Public TOKEN_NAME As String
		Public TOKEN_KEY As String
		Public TOKEN_ISSUER As String
		Public TOKEN_AUDIECNCE As String

		Public CACHE_HEADER As List(Of String)
		Public CACHE_DATA As List(Of String)
		Public CACHE_COOKIES As List(Of String)

		Public AUTH_HEADER As List(Of String)
		Public AUTH_DATA As List(Of String)
		Public AUTH_COOKIES As List(Of String)

		Public SYS_HOST As String
		Public SYS_PORT As Integer
		Public SYS_SSL As Boolean
		Public SYS_SSL_PORT As Integer
		Public SYS_SSL_FILE As String
		Public SYS_SSL_PASSWORD As String

		Public SYS_LOG_LEVEL As LogType
		Public SYS_LOG_CONSOLE As Boolean
		Public SYS_LOG_SAVE As Boolean
		Public SYS_LOG_STACKTRACE As Boolean
		Public SYS_LOG_SIZE As Integer

		Public SYS_STATISTICS As Boolean

		''' <summary>记录 CPU，内存，链接信息间隔时间，单位：秒</summary>
		Public COUNTER_SYSTEM_DELAY As Integer

		''' <summary>访问统计时间段间隔（分钟），建议 1、5、10、15、20、30、60 能被整除的数，访问量小的时候间隔时间可以拉长</summary>
		Public COUNTER_STATUS_DELAY As Integer

		''' <summary>访问统计最早时间间隔（小时），本参数小时前到现在的数据才会被用于统计</summary>
		Public COUNTER_STATUS_BEFORE As Integer

		Public Shared ReadOnly Property [Default] As Config
			Get
				Dim Path = GetType(Config).Assembly.GetName.Name & ".json"
				Dim Ret = Utils.ReadJson(Of Config)(Path)
				If Ret Is Nothing Then
					Ret = New Config With {
						.SYS_HOST = "",
						.SYS_PORT = 80,
						.SYS_SSL = False,
						.SYS_SSL_FILE = "",
						.SYS_SSL_PASSWORD = "",
						.SYS_LOG_LEVEL = LogType.Warring,
						.SYS_LOG_CONSOLE = True,
						.SYS_LOG_SAVE = True,
						.SYS_LOG_STACKTRACE = True,
						.SYS_LOG_SIZE = 10000,
						.SYS_STATISTICS = True,
						.COUNTER_SYSTEM_DELAY = 5,
						.COUNTER_STATUS_DELAY = 15,
						.COUNTER_STATUS_BEFORE = 24
					}
				End If
				Ret = If(Ret, New Config)

				Dim hasChange As Boolean = False

				If String.IsNullOrWhiteSpace(Ret.TOKEN_NAME) Then
					Ret.TOKEN_NAME = "Token"
					hasChange = True
				End If

				If String.IsNullOrWhiteSpace(Ret.TOKEN_KEY) OrElse Ret.TOKEN_KEY.Trim.Length < 32 Then
					Ret.TOKEN_KEY = Guid.NewGuid.ToString
					hasChange = True
				End If

				If String.IsNullOrWhiteSpace(Ret.TOKEN_ISSUER) Then
					Ret.TOKEN_ISSUER = GetType(Config).Assembly.GetName.Name
					hasChange = True
				End If

				If String.IsNullOrWhiteSpace(Ret.TOKEN_AUDIECNCE) Then
					Ret.TOKEN_AUDIECNCE = GetType(Config).Assembly.GetName.Version.ToString
					hasChange = True
				End If

				'----------------------------------------------------

				Ret.CACHE_HEADER = If(Ret.CACHE_HEADER, New List(Of String))
				Ret.CACHE_DATA = If(Ret.CACHE_DATA, New List(Of String))
				Ret.CACHE_COOKIES = If(Ret.CACHE_COOKIES, New List(Of String))

				'----------------------------------------------------

				Ret.AUTH_HEADER = If(Ret.AUTH_HEADER, New List(Of String))
				Ret.AUTH_DATA = If(Ret.AUTH_DATA, New List(Of String))
				Ret.AUTH_COOKIES = If(Ret.AUTH_COOKIES, New List(Of String))

				'----------------------------------------------------

				If Ret.SYS_PORT < 1 Or Ret.SYS_PORT > 65536 Then
					Ret.SYS_PORT = 80
					hasChange = True
				End If

				If Ret.SYS_SSL_PORT < 1 Or Ret.SYS_SSL_PORT > 65536 Then
					Ret.SYS_SSL_PORT = 443
					hasChange = True
				End If

				'----------------------------------------------------

				If Ret.SYS_LOG_SIZE < 1 Then
					Ret.SYS_LOG_SIZE = 0
					Ret.SYS_LOG_SAVE = False
				End If

				'----------------------------------------------------

				If Ret.COUNTER_SYSTEM_DELAY < 1 Then
					Ret.COUNTER_SYSTEM_DELAY = 1
					hasChange = True
				End If

				If Ret.COUNTER_STATUS_DELAY < 1 Or Ret.COUNTER_STATUS_DELAY > 60 Then
					Ret.COUNTER_STATUS_DELAY = 60
					hasChange = True
				End If

				If Ret.COUNTER_STATUS_BEFORE < 1 Then
					Ret.COUNTER_STATUS_BEFORE = 1
					hasChange = True
				End If

				'----------------------------------------------------

				' 存在变化，保存数据
				If hasChange Then Ret.Save()

				Return Ret
			End Get
		End Property

		Public Sub Save()
			Dim Path = GetType(Config).Assembly.GetName.Name & ".json"
			SaveJson(Path, Me, True)
		End Sub

	End Class
End Namespace

'Namespace Utils

'	''' <summary>参数操作</summary>
'	Friend Class Config

'		Default Property Item(Name As String) As Object
'			Get
'				Return xDatabase.Config(Name)
'			End Get
'			Set(value As Object)
'				xDatabase.Config(Name) = value
'			End Set
'		End Property

'		Public TOKEN_NAME As String
'		Public TOKEN_KEY As Byte()
'		Public TOKEN_ISSUER As String
'		Public TOKEN_AUDIECNCE As String

'		Public ReadOnly CACHE_HEADER As List(Of String)
'		Public ReadOnly CACHE_DATA As List(Of String)
'		Public ReadOnly CACHE_COOKIES As List(Of String)

'		Public ReadOnly AUTH_HEADER As List(Of String)
'		Public ReadOnly AUTH_DATA As List(Of String)
'		Public ReadOnly AUTH_COOKIES As List(Of String)

'		Public SYS_HOST As String
'		Public SYS_PORT As Integer
'		Public SYS_SSL As Boolean
'		Public SYS_SSL_PORT As Integer
'		Public SYS_SSL_FILE As String
'		Public SYS_SSL_PASSWORD As String

'		Public SYS_LOG_LEVEL As LogType
'		Public SYS_LOG_CONSOLE As Boolean
'		Public SYS_LOG_SAVE As Boolean
'		Public SYS_LOG_STACKTRACE As Boolean

'		Public Sub New()
'			CACHE_HEADER = New List(Of String)
'			CACHE_DATA = New List(Of String)
'			CACHE_COOKIES = New List(Of String)

'			AUTH_HEADER = New List(Of String)
'			AUTH_DATA = New List(Of String)
'			AUTH_COOKIES = New List(Of String)
'		End Sub

'		''' <summary>加载默认参数</summary>
'		Public Sub Load()
'			TOKEN_NAME = Item("TOKEN_NAME")
'			If String.IsNullOrWhiteSpace(TOKEN_NAME) Then
'				TOKEN_NAME = "Token"
'				Item("TOKEN_NAME") = TOKEN_NAME
'			End If

'			TOKEN_KEY = Item("TOKEN_KEY")
'			If TOKEN_KEY Is Nothing OrElse TOKEN_KEY.Length <> 128 Then
'				Dim Keys(127) As Byte
'				Call New Random().NextBytes(Keys)

'				TOKEN_KEY = Keys
'				Item("TOKEN_KEY") = TOKEN_KEY
'			End If

'			TOKEN_ISSUER = Item("TOKEN_ISSUER")
'			If String.IsNullOrWhiteSpace(TOKEN_ISSUER) Then
'				TOKEN_ISSUER = Me.GetType.Assembly.GetName.Name
'				Item("TOKEN_ISSUER") = TOKEN_ISSUER
'			End If

'			TOKEN_AUDIECNCE = Item("TOKEN_AUDIECNCE")
'			If String.IsNullOrWhiteSpace(TOKEN_AUDIECNCE) Then
'				TOKEN_AUDIECNCE = Me.GetType.Assembly.GetName.Version.ToString
'				Item("TOKEN_AUDIECNCE") = TOKEN_AUDIECNCE
'			End If

'			'----------------------------------------------------

'			GetList(CACHE_HEADER, "CACHE_HEADER")
'			GetList(CACHE_DATA, "CACHE_DATA")
'			GetList(CACHE_COOKIES, "CACHE_COOKIES")

'			'----------------------------------------------------

'			GetList(AUTH_HEADER, "AUTH_HEADER")
'			GetList(AUTH_DATA, "AUTH_DATA")
'			GetList(AUTH_COOKIES, "AUTH_COOKIES")

'			'----------------------------------------------------

'			SYS_HOST = Item("SYS_HOST")

'			SYS_PORT = If(Item("SYS_PORT"), 80)
'			If SYS_PORT < 1 Or SYS_PORT > 65536 Then
'				SYS_PORT = 80
'				Item("SYS_PORT") = SYS_PORT
'			End If

'			SYS_SSL = If(Item("SYS_SSL"), False)

'			SYS_SSL_PORT = If(Item("SYS_SSL_PORT"), 443)
'			If SYS_SSL_PORT < 1 Or SYS_SSL_PORT > 65536 Then
'				SYS_SSL_PORT = 443
'				Item("SYS_SSL_PORT") = SYS_SSL_PORT
'			End If

'			SYS_SSL_FILE = Item("SYS_SSL_FILE")
'			SYS_SSL_PASSWORD = Item("SYS_SSL_PASSWORD")

'			'----------------------------------------------------

'			SYS_LOG_LEVEL = If(Item("SYS_LOG_LEVEL"), LogType.Error)
'			SYS_LOG_CONSOLE = If(Item("SYS_LOG_CONSOLE"), True)
'			SYS_LOG_SAVE = If(Item("SYS_LOG_SAVE"), True)
'			SYS_LOG_STACKTRACE = If(Item("SYS_LOG_STACKTRACE"), True)
'		End Sub

'		''' <summary>保存参数</summary>
'		Public Sub Save()
'			Item("TOKEN_NAME") = TOKEN_NAME
'			Item("TOKEN_KEY") = TOKEN_KEY
'			Item("TOKEN_ISSUER") = TOKEN_ISSUER
'			Item("TOKEN_AUDIECNCE") = TOKEN_AUDIECNCE

'			'----------------------------------------------------

'			SetList(CACHE_HEADER, "CACHE_HEADER")
'			SetList(CACHE_DATA, "CACHE_DATA")
'			SetList(CACHE_COOKIES, "CACHE_COOKIES")

'			'----------------------------------------------------

'			SetList(AUTH_HEADER, "AUTH_HEADER")
'			SetList(AUTH_DATA, "AUTH_DATA")
'			SetList(AUTH_COOKIES, "AUTH_COOKIES")

'			'----------------------------------------------------

'			Item("SYS_HOST") = SYS_HOST
'			Item("SYS_PORT") = SYS_PORT
'			Item("SYS_SSL") = SYS_SSL
'			Item("SYS_SSL_PORT") = SYS_SSL_PORT
'			Item("SYS_SSL_FILE") = SYS_SSL_FILE
'			Item("SYS_SSL_PASSWORD") = SYS_SSL_PASSWORD

'			Item("SYS_LOG_LEVEL") = SYS_LOG_LEVEL
'			Item("SYS_LOG_CONSOLE") = SYS_LOG_CONSOLE
'			Item("SYS_LOG_SAVE") = SYS_LOG_SAVE
'			Item("SYS_LOG_STACKTRACE") = SYS_LOG_STACKTRACE
'		End Sub

'		Private Sub GetList(Obj As List(Of String), Name As String)
'			Obj.Clear()

'			Dim List As String = Item(Name)
'			If Not String.IsNullOrWhiteSpace(List) Then
'				Dim Arrs = List.Split("||", StringSplitOptions.RemoveEmptyEntries)
'				If Arrs.Length > 0 Then
'					Obj.AddRange(Arrs)
'				End If
'			End If
'		End Sub

'		Private Sub SetList(Obj As List(Of String), Name As String)
'			If Obj.Count > 0 Then
'				Item(Name) = String.Join("||", Obj.ToArray)
'			Else
'				Item(Name) = ""
'			End If
'		End Sub

'	End Class
'End Namespace
