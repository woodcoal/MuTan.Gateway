'--------------------------------------------------   
'   
'   匹配规则
'   
'   namespace: Model.Match.Rule
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 匹配规则
'   release: 2020-08-18
'   
'-------------------------------------------------- 

Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports BeetleX.FastHttpApi

Namespace Model.Match

	''' <summary>匹配规则</summary>
	''' <remarks>
	''' url
	''' host@url|d:data|h:head
	''' </remarks>
	Public Class Rule

		''' <summary>需要匹配的主机</summary>
		Public ReadOnly Host As String

		''' <summary>需要匹配的路径</summary>
		Public ReadOnly Path As String

		''' <summary>需要匹配的头部</summary>
		Public ReadOnly Header As String

		''' <summary>需要匹配的请求数据</summary>
		Public ReadOnly Data As String

		''' <summary>需要匹配的 Cookies</summary>
		Public ReadOnly Cookies As String

		''' <summary>原始规则字符串</summary>
		Public ReadOnly OriginalString As String

		''' <summary>是否空规则</summary>
		Public ReadOnly Empty As Boolean

		Public Sub New(Rule As String)
			Me.OriginalString = ""
			Me.Host = ""
			Me.Path = ""
			Me.Header = ""
			Me.Data = ""
			Me.Cookies = ""

			If Not String.IsNullOrWhiteSpace(Rule) Then
				Rule = Rule.Trim.ToLower

				' 分离 Host
				Dim P = Rule.IndexOf("@")
				If P > -1 Then
					Me.Host = Rule.Substring(0, P)
					Rule = Rule.Substring(P + 1)
				End If

				If Not String.IsNullOrEmpty(Rule) Then
					Rule = "@" & Rule.Trim & "|e:"

					Dim Ms = Regex.Match(Rule, "^@(.+?)\|[h|c|d|e]\:", RegexOptions.IgnoreCase)
					If Ms?.Length > 0 Then Me.Path = Ms.Groups(1).Value

					Ms = Regex.Match(Rule, "\|h\:(.+?)\|[c|d|e]\:", RegexOptions.IgnoreCase)
					If Ms?.Length > 0 Then Me.Header = Ms.Groups(1).Value

					Ms = Regex.Match(Rule, "\|d\:(.+?)\|[h|c|e]\:", RegexOptions.IgnoreCase)
					If Ms?.Length > 0 Then Me.Data = Ms.Groups(1).Value

					Ms = Regex.Match(Rule, "\|c\:(.+?)\|[h|d|e]\:", RegexOptions.IgnoreCase)
					If Ms?.Length > 0 Then Me.Cookies = Ms.Groups(1).Value
				End If

				If Not String.IsNullOrEmpty(Me.Host) Then Me.OriginalString = Me.Host & "@"
				If Not String.IsNullOrEmpty(Me.Path) Then Me.OriginalString &= Me.Path
				If Not String.IsNullOrEmpty(Me.Header) Then Me.OriginalString &= "|h:" & Me.Header
				If Not String.IsNullOrEmpty(Me.Data) Then Me.OriginalString &= "|d:" & Me.Data
				If Not String.IsNullOrEmpty(Me.Cookies) Then Me.OriginalString &= "|c:" & Me.Cookies
			End If

			Me.Empty = String.IsNullOrEmpty(Me.OriginalString)
		End Sub

		''' <summary>匹配请求</summary>
		Public Function Match(request As HttpRequest) As Boolean
			If request IsNot Nothing Then
				Return Empty OrElse Match(request.Host, request.GetSourceBaseUrl, request.Data.ToString, request.Header.ToString)
			Else
				Return False
			End If
		End Function

		''' <summary>匹配网址</summary>
		Public Function Match(mUrl As String) As Boolean
			If Not String.IsNullOrEmpty(mUrl) Then
				If Empty Then Return True

				Try
					Dim URI As New Uri(mUrl)
					Return Match(URI.Host, URI.AbsolutePath, URI.Query)
				Catch ex As Exception
				End Try
			End If

			Return False
		End Function

		''' <summary>匹配参数</summary>
		Public Function Match(Optional mHost As String = "", Optional mPath As String = "", Optional mData As String = "", Optional mHeader As String = "") As Boolean
			' 无规则直接返回成功
			If Empty Then Return True

			' 匹配 Host
			If Not String.IsNullOrEmpty(mHost) AndAlso Not String.IsNullOrEmpty(Host) Then
				If Not Regex.IsMatch(mHost, Host, RegexOptions.IgnoreCase) Then Return False
			End If

			' 匹配 Path
			If Not String.IsNullOrEmpty(mPath) AndAlso Not String.IsNullOrEmpty(Path) Then
				If Not Regex.IsMatch(mPath, Path, RegexOptions.IgnoreCase) Then Return False
			End If

			' 匹配 Data
			If Not String.IsNullOrEmpty(mData) AndAlso Not String.IsNullOrEmpty(Data) Then
				If Not Regex.IsMatch(mData, Data, RegexOptions.IgnoreCase) Then Return False
			End If

			' 匹配 Header
			If Not String.IsNullOrEmpty(mHeader) AndAlso Not String.IsNullOrEmpty(Header) Then
				If Not Regex.IsMatch(mHeader, Header, RegexOptions.IgnoreCase) Then Return False
			End If

			Return True
		End Function
		''' <summary>根据请求生成唯一KEY</summary>
		Public Shared Function MakeKey(request As HttpRequest, Optional Headers As IEnumerable(Of String) = Nothing, Optional Datas As IEnumerable(Of String) = Nothing, Optional Cookies As IEnumerable(Of String) = Nothing, Optional IsHash As Boolean = False) As String
			Dim R = ""

			If request IsNot Nothing Then
				With New Text.StringBuilder
					.AppendLine(request.GetHostBase)
					.AppendLine(request.GetSourceBaseUrl)

					If request.Header.Count > 0 AndAlso Headers?.Count Then
						For Each item In Headers
							Dim value = request.Header(item)
							If Not String.IsNullOrWhiteSpace(value) Then .AppendLine("H_" & item & ":" & value)
						Next
					End If

					If Datas?.Count Then
						For Each item In Datas
							Dim value = request.Data(item)
							If Not String.IsNullOrWhiteSpace(value) Then .AppendLine("D_" & item & ":" & value)
						Next
					End If

					If Cookies?.Count Then
						For Each item In Cookies
							Dim value = request.Cookies(item)
							If Not String.IsNullOrWhiteSpace(value) Then .AppendLine("C_" & item & ":" & value)
						Next
					End If


					R = .ToString.ToLower

					If IsHash Then
						Using Hash = MD5.Create
							Dim Res = Hash.ComputeHash(Encoding.UTF8.GetBytes(R))
							R = BitConverter.ToString(Res).Replace("-", "")
						End Using
					End If
				End With
			End If

			Return R
		End Function

	End Class

End Namespace
