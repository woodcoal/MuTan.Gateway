'--------------------------------------------------   
'   
'   数据库操作
'   
'   namespace: Utils.Database
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 数据库操作
'   release: 2020-08-18
'   
'-------------------------------------------------- 

Imports BeetleX.FastHttpApi
Imports LiteDB

Namespace Utils

	''' <summary>数据库操作</summary>
	Friend Class Database
		Implements IDisposable

		''' <summary>数据库对象</summary>
		Private ReadOnly Db As LiteDatabase

		''' <summary>缓存规则对象</summary>
		Private ReadOnly CacheRules As List(Of Model.Match.Data(Of Integer))

#Region "基本操作"

		Public Sub New()
			Dim File = Root(Me.GetType.Assembly.GetName.Name & ".db")
			Db = New LiteDatabase(File)

			' 管理员检测
			ClientManagerCheck()

			' 缓存规则加载
			CacheRules = New List(Of Model.Match.Data(Of Integer))
			CacheLoad()
		End Sub

		Public Sub Dispose() Implements IDisposable.Dispose
			Db.Dispose()
			GC.SuppressFinalize(Me)
		End Sub

#End Region

		'#Region "参数"

		'		''' <summary>获取参数</summary>
		'		Public Property Config(Key As String) As Object
		'			Get
		'				Call UpdateKey(Key)

		'				If Not String.IsNullOrEmpty(Key) Then
		'					Dim Table = Db.GetCollection(Of Model.Config)
		'					Table.EnsureIndex(Function(x) x.Key, True)

		'					Return Table.FindOne(Function(x) x.Key.Equals(Key))?.Value
		'				End If

		'				Return Nothing
		'			End Get
		'			Set(value As Object)
		'				Call UpdateKey(Key)

		'				If Not String.IsNullOrEmpty(Key) Then
		'					Dim Table = Db.GetCollection(Of Model.Config)
		'					Table.EnsureIndex(Function(x) x.Key, True)

		'					Dim Item = Table.FindOne(Function(x) x.Key.Equals(Key))
		'					If Item IsNot Nothing Then
		'						If value Is Nothing Then
		'							' 值不存在，删除此参数
		'							Table.DeleteMany(Function(x) x.Key.Equals(Key))
		'						Else
		'							' 存在，更新数据
		'							Item.Value = value
		'							Table.Update(Item)
		'						End If
		'						Item.Value = value
		'					Else
		'						' 存在值，进行存储
		'						If value IsNot Nothing Then
		'							Item = New Model.Config With {
		'								.Key = Key,
		'								.Value = value
		'							}

		'							Table.Insert(Item)
		'						End If
		'					End If
		'				End If
		'			End Set
		'		End Property

		'#End Region

#Region "缓存"

		''' <summary>所有缓存规则</summary>
		Public Function CacheList() As Dictionary(Of String, Integer)
			Dim Table = Db.GetCollection(Of Model.Cache.Rule)
			Return Table.FindAll.ToDictionary(Function(x) x.API, Function(x) x.Time)
		End Function

		''' <summary>获取缓存规则</summary>
		Public Function CacheItem(API As String) As Integer
			Call UpdateMatchRule(API)
			If Not String.IsNullOrWhiteSpace(API) Then
				Dim Table = Db.GetCollection(Of Model.Cache.Rule)
				Dim Rule = Table.FindOne(Function(x) x.API.Equals(API))
				If Rule IsNot Nothing Then Return Rule.Time
			End If

			Return -1
		End Function

		''' <summary>更新缓存规则</summary>
		Public Function CacheUpdate(API As String, Time As Integer) As Boolean
			Dim R = False

			Call UpdateMatchRule(API)
			If Not String.IsNullOrEmpty(API) AndAlso Time > 0 Then
				Try
					Dim Table = Db.GetCollection(Of Model.Cache.Rule)
					Dim Item = Table.FindOne(Function(x) x.API.Equals(API))
					If Item Is Nothing Then
						' 不存在，添加
						Item = New Model.Cache.Rule With {
								.API = API,
								.Time = Time,
								.Created = Date.Now,
								.Update = Date.Now
							}

						Table.Insert(Item)
						R = True
					Else
						' 更新
						Item.Time = Time
						Item.Update = Date.Now

						R = Table.Update(Item)
					End If
				Catch ex As Exception
				End Try
			End If

			' 更新缓存
			If R Then Call CacheLoad()

			Return R
		End Function

		''' <summary>删除缓存规则</summary>
		Public Function CacheRemove(API As String) As Boolean
			Call UpdateMatchRule(API)
			If Not String.IsNullOrEmpty(API) Then
				Dim Table = Db.GetCollection(Of Model.Cache.Rule)
				If Table.DeleteMany(Function(x) x.API.Equals(API)) > 0 Then
					Call CacheLoad()
					Return True
				End If
			End If

			Return False
		End Function

		''' <summary>重载缓存列表</summary>
		Public Sub CacheLoad()
			SyncLock CacheRules
				CacheRules.Clear()

				Dim List = CacheList()
				If List?.Count > 0 Then
					For Each Item In List
						CacheRules.Add(New Model.Match.Data(Of Integer)(Item.Key, Item.Value))
					Next
				End If
			End SyncLock
		End Sub

		''' <summary>获取 API 对应的缓存时间</summary>
		Public Function CacheTime(request As HttpRequest) As Integer
			Dim Time = -1

			If CacheRules.Count > 0 AndAlso request IsNot Nothing Then
				Select Case request.Method
					Case "GET", "POST", "PUT"
						' 获取 TIME
						For Each Rule In CacheRules
							Dim RuleTime = Rule.GetResult(request)
							If RuleTime > 0 Then
								Time = RuleTime
								Exit For
							End If
						Next
				End Select
			End If

			Return Time
		End Function

