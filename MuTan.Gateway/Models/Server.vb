'--------------------------------------------------   
'   
'   服务器数据模型
'   
'   namespace: Model.Server
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 服务器数据模型
'   release: 2020-07-22
'   
'-------------------------------------------------- 

Namespace Model
    ''' <summary>服务器数据模型</summary>
    Public Class Server
        Public Property Host As String
        Public Property MaxConnections As Integer
        Public Property Available As Boolean
        Public Property Category As String
        Public Property Command As String
        Public Property Remark As String

        Public Sub New(Optional server As Bumblebee.Servers.ServerAgent = Nothing)
            If server IsNot Nothing Then
                With server
                    Host = .Uri.ToString
                    MaxConnections = .MaxConnections
                    Available = .Available
                    Category = .Category
                    Command = .Command
                    Remark = .Remark
                End With
            End If
        End Sub

    End Class
End Namespace
