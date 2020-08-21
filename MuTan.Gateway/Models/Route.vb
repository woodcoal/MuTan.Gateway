'--------------------------------------------------   
'   
'   路由数据模型
'   
'   namespace: Model.Server
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 路由数据模型
'   release: 2020-07-22
'   
'-------------------------------------------------- 

Namespace Model
    ''' <summary>路由数据模型</summary>
    Public Class Route
        Public Property Url As String
        Public Property HashPattern As String
        Public Property Remark As String
        Public Property Editor As Boolean
        Public Property MaxRps As Integer
        Public Property TimeOut As Long
        Public Property Show As Boolean
        Public Property AccessControlAllowOrigin As String
        Public Property AccessControlAllowMethods As String = "*"
        Public Property AccessControlAllowHeaders As String
        Public Property AccessControlMaxAge As Integer
        Public Property Vary As String = "Origin"
        Public Property AccessControlAllowCredentials As Boolean
        Public Property Servers As RouteServer()

        Public Sub New(Optional Route As Bumblebee.Routes.UrlRoute = Nothing)
            If Route IsNot Nothing Then
                With Route
                    Url = .Url
                    HashPattern = .HashPattern
                    Remark = .Remark
                    Editor = True
                    MaxRps = .MaxRps
                    TimeOut = .TimeOut
                    Show = True
                    AccessControlAllowOrigin = .AccessControlAllowOrigin
                    AccessControlAllowMethods = .AccessControlAllowMethods
                    AccessControlAllowHeaders = .AccessControlAllowHeaders
                    AccessControlMaxAge = .AccessControlMaxAge
                    Vary = .Vary
                    AccessControlAllowCredentials = .AccessControlAllowCredentials
                    Servers = Route.Servers?.Select(Function(x) New Model.RouteServer(x)).ToArray
                End With
            End If
        End Sub

    End Class
End Namespace