#End Region

#Region "权限"

		''' <summary>所有权限</summary>
		Public Function RoleList() As Dictionary(Of String, String())
			Dim Table = Db.GetCollection(Of Model.Role)
			Return Table.FindAll.ToDictionary(Function(x) x.Name, Function(x) x.APIs)
		End Function

		''' <summary>权限分页列表</summary>
		Public Function RolePages(Optional Keyword As String = "", Optional PageIndex As Integer = 1, Optional PageCount As Integer = 10) As (Count As Integer, Result As List(Of Model.Role))
			If PageIndex < 1 Then PageIndex = 1
			If PageCount < 1 Then PageCount = 10
			If PageCount > 100 Then PageCount = 100

			Dim Count = 0
			Dim Result As List(Of Model.Role) = Nothing

			Dim Table = Db.GetCollection(Of Model.Role)
			Call UpdateKey(Keyword)
			If String.IsNullOrEmpty(Keyword) Then
				Count = Table.Count

				Dim PageMax = Math.Ceiling(Count / PageCount)
				If PageIndex > PageMax Then PageIndex = PageMax

				Result = Table.Find(New Query, (PageIndex - 1) * PageCount, PageCount).ToList
			Else
				' 最多查询 10000 条
				Result = Table.Find(Function(x) x.Name.Contains(Keyword, StringComparison.OrdinalIgnoreCase), 0, 10000)
				Count = Result?.Count
				If Count > 0 Then
					Dim PageMax = Math.Ceiling(Count / PageCount)
					If PageIndex > PageMax Then PageIndex = PageMax

					Result = Result.Skip((PageIndex - 1) * PageCount).Take(PageCount).ToList
				End If
			End If

			Return (Count, Result)
		End Function


		''' <summary>指定权限</summary>
		Public Function RoleItem(Name As String) As Model.Role
			Call UpdateName(Name)
			If Not String.IsNullOrEmpty(Name) Then
				Dim Table = Db.GetCollection(Of Model.Role)
				Return Table.FindOne(Function(x) x.Name.Equals(Name))
			End If

			Return Nothing
		End Function

		''' <summary>更新权限，不存在则添加</summary>
		Public Function RoleUpdate(Name As String, ParamArray Apis() As String) As Boolean
			Dim R = False

			UpdateName(Name)
			UpdateMatchRules(Apis)

			If Not String.IsNullOrEmpty(Name) AndAlso Apis.Length > 0 Then
				Try
					Dim Table = Db.GetCollection(Of Model.Role)
					Dim Item = Table.FindOne(Function(x) x.Name.Equals(Name))

					If Item Is Nothing Then
						' 不存在
						Item = New Model.Role With {
							.Name = Name,
							.APIs = Apis,
							.Created = Date.Now,
							.Update = Date.Now
						}

						Table.Insert(Item)
						R = True
					Else
						Item.APIs = Apis
						Item.Update = Date.Now

						R = Table.Update(Item)
					End If

					If R Then Call xAuthorization.Reload()
				Catch ex As Exception
				End Try
			End If

			Return R
		End Function

		''' <summary>删除权限</summary>
		Public Function RoleRemove(Name As String) As Boolean
			UpdateName(Name)
			If Not String.IsNullOrEmpty(Name) Then
				Dim Table = Db.GetCollection(Of Model.Role)
				If Table.DeleteMany(Function(x) x.Name.Equals(Name)) > 0 Then
					Call xAuthorization.Reload()
					Return True
				End If
			End If

			Return False
		End Function

#End Region

#Region "客户端"

		''' <summary>检查是否存在超级管理员</summary>
		Public Sub ClientManagerCheck()
			Dim Table = Db.GetCollection(Of Model.Client)
			Table.EnsureIndex(Function(x) x.Name, True)
			Table.EnsureIndex(Function(x) x.Role)

			If Not Table.Exists(Function(x) x.Role.Equals(Super)) Then
				Dim Name = "manager"

				If Table.Exists(Function(x) x.Name.Equals(Name)) Then Table.DeleteMany(Function(x) x.Name.Equals(Name))

				Dim Item As New Model.Client With {
					.Name = Name,
					.Key = Guid.NewGuid.ToString,
					.Role = Super,
					.Open = True,
					.Created = Date.Now,
					.Update = Date.Now
				}

				Table.Insert(Item)

				Utils.SaveJson("_manager.json", New With {Item.Name, Item.Key}, True)
			End If
		End Sub

		''' <summary>所有客户端</summary>
		Public Function ClientList() As List(Of Model.Client)
			Dim Table = Db.GetCollection(Of Model.Client)
			Return Table.FindAll.ToList
		End Function

		''' <summary>客户端分页列表</summary>
		Public Function ClientPages(Optional Keyword As String = "", Optional PageIndex As Integer = 1, Optional PageCount As Integer = 10) As (Count As Integer, Result As List(Of Model.Client))
			If PageIndex < 1 Then PageIndex = 1
			If PageCount < 1 Then PageCount = 10
			If PageCount > 100 Then PageCount = 100

			Dim Count = 0
			Dim Result As List(Of Model.Client) = Nothing

			Dim Table = Db.GetCollection(Of Model.Client)
			Call UpdateKey(Keyword)
			If String.IsNullOrEmpty(Keyword) Then
				Count = Table.Count

				Dim PageMax = Math.Ceiling(Count / PageCount)
				If PageIndex > PageMax Then PageIndex = PageMax

				Result = Table.Find(New Query, (PageIndex - 1) * PageCount, PageCount).ToList
			Else
				' 最多查询 10000 条
				Result = Table.Find(Function(x) x.Name.Contains(Keyword, StringComparison.OrdinalIgnoreCase), 0, 10000)
				Count = Result?.Count
				If Count > 0 Then
					Dim PageMax = Math.Ceiling(Count / PageCount)
					If PageIndex > PageMax Then PageIndex = PageMax

					Result = Result.Skip((PageIndex - 1) * PageCount).Take(PageCount).ToList
				End If
			End If

			If Result?.Count > 0 Then
				For Each R In Result
					R.Key = ""
				Next
			End If

			Return (Count, Result)
		End Function

		''' <summary>客户端详情</summary>
		Public Function ClientItem(Name As String) As Model.Client
			Call UpdateName(Name)
			If Not String.IsNullOrEmpty(Name) Then
				Dim Table = Db.GetCollection(Of Model.Client)
				Return Table.FindOne(Function(x) x.Name.Equals(Name))
			End If

			Return Nothing
		End Function

		''' <summary>添加项目</summary>
		Public Function ClientInsert(Name As String, Optional Role As String = "", Optional Open As Boolean = True) As Boolean
			Dim R = False

			Call UpdateName(Name)
			If Not String.IsNullOrEmpty(Name) Then
				Try
					Dim Table = Db.GetCollection(Of Model.Client)
					If Not Table.Exists(Function(x) x.Name.Equals(Name)) Then
						Dim ClientRole = RoleItem(Role)
						If ClientRole Is Nothing Then
							Role = ""
						Else
							Role = ClientRole.Name
						End If

						Dim Item As New Model.Client With {
							.Name = Name,
							.Role = Role,
							.Key = Guid.NewGuid.ToString,
							.Open = Open,
							.Created = Date.Now,
							.Update = Date.Now
						}

						Table.Insert(Item)
						R = True
					End If
				Catch ex As Exception
				End Try
			End If

			Return R
		End Function

		''' <summary>更新项目，管理员不能修改权限</summary>
		Public Function ClientUpdate(client As Model.Client) As Boolean
			Dim Name = client?.Name
			UpdateName(Name)
			If Not String.IsNullOrEmpty(Name) Then
				Dim Table = Db.GetCollection(Of Model.Client)
				Dim Item = Table.FindOne(Function(x) x.Name.Equals(Name))
				If Item IsNot Nothing AndAlso Item.Role <> Super Then
					Dim ClientRole = RoleItem(client.Role)
					If ClientRole Is Nothing Then
						client.Role = ""
					Else
						client.Role = ClientRole.Name
					End If

					Item.Role = client.Role
					Item.TimeStart = client.TimeStart
					Item.TimeStop = client.TimeStop
					Item.Open = client.Open
					Item.CloseMessage = client.CloseMessage
					Item.Version = client.Version
					Item.Config = client.Config
					Item.Update = Date.Now

					Return Table.Update(Item)
				End If
			End If

			Return False
		End Function

		''' <summary>更新密匙</summary>
		Public Function ClientChangeKey(Name As String) As String
			UpdateName(Name)
			If Not String.IsNullOrEmpty(Name) Then
				Dim Table = Db.GetCollection(Of Model.Client)
				Dim Item = Table.FindOne(Function(x) x.Name.Equals(Name))
				If Item IsNot Nothing Then
					Item.Key = Guid.NewGuid.ToString
					If Table.Update(Item) Then Return Item.Key
				End If
			End If

			Return ""
		End Function

		''' <summary>删除</summary>
		Public Function ClientRemove(Name As String) As Boolean
			UpdateName(Name)
			If Not String.IsNullOrEmpty(Name) Then
				Dim Table = Db.GetCollection(Of Model.Client)
				Return Table.DeleteMany(Function(x) x.Name.Equals(Name)) > 0
			End If

			Return False
		End Function

#End Region


	End Class
End Namespace

#Region "原始操作"

'Namespace Utils
'	''' <summary>Client 操作</summary>
'	Public Class Client
'		Implements IDisposable

