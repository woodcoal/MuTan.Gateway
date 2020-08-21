'--------------------------------------------------   
'   
'   JWT 操作
'   
'   namespace: JWT.Instance
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: JWT 操作
'   release: 2020-07-27
'   
'-------------------------------------------------- 

Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports BeetleX.EventArgs
Imports BeetleX.FastHttpApi
Imports Microsoft.IdentityModel.Tokens

Namespace Utils
	''' <summary>JWT 操作</summary>
	Friend Class JWT

		' Authorization: Bearer <token>
		' Cookies: token=

		Public Class UserInfo
			Public Name As String
			Public Role As String
		End Class

		Private ReadOnly Issuer As String
		Private ReadOnly Audience As String

		Private ReadOnly SigningCredentials As SigningCredentials
		Private ReadOnly TokenValidation As TokenValidationParameters
		Private ReadOnly JwtSecurityTokenHandler As JwtSecurityTokenHandler

		Public Sub New(Issuer As String, Audience As String, key As String)
			Me.Issuer = Issuer
			Me.Audience = Audience

			Me.TokenValidation = New TokenValidationParameters
			Me.JwtSecurityTokenHandler = New JwtSecurityTokenHandler

			If String.IsNullOrEmpty(Me.Issuer) Then
				Me.TokenValidation.ValidateIssuer = False
			Else
				Me.TokenValidation.ValidIssuer = Me.Issuer
			End If

			If String.IsNullOrEmpty(Me.Audience) Then
				Me.TokenValidation.ValidateAudience = False
			Else
				Me.TokenValidation.ValidAudience = Me.Audience
			End If

			If Not String.IsNullOrWhiteSpace(key) Then key = Guid.NewGuid.ToString
			Dim SecurityKey = New SymmetricSecurityKey(Text.Encoding.UTF8.GetBytes(key))
			Me.TokenValidation.IssuerSigningKey = SecurityKey
			Me.SigningCredentials = New SigningCredentials(SecurityKey, "HS256")
		End Sub

		''' <summary>清除 Token，仅针对 Cookies</summary>
		Public Sub ClearToken(response As HttpResponse)
			response.SetCookie(xConfig.TOKEN_NAME, "", "/", Date.Now)
		End Sub

		''' <summary>生成 Token，仅针对 Cookies</summary>
		Public Function CreateToken(response As HttpResponse, name As String, role As String, Optional timeout As Integer = 60) As String
			If timeout < 1 Then timeout = 1

			Dim token = CreateToken(name, role, timeout)
			response.SetCookie(xConfig.TOKEN_NAME, token, "/", Date.Now.AddMinutes(timeout))
			Return token
		End Function

		''' <summary>生成 Token</summary>
		Public Function CreateToken(name As String, role As String, Optional timeout As Integer = 60) As String
			If timeout < 1 Then timeout = 1

			Dim claimsIdentity As New ClaimsIdentity()
			claimsIdentity.AddClaim(New Claim("Name", name))
			claimsIdentity.AddClaim(New Claim("Role", role))

			Return Me.JwtSecurityTokenHandler.CreateEncodedJwt(Me.Issuer, Me.Audience, claimsIdentity, Date.Now.AddMinutes(-5), Date.Now.AddMinutes(timeout), Date.Now, Me.SigningCredentials)
		End Function

		''' <summary>获取 Token 参数，顺序获取：Header|Cookies:Token / Header|Cookies:Authorization(Bearer)</summary>
		Private Function GetToken(request As HttpRequest) As String
			Dim R = ""

			If request IsNot Nothing Then
				R = request.Header(xConfig.TOKEN_NAME)

				If String.IsNullOrWhiteSpace(R) Then
					R = request.Cookies(xConfig.TOKEN_NAME)

					If String.IsNullOrWhiteSpace(R) Then

						R = request.Header("Authorization")

						If String.IsNullOrWhiteSpace(R) Then
							R = request.Cookies("Authorization")

						End If
					End If
				End If

				If Not String.IsNullOrWhiteSpace(R) AndAlso R.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) Then R = R.Substring(7).Trim
			End If

			Return R
		End Function

		''' <summary>根据请求获取用户信息</summary>
		Public Function GetUserInfo(request As HttpRequest) As UserInfo
			Return Me.GetUserInfo(GetToken(request))
		End Function

		''' <summary>根据 Token 数据获取用户信息</summary>
		Public Function GetUserInfo(token As String) As UserInfo
			If Not String.IsNullOrEmpty(token) Then
				Dim validate = ValidateToken(token)
				If validate IsNot Nothing Then
					Dim claimsIdentity = TryCast(validate.Identity, ClaimsIdentity)
					If claimsIdentity?.Claims?.Count > 0 Then
						Return New UserInfo With {
							.Name = claimsIdentity.Claims.FirstOrDefault(Function(x) x.Type = "Name")?.Value,
							.Role = claimsIdentity.Claims.FirstOrDefault(Function(x) x.Type = "Role")?.Value
						}
					End If
				End If
			End If

			Return Nothing
		End Function

		Private Function ValidateToken(token As String) As ClaimsPrincipal
			Try
				Return Me.JwtSecurityTokenHandler.ValidateToken(token, Me.TokenValidation, Nothing)
			Catch ex As Exception
				Return Nothing
			End Try
		End Function

		''' <summary>当前登录用户</summary>
		Public Function Current(ByVal Request As HttpRequest) As (Name As String, Role As String, IsSuper As Boolean)
			Dim UI = GetUserInfo(Request)
			If UI IsNot Nothing Then
				Return (UI.Name, UI.Role, UI.Role = Super)
			Else
				Return ("", "", False)
			End If
		End Function
	End Class

End Namespace
