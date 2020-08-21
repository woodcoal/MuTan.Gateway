'--------------------------------------------------   
'   
'   插件模型
'   
'   namespace: Model.Plugin
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 插件模型
'   release: 2020-08-12
'   
'-------------------------------------------------- 

Imports Bumblebee.Plugins

Namespace Model
	''' <summary>路由数据模型</summary>
	Public Class Plugin
		Inherits PluginInfo

		Public Property Information As Dictionary(Of String, String)

		Public Property Setting As Object

		Public Property EnSetting As Boolean

		Public Sub New(Optional Plugin As Bumblebee.Plugins.IPlugin = Nothing)
			MyBase.New(Plugin)
			Me.EnSetting = True

			If Plugin IsNot Nothing Then
				Me.Setting = Plugin.SaveSetting

				' 获取是否需要编辑
				Dim Prop = Plugin.GetType.GetProperties.Where(Function(x) x.Name.ToLower = "ensetting").FirstOrDefault
				If Prop IsNot Nothing Then
					EnSetting = Prop.GetValue(Plugin)
				End If

				' 获取扩展信息
				Prop = Plugin.GetType.GetProperties.Where(Function(x) x.Name.ToLower = "information").FirstOrDefault
				If Prop IsNot Nothing Then
					Information = Prop.GetValue(Plugin)
				End If
			End If
		End Sub

	End Class
End Namespace