'		Private ReadOnly Db As LiteDatabase
'		Private ReadOnly Apis As ConcurrentDictionary(Of String, List(Of String))
'		Private ReadOnly DefApis As Dictionary(Of String, List(Of String))
'		Private ReadOnly GloApis As List(Of String)
'		Public ReadOnly JWT As JWT

'#Region "基本操作"

'		Public Sub New()
'			Dim File = Path.Root(Me.GetType.Assembly.GetName.Name & ".db")
'			Db = New LiteDatabase(File)

'			' 检查是否存在超级管理员
'			Dim Table = Db.GetCollection(Of Model.Client)
'			Table.EnsureIndex(Function(x) x.Name, True)
'			Table.EnsureIndex(Function(x) x.Role)

'			If Not Table.Exists(Function(x) x.Role.Equals(Super)) Then
'				Dim Name = "manager"

'				If Table.Exists(Function(x) x.Name.Equals(Name)) Then Table.DeleteMany(Function(x) x.Name.Equals(Name))

'				Dim c As New Model.Client With {
'					.Name = Name,
'					.Key = Guid.NewGuid.ToString,
'					.Role = Super,
'					.Open = True,
'					.Created = Timer.Now,
'					.Update = Timer.Now
'				}

'				Table.Insert(c)

'				Serialize.JsonToFile("_manager.json", c)
'			End If

