'--------------------------------------------------   
'   
'   路由服务器数据模型
'   
'   namespace: Model.RouteServer
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 路由服务器数据模型
'   release: 2020-07-22
'   
'-------------------------------------------------- 

Namespace Model
    ''' <summary>路由服务器数据模型</summary>
    Public Class RouteServer
        Public Property Host As String
        Public Property Weight As Integer
        Public Property Available As Boolean
        Public Property MaxRps As Integer
        Public Property Standby As Boolean

        Public Sub New(Optional server As Bumblebee.Servers.UrlRouteServerGroup.UrlServerInfo = Nothing)
            If server IsNot Nothing Then
                With server
                    Host = .Agent.Uri.ToString
                    Weight = .Weight
                    Available = .Agent.Available
                    MaxRps = .MaxRPS
                    Standby = .Standby
                End With
            End If
        End Sub
    End Class
End Namespace
