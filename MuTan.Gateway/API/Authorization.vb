'--------------------------------------------------   
'   
'   授权相关操作
'   
'   namespace: API.Authorization
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 授权相关操作
'   release: 2020-08-19
'   
'-------------------------------------------------- 

Imports BeetleX
Imports BeetleX.FastHttpApi
Imports Bumblebee.Events

Namespace API

	''' <summary>授权相关操作</summary>
	<Controller(BaseUrl:="_gateway", SkipPublicFilter:=True)>
	<Options(AllowOrigin:="*", AllowHeaders:="*", AllowMethods:="OPTIONS,GET,POST", AllowMaxAge:="86400")>
	Public Class Authorization

		''' <summary>网关对象</summary>
		Private ReadOnly Gateway As Bumblebee.Gateway

		Public Sub New(Gateway As Bumblebee.Gateway)
			Me.Gateway = Gateway
		End Sub

		''' <summary>登录</summary>
		Public Function Login(ByVal name As String, ByVal key As String, ByVal cookie As Boolean, ByVal context As IHttpContext) As Object
			Dim c = xDatabase.ClientItem(name)
			If c Is Nothing OrElse c.Key <> key Then
				Gateway.Pluginer.Requested(New EventRequestCompletedArgs(Nothing, context.Request, context.Response, Gateway, 400, Nothing, TimeWatch.GetTotalMilliseconds - context.Request.RequestTime, context.Request.ID, "账号、密码不匹配！"))
				Return New BadRequestResult("账号、密码不匹配！")
			Else
				Dim Token As String
				If cookie Then
					Token = xJWT.CreateToken(context.Response, c.Name, c.Role)
				Else
					Token = xJWT.CreateToken(c.Name, c.Role)
				End If

				Gateway.Pluginer.Requested(New EventRequestCompletedArgs(Nothing, context.Request, context.Response, Gateway, 200, Nothing, TimeWatch.GetTotalMilliseconds - context.Request.RequestTime, context.Request.ID, Nothing))
				Return New With {Token, c.Name}
			End If
		End Function

		''' <summary>注销</summary>
		Public Sub Logout(ByVal context As IHttpContext)
			xJWT.ClearToken(context.Response)
			Gateway.Pluginer.Requested(New EventRequestCompletedArgs(Nothing, context.Request, context.Response, Gateway, 200, Nothing, TimeWatch.GetTotalMilliseconds - context.Request.RequestTime, context.Request.ID, Nothing))
		End Sub

		''' <summary>刷新</summary>
		Public Function Refresh(ByVal cookie As Boolean, ByVal context As IHttpContext) As Object
			Dim UI = xJWT.GetUserInfo(context.Request)
			If UI Is Nothing Then
				Gateway.Pluginer.Requested(New EventRequestCompletedArgs(Nothing, context.Request, context.Response, Gateway, 403, Nothing, TimeWatch.GetTotalMilliseconds - context.Request.RequestTime, context.Request.ID, "授权 Token 无效！"))
				Return New NotSupportResult("授权 Token 无效！")
			Else
				' 刷新 token
				Dim c = xDatabase.ClientItem(UI.Name)
				If c IsNot Nothing Then
					Dim Token As String
					If cookie Then
						Token = xJWT.CreateToken(context.Response, c.Name, c.Role)
					Else
						Token = xJWT.CreateToken(c.Name, c.Role)
					End If

					Gateway.Pluginer.Requested(New EventRequestCompletedArgs(Nothing, context.Request, context.Response, Gateway, 200, Nothing, TimeWatch.GetTotalMilliseconds - context.Request.RequestTime, context.Request.ID, Nothing))
					Return New With {Token, c.Name}
				Else
					Gateway.Pluginer.Requested(New EventRequestCompletedArgs(Nothing, context.Request, context.Response, Gateway, 400, Nothing, TimeWatch.GetTotalMilliseconds - context.Request.RequestTime, context.Request.ID, "授权对象无效！"))
					Return New BadRequestResult("授权对象无效！")
				End If
			End If
		End Function

		''' <summary>是否超级管理员</summary>
		Public Function Super(ByVal context As IHttpContext) As Boolean
			Return xJWT.Current(context.Request).IsSuper
		End Function

#Region "Rule"

		''' <summary>匹配地址测试</summary>
		<[Post](Route:="rule/match")>
		Public Function RuleMatch(url As String) As Dictionary(Of String, List(Of String))
			Return xAuthorization.Match(url)
		End Function

		''' <summary>搜索地址测试</summary>
		<[Post](Route:="rule/search")>
		Public Function RuleSearch(url As String) As Dictionary(Of String, List(Of String))
			Return xAuthorization.Search(url)
		End Function

		''' <summary>无需权限 API 列表</summary>
		<[Get](Route:="rule/ignores")>
		Public Function RuleIgnores() As String()
			Return xAuthorization.IgnoreRules
		End Function

		''' <summary>本人有效规则</summary>
		<[Get](Route:="rule/authorization")>
		Public Function Rules(ByVal context As IHttpContext) As String()
			Dim Current = xJWT.Current(context)
			If Current.IsSuper Then
				' 超级管理员
				Return {"*"}
			Else
				Return xDatabase.RoleItem(Current.Role)?.APIs
			End If

			Return Nothing
		End Function

#End Region


	End Class

End Namespace