'			' 加载 Api
'			Me.GloApis = New List(Of String)
'			Me.DefApis = New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)
'			Me.Apis = New ConcurrentDictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)

'			' 注册需要管理员权限 API
'			RegisterAPI("/_gateway/client/login")
'			RegisterAPI("/_gateway/client/logout")
'			RegisterAPI("/_gateway/client/refresh")
'			RegisterAPI("/_gateway/status/")
'			RegisterAPI("/_gateway/counter/")

'			RegisterAPI("/_gateway/*", Super)
'			'RegisterAPI("/_gateway/client/api", Super)
'			'RegisterAPI("/_gateway/client/client*", Super)
'			'RegisterAPI("/_gateway/client/role*", Super)
'			'RegisterAPI("/_gateway/log*", Super)
'			'RegisterAPI("/_gateway/plugin*", Super)
'			'RegisterAPI("/_gateway/rewrite*", Super)
'			'RegisterAPI("/_gateway/route*", Super)
'			'RegisterAPI("/_gateway/server*", Super)

'			ReloadAPIs()

'			' 加载 JWT 对象
'			' 加载默认密匙
'			Dim tokenFile = Path.Root("token.json", True)
'			Dim token = If(IO.File.Exists(tokenFile), IO.File.ReadAllBytes(tokenFile), Nothing)
'			If token Is Nothing OrElse token.Length <> 128 Then
'				Dim Keys(127) As Byte
'				Call New Random().NextBytes(Keys)

