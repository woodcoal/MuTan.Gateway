'--------------------------------------------------   
'   
'   授权验证
'   
'   namespace: Utils.Authorization
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 授权验证
'   release: 2020-08-18
'   
'-------------------------------------------------- 

Imports BeetleX.FastHttpApi
Imports MuTan.Gateway.Model.Cache

Namespace Utils
	''' <summary>授权验证</summary>
	Friend Class Authorization

		''' <summary>用于匹配的API列表</summary>
		Private ReadOnly Validates As List(Of Model.Match.Data(Of List(Of String)))

		''' <summary>用于忽略匹配的API列表</summary>
		Private ReadOnly Ignores As List(Of Model.Match.Rule)

		''' <summary>默认全局添加的API列表</summary>
		Private ReadOnly Defaults As Dictionary(Of String, String())

		''' <summary>是否存在规则</summary>
		Public Empty As Boolean

		Public Sub New()
			Me.Defaults = New Dictionary(Of String, String())(StringComparer.OrdinalIgnoreCase)
			Me.Validates = New List(Of Model.Match.Data(Of List(Of String)))
			Me.Ignores = New List(Of Model.Match.Rule)

			' 注册需要管理员权限 API
			Register("^/_gateway/login")
			Register("^/_gateway/logout")
			Register("^/_gateway/refresh")
			Register("^/_gateway/rule/authorization")
			Register("^/_gateway/status/")

			Register("^/_gateway/", Super)

			' 加载规则列表
			Reload()
		End Sub

#Region "API"

		''' <summary>注册全局过滤规则</summary>
		Public Sub Register(Rule As String)
			Dim matchRule As New Model.Match.Rule(Rule)
			If Not matchRule.Empty Then
				SyncLock Ignores
					If Not Ignores.Any(Function(x) x.OriginalString = matchRule.OriginalString) Then
						Ignores.Add(matchRule)
					End If
				End SyncLock
			End If
		End Sub

		''' <summary>注册默认规则</summary>
		Public Sub Register(Rule As String, ParamArray Roles() As String)
			Dim matchRule As New Model.Match.Rule(Rule)
			If Not matchRule.Empty Then
				If Roles?.Length > 0 Then
					SyncLock Defaults
						If Not Defaults.Any(Function(x) x.Key = matchRule.OriginalString) Then
							Defaults.Add(matchRule.OriginalString, Roles)
						End If
					End SyncLock
				End If
			End If
		End Sub

		''' <summary>重载规则列表</summary>
		Public Sub Reload()
			Validates.Clear()

			' 加载默认规则
			If Defaults.Count > 0 Then
				For Each Rule In Defaults
					InsertValidate(Rule.Key, Rule.Value)
				Next
			End If

			' 加载系统数据
			Dim Roles = xDatabase.RoleList
			If Roles?.Count > 0 Then
				For Each Role In Roles
					If Role.Value?.Length > 0 Then
						For Each ruleAPI In Role.Value
							InsertValidate(ruleAPI, Role.Key)
						Next
					End If
				Next
			End If

			Empty = (Validates.Count < 1)
		End Sub

		''' <summary>添加规则</summary>
		Private Sub InsertValidate(Rule As String, ParamArray Roles As String())
			Dim matchRule As New Model.Match.Data(Of List(Of String))(Rule, New List(Of String))
			If Not matchRule.Empty Then
				If Roles?.Length > 0 Then
					SyncLock Validates
						Dim existRule = Validates.FirstOrDefault(Function(x) x.OriginalString = matchRule.OriginalString)
						If existRule Is Nothing Then
							' 不存在，添加
							Validates.Add(matchRule)
						Else
							' 存在，更新结果
							matchRule = existRule
						End If

						' 更新权限
						For Each Role In Roles
							If Not matchRule.Value.Contains(Role, StringComparer.OrdinalIgnoreCase) Then
								matchRule.Value.Add(Role)
							End If
						Next
					End SyncLock
				End If
			End If
		End Sub

		''' <summary>匹配请求</summary>
		Public Function Match(request As HttpRequest) As List(Of String)
			Dim Result As New List(Of String)

			' 是否存在忽略项目，存在直接返回空值
			If Ignores.Count > 0 Then
				For Each Rule In Ignores
					If Rule.Match(request) Then
						Return Result
					End If
				Next
			End If

			' 没有忽略项目，匹配校验项目
			If Validates.Count > 0 Then
				For Each Rule In Validates
					Dim ret = Rule.GetResult(request)
					If ret?.Count > 0 Then
						For Each v In ret
							If Not Result.Contains(v, StringComparer.OrdinalIgnoreCase) Then Result.Add(v)
						Next
					End If
				Next
			End If

			Return Result
		End Function

		''' <summary>匹配请求</summary>
		Public Function Match(Url As String) As Dictionary(Of String, List(Of String))
			Dim Result As New Dictionary(Of String, List(Of String))

			' 地址不存在，返回默认空值
			If String.IsNullOrWhiteSpace(Url) Then Return Result

			' 地址不包含 http 则自行加上构造网址
			If Not Url.StartsWith("http", StringComparison.OrdinalIgnoreCase) Then Url = "http://" & Url

			' 是否存在忽略项目，存在直接返回空值
			If Ignores.Count > 0 Then
				For Each Rule In Ignores
					If Rule.Match(Url) Then
						Return Result
					End If
				Next
			End If

			' 没有忽略项目，匹配校验项目
			If Validates.Count > 0 Then
				For Each Rule In Validates
					Dim ret = Rule.GetResult(Url)
					If ret?.Count > 0 Then
						For Each v In ret
							Result.Add(Rule.OriginalString, ret)
						Next
					End If
				Next
			End If

			Return Result
		End Function

		''' <summary>查询 API 列表</summary>
		Public Function Search(Rule As String) As Dictionary(Of String, List(Of String))
			If Not String.IsNullOrWhiteSpace(Rule) AndAlso Validates.Count > 0 Then
				Rule = Rule.Trim.ToLower
				Return Validates.Where(Function(x) x.OriginalString.Contains(Rule, StringComparison.OrdinalIgnoreCase)).ToDictionary(Function(x) x.OriginalString, Function(x) x.Value)
			Else
				Return Nothing
			End If
		End Function

		''' <summary>忽略 API 列表</summary>
		Public Function IgnoreRules() As String()
			Return Ignores.Select(Function(x) x.OriginalString).ToArray
		End Function

#End Region

	End Class
End Namespace
