'--------------------------------------------------   
'   
'   插件管理   
'   
'   namespace: API.Plugin
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 插件管理
'   release: 2020-07-22
'   
'-------------------------------------------------- 

Imports BeetleX.FastHttpApi
Imports Bumblebee.Plugins
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace API

	''' <summary>服务器状态</summary>
	<Controller(BaseUrl:="_gateway/plugin", SkipPublicFilter:=True)>
	<Options(AllowOrigin:="*", AllowHeaders:="*", AllowMethods:="OPTIONS,GET,POST", AllowMaxAge:="86400")>
	Public Class Plugin

		''' <summary>网关对象</summary>
		Private ReadOnly PluginCenter As Bumblebee.Plugins.PluginCenter

		Public Sub New(Gateway As Bumblebee.Gateway)
			Me.PluginCenter = Gateway.PluginCenter
		End Sub

		Public Function Switch(name As String, enabled As Boolean) As Boolean
			If Not String.IsNullOrWhiteSpace(name) Then
				Try
					Dim plugin = PluginCenter.GetPlugin(name)
					If plugin IsNot Nothing Then
						PluginCenter.SetPluginEnabled(plugin, enabled)
						Return True
					End If
				Catch ex As Exception
				End Try
			End If

			Return False
		End Function

		<Post()>
		Public Function Save(name As String, setting As JToken) As Boolean
			If Not String.IsNullOrWhiteSpace(name) Then
				Try
					Dim plugin = PluginCenter.GetPlugin(name)
					If plugin IsNot Nothing Then
						plugin.LoadSetting(setting)
						PluginCenter.SaveSetting(plugin, True)
					End If

					Return True
				Catch ex As Exception
				End Try
			End If

			Return False
		End Function

		Public Function Info(name As String) As Model.Plugin
			If Not String.IsNullOrWhiteSpace(name) Then
				Return New Model.Plugin(PluginCenter.GetPlugin(name))
			Else
				Return Nothing
			End If
		End Function

		Public Function List() As List(Of PluginInfo)
			Return PluginCenter.ListPluginInfos.OrderBy(Function(x) x.Name).ToList
		End Function

	End Class

End Namespace