'				IO.File.WriteAllBytes(tokenFile, Keys)
'				token = Keys
'			End If

'			Me.JWT = New Gateway.Utils.JWT(Me.GetType.Assembly.GetName.Name, Me.GetType.FullName, token)
'		End Sub

'		Public Sub Dispose() Implements IDisposable.Dispose
'			Db.Dispose()
'			GC.SuppressFinalize(Me)
'		End Sub

'#End Region

'#Region "客户端"

'		''' <summary>分页列表</summary>
'		Public Function Pages(Optional Keyword As String = "", Optional PageIndex As Integer = 1, Optional PageCount As Integer = 10) As (Count As Integer, Result As List(Of Model.Client))
'			If PageIndex < 1 Then PageIndex = 1
'			If PageCount < 1 Then PageCount = 10
'			If PageCount > 100 Then PageCount = 100

'			Dim Table = Db.GetCollection(Of Model.Client)
'			Dim MaxCount = Table.Count

'			If MaxCount > 0 Then
'				If String.IsNullOrWhiteSpace(Keyword) Then
'					Dim PageMax = Math.Ceiling(MaxCount / PageCount)
'					If PageIndex > PageMax Then PageIndex = PageMax

'					Dim Result = Table.Find(New Query, (PageIndex - 1) * PageCount, PageCount).ToList
'					If Result?.Count > 0 Then
'						For Each R In Result
'							R.Key = ""
'						Next

'						Return (MaxCount, Result)
'					End If
'				Else
'					' 最多查询 1000 条
'					Dim Result = Table.Find(Function(x) x.Name.Contains(Keyword, StringComparison.OrdinalIgnoreCase), 0, 1000)

'					MaxCount = Result.Count
'					If MaxCount > 0 Then
'						Dim PageMax = Math.Ceiling(MaxCount / PageCount)
'						If PageIndex > PageMax Then PageIndex = PageMax

'						Result = Result.Skip((PageIndex - 1) * PageCount).Take(PageCount).ToList

'						If Result?.Count > 0 Then Return (MaxCount, Result)
'					End If
'				End If
'			End If

'			Return (MaxCount, Nothing)
'		End Function

'		''' <summary>搜索</summary>
'		Public Function Search(Name As String) As List(Of Model.Client)
'			Name = Core.String.Clear.Trim(Name)
'			If Not String.IsNullOrWhiteSpace(Name) Then
'				Dim Table = Db.GetCollection(Of Model.Client)
'				Dim MaxCount = Table.Count

'				If MaxCount > 0 Then
'					Dim Result = Table.Find(Function(x) x.Name.Contains(Name, StringComparison.OrdinalIgnoreCase), 0, 100).ToList
'					If Result?.Count > 0 Then
'						For Each R In Result
'							R.Key = ""
'						Next

'						Return Result
'					End If
'				End If
'			End If

'			Return Nothing
'		End Function

'		''' <summary>获取详情</summary>
'		Public Function Item(Name As String) As Model.Client
'			If Core.String.Validate.UserName(Name) Then
'				Dim Table = Db.GetCollection(Of Model.Client)
'				Dim MaxCount = Table.Count

'				If MaxCount > 0 Then
'					Return Table.FindOne(Function(x) x.Name.Equals(Name))
'				End If
'			End If

'			Return Nothing
'		End Function

'		''' <summary>添加项目</summary>
'		Public Function Insert(Name As String, Optional Role As String = "", Optional Open As Boolean = True, Optional TimeStart As Date = Nothing, Optional TimeStop As Date = Nothing, Optional CloseMessage As String = "", Optional Version As Single = 0) As (Result As Boolean, Message As String)
'			Dim Message = ""
'			If Core.String.Validate.UserName(Name) Then
'				Name = Name.ToLower

'				Dim Table = Db.GetCollection(Of Model.Client)
'				If Table.Exists(Function(x) x.Name.Equals(Name)) Then
'					Message = "此名称已经存在"
'				Else
'					If Me.Role(Role) Is Nothing Then
'						Role = ""
'					Else
'						Role = Role.ToLower
'					End If

'					Dim C As New Model.Client With {
'						.Name = Name,
'						.Role = Role,
'						.Key = Guid.NewGuid.ToString,
'						.TimeStart = TimeStart,
'						.TimeStop = TimeStop,
'						.Open = Open,
'						.CloseMessage = CloseMessage,
'						.Version = Version,
'						.Created = Timer.Now,
'						.Update = Timer.Now
'					}

'					Table.Insert(C)
'				End If
'			Else
'				Message = "名称只能使用字母和数字，且必须字母开头，最少3个，最多24个"
'			End If

'			Return (String.IsNullOrEmpty(Message), Message)
'		End Function

'		''' <summary>更新项目，管理员不能修改权限</summary>
'		Public Function Update(client As Model.Client) As Boolean
'			If client?.Name?.Length > 0 Then
'				Dim Name = client.Name.ToLower

'				Dim Table = Db.GetCollection(Of Model.Client)
'				Dim c = Table.FindOne(Function(x) x.Name.Equals(Name))
'				If c IsNot Nothing AndAlso c.Role <> Super Then
'					If Me.Role(client.Role) Is Nothing Then
'						client.Role = ""
'					Else
'						client.Role = client.Role.ToLower
'					End If

'					c.Role = client.Role
'					c.TimeStart = client.TimeStart
'					c.TimeStop = client.TimeStop
'					c.Open = client.Open
'					c.CloseMessage = client.CloseMessage
'					c.Version = client.Version
'					c.Config = client.Config

'					c.Update = Timer.Now

'					Return Table.Update(c)
'				End If
'			End If

'			Return False
'		End Function

'		''' <summary>更新项目，管理员不能修改权限</summary>
'		Public Function Update(Name As String, Optional Role As String = "", Optional Open As Boolean = True, Optional TimeStart As Date? = Nothing, Optional TimeStop As Date? = Nothing, Optional CloseMessage As String = "", Optional Version As Single = 0) As Boolean
'			If Core.String.Validate.UserName(Name) Then
'				Name = Name.ToLower

'				Dim Table = Db.GetCollection(Of Model.Client)
'				Dim c = Table.FindOne(Function(x) x.Name.Equals(Name))
'				If c IsNot Nothing AndAlso c.Role <> Super Then
'					If Me.Role(Role) Is Nothing Then
'						Role = ""
'					Else
'						Role = Role.ToLower
'					End If

'					c.Role = Role
'					c.TimeStart = TimeStart
'					c.TimeStop = TimeStop
'					c.Open = Open
'					c.CloseMessage = CloseMessage
'					c.Version = Version

'					c.Update = Timer.Now

'					Return Table.Update(c)
'				End If
'			End If

'			Return False
'		End Function

'		''' <summary>更新密匙</summary>
'		Public Function UpdateKey(Name As String) As String
'			If Core.String.Validate.UserName(Name) Then
'				Name = Name.ToLower

'				Dim Table = Db.GetCollection(Of Model.Client)
'				Dim c = Table.FindOne(Function(x) x.Name.Equals(Name))
'				If c IsNot Nothing Then
'					c.Key = Guid.NewGuid.ToString
'					If Table.Update(c) Then Return c.Key
'				End If
'			End If

'			Return ""
'		End Function

'		''' <summary>删除</summary>
'		Public Function Remove(Name As String) As Boolean
'			If Core.String.Validate.UserName(Name) Then
'				Name = Name.ToLower

'				Dim Table = Db.GetCollection(Of Model.Client)
'				Dim C = Table.DeleteMany(Function(x) x.Name.Equals(Name))
'				Return C > 0
'			End If

'			Return False
'		End Function

'#End Region

'#Region "权限"

'		''' <summary>所有权限</summary>
'		Public Function Roles(Optional Name As String = "") As Dictionary(Of String, String())
'			Dim Table = Db.GetCollection(Of Model.Role)

'			Name = Core.String.Clear.Trim(Name)
'			If String.IsNullOrWhiteSpace(Name) Then
'				Return Table.FindAll.ToDictionary(Function(x) x.Name, Function(x) x.APIs)
'			Else
'				Return Table.Find(Function(x) x.Name.Contains(Name, StringComparison.OrdinalIgnoreCase)).ToDictionary(Function(x) x.Name, Function(x) x.APIs)
'			End If
'		End Function

'		''' <summary>指定权限</summary>
'		Public Function Role(Name As String) As Model.Role
'			If Core.String.Validate.UserName(Name) Then
'				Name = Name.ToLower

'				Dim Table = Db.GetCollection(Of Model.Role)
'				Return Table.FindOne(Function(x) x.Name.Equals(Name))
'			End If

'			Return Nothing
'		End Function

'		''' <summary>添加权限</summary>
'		Public Function InsertRole(Name As String, ParamArray Apis() As String) As (Result As Boolean, Message As String)
'			Dim Message = ""

'			Apis = Apis?.Where(Function(x) Not String.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray
'			If Apis?.Length > 0 Then
'				If Core.String.Validate.UserName(Name) Then
'					Name = Name.ToLower

'					Dim Table = Db.GetCollection(Of Model.Role)
'					If Table.Exists(Function(x) x.Name.Equals(Name)) Then
'						Message = "此名称已经存在"
'					Else
'						Dim R As New Model.Role With {
'							.Name = Name,
'							.APIs = Apis,
'							.Created = Timer.Now,
'							.Update = Timer.Now
'						}

'						Table.Insert(R)

'						Call ReloadAPIs()
'					End If
'				Else
'					Message = "名称只能使用字母和数字，且必须字母开头，最少3个，最多24个"
'				End If
'			Else
'				Message = "未设置有效 API 地址列表"
'			End If

'			Return (String.IsNullOrEmpty(Message), Message)
'		End Function

'		''' <summary>更新权限</summary>
'		Public Function UpdateRole(Name As String, ParamArray Apis() As String) As Boolean
'			Apis = Apis?.Where(Function(x) Not String.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray

'			If Core.String.Validate.UserName(Name) AndAlso Apis?.Length > 0 Then
'				Name = Name.ToLower

'				Dim Table = Db.GetCollection(Of Model.Role)
'				Dim c = Table.FindOne(Function(x) x.Name.Equals(Name))
'				If c IsNot Nothing Then
'					c.APIs = Apis
'					c.Update = Timer.Now

'					If Table.Update(c) Then
'						Call ReloadAPIs()
'						Return True
'					End If
'				End If
'			End If

'			Return False
'		End Function

'		''' <summary>删除权限</summary>
'		Public Function RemoveRole(Name As String) As Boolean
'			If Core.String.Validate.UserName(Name) Then
'				Name = Name.ToLower

'				Dim Table = Db.GetCollection(Of Model.Role)
'				Dim C = Table.DeleteMany(Function(x) x.Name.Equals(Name))

'				If C > 0 Then
'					Call ReloadAPIs()
'					Return True
'				End If
'			End If

'			Return False
'		End Function

'#End Region

'#Region "API"

'		''' <summary>授权全局过滤 API</summary>
'		Public Sub RegisterAPI(api As String)
'			api = Core.String.Clear.Trim(api)
'			If Not String.IsNullOrWhiteSpace(api) Then
'				SyncLock GloApis
'					If Not GloApis.Contains(api, StringComparer.OrdinalIgnoreCase) Then GloApis.Add(api)
'				End SyncLock
'			End If
'		End Sub

'		''' <summary>授权默认 API</summary>
'		Public Sub RegisterAPI(api As String, ParamArray Roles() As String)
'			api = Core.String.Clear.Trim(api)
'			If Not String.IsNullOrWhiteSpace(api) AndAlso Roles.Length > 0 Then
'				SyncLock DefApis
'					If Not DefApis.ContainsKey(api) Then DefApis.Add(api, New List(Of String))
'					Dim Rs = DefApis(api)
'					For Each R In Roles
'						R = Core.String.Clear.Trim(R)
'						If Not String.IsNullOrWhiteSpace(R) AndAlso Not Rs.Contains(R, StringComparer.OrdinalIgnoreCase) Then Rs.Add(R)
'					Next
'				End SyncLock
'			End If
'		End Sub

'		''' <summary>获取APIs 权限列表</summary>
'		Public Sub ReloadAPIs()
'			Apis.Clear()

'			' 加载默认 API
'			If DefApis.Count > 0 Then
'				For Each A In DefApis
'					Apis.TryAdd(A.Key, A.Value)
'				Next
'			End If

'			Dim Table = Db.GetCollection(Of Model.Role)
'			If Table.Count > 0 Then
'				For Each R In Table.FindAll
'					If R.APIs?.Length > 0 Then
'						For Each A In R.APIs
'							If Not String.IsNullOrWhiteSpace(A) Then
'								If Not Apis.ContainsKey(A) Then Apis.TryAdd(A, New List(Of String))
'								If Not Apis(A).Contains(R.Name, StringComparer.OrdinalIgnoreCase) Then Apis(A).Add(R.Name)
'							End If
'						Next
'					End If
'				Next
'			End If
'		End Sub

'		''' <summary>匹配地址</summary>
'		Private Function MatchAPI(url As String, search As String) As Boolean
'			Dim R = False

'			'处理星号
'			If (search.StartsWith("(") AndAlso search.EndsWith(")")) Or search.StartsWith("^") Then
'				R = Regex.IsMatch(url, search, RegexOptions.IgnoreCase)
'			ElseIf search.StartsWith("*") AndAlso search.EndsWith("*") Then
'				search = search.Substring(1, search.Length - 2)
'				If Not String.IsNullOrWhiteSpace(search) Then R = url.Contains(search, StringComparison.OrdinalIgnoreCase)
'			ElseIf search.StartsWith("*") Then
'				search = search.Substring(1)
'				If Not String.IsNullOrWhiteSpace(search) Then R = url.EndsWith(search, StringComparison.OrdinalIgnoreCase)
'			ElseIf search.EndsWith("*") Then
'				search = search.Substring(0, search.Length - 1)
'				If Not String.IsNullOrWhiteSpace(search) Then R = url.StartsWith(search, StringComparison.OrdinalIgnoreCase)
'			Else
'				R = (url.ToLower = search.ToLower)
'			End If

'			Return R
'		End Function

'		'''' <summary>匹配请求</summary>
'		'Public Function MatchAPI(request As HttpRequest) As (Result As Boolean, Roles As List(Of String))
'		'	Dim Result = False
'		'	Dim Roles As New List(Of String)

'		'	If Apis.Count > 0 Then
'		'		For Each U In Apis.Keys
'		'			If MatchAPI(request.GetSourceBaseUrl, U) Then
'		'				Dim Rs = Apis(U)
'		'				If Rs.Count > 0 Then
'		'					For Each R In Rs
'		'						If Not Roles.Contains(R, StringComparer.OrdinalIgnoreCase) Then Roles.Add(R)
'		'					Next
'		'				End If
'		'			End If
'		'		Next

'		'		Result = (Roles.Count > 0)
'		'	End If

'		'	Return (Result, Roles)
'		'End Function


'		''' <summary>匹配请求</summary>
'		Public Function MatchAPI(request As HttpRequest) As List(Of String)
'			Dim Roles As New List(Of String)

'			' 全局过滤不检查网址
'			' 如果包含HOST则需要校验主机头
'			' HOST|URL

'			Dim List = MatchAPI(request.GetSourceBaseUrl)
'			If List.Count > 0 Then
'				For Each Rs In List.Values
'					For Each R In Rs
'						If Not Roles.Contains(R, StringComparer.OrdinalIgnoreCase) Then Roles.Add(R)
'					Next
'				Next
'			End If

'			Return Roles
'		End Function

'		''' <summary>匹配 API 列表</summary>
'		Public Function MatchAPI(Url As String) As Dictionary(Of String, List(Of String))
'			Dim Result As New Dictionary(Of String, List(Of String))

'			Url = Core.String.Clear.Trim(Url)
'			If Apis.Count > 0 AndAlso Not String.IsNullOrWhiteSpace(Url) Then
'				' 默认需要过滤的网址
'				If Not GloApis.Any(Function(x) Url.StartsWith(x, StringComparison.OrdinalIgnoreCase)) Then

'					' 匹配检验
'					For Each U In Apis.Keys
'						If MatchAPI(Url, U) Then
'							Dim Rs = Apis(U)
'							If Rs.Count > 0 Then
'								Result.Add(U, Rs)
'							End If
'						End If
'					Next
'				End If
'			End If

'			Return Result
'		End Function

'		''' <summary>查询 API 列表</summary>
'		Public Function SearchAPI(api As String) As Dictionary(Of String, List(Of String))
'			api = Core.String.Clear.Trim(api)

'			If Apis.Count > 0 AndAlso Not String.IsNullOrWhiteSpace(api) Then
'				Return Apis.Where(Function(x) x.Key.Contains(api, StringComparison.OrdinalIgnoreCase)).ToDictionary(Function(x) x.Key, Function(x) x.Value)
'			Else
'				Return Nothing
'			End If
'		End Function

'#End Region

'		''' <summary>当前登录用户</summary>
'		Public Function Current(ByVal Request As HttpRequest) As (Name As String, Role As String, IsSuper As Boolean)
'			Dim C = JWT.GetUserInfo(Request)
'			If C IsNot Nothing Then
'				Return (C.Name, C.Role, C.Role = Super)
'			Else
'				Return ("", "", False)
'			End If
'		End Function

'	End Class
'End Namespace

#End Region