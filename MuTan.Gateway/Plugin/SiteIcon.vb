'--------------------------------------------------   
'   
'   站点图标   
'   
'   namespace: Plugin.SiteIcon
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 网站收藏夹图标，站点图标
'   release: 2020-07-20
'   
'-------------------------------------------------- 

Imports System.Reflection
Imports Bumblebee.Events
Imports Bumblebee.Plugins
Imports Newtonsoft.Json.Linq

Namespace Plugin
	<RouteBinder(RouteUrl:="^/favicon\.ico", ApiLoader:=False)>
	Public Class SiteIcon
		Implements IRequestingHandler, Model.IPlugin

		''' <summary>强制启用</summary>
		Public Property Enabled As Boolean = True

		''' <summary>名称</summary>
		Public ReadOnly Property Name As String = Me.GetType.Name Implements IPlugin.Name

		''' <summary>描述</summary>
		Public ReadOnly Property Description As String = "网站收藏夹图标，站点图标" Implements IPlugin.Description

		''' <summary>插件级别</summary>
		Public ReadOnly Property Level As PluginLevel = PluginLevel.None Implements IPlugin.Level

		''' <summary>是否需要设置参数</summary>
		Public ReadOnly Property EnSetting As Boolean = False Implements Model.IPlugin.EnSetting

		''' <summary>扩展展示信息</summary>
		Public ReadOnly Property Information As Dictionary(Of String, String) = Nothing Implements Model.IPlugin.Information

		''' <summary>执行操作</summary>
		Public Sub Execute(e As EventRequestingArgs) Implements IRequestingHandler.Execute
			e.Cancel = True
			e.ResultType = ResultType.None
		End Sub

		''' <summary>初始化</summary>
		Public Sub Init(gateway As Global.Bumblebee.Gateway, assembly As Assembly) Implements IPlugin.Init
		End Sub

		''' <summary>加载配置</summary>
		Public Sub LoadSetting(setting As JToken) Implements IPlugin.LoadSetting
		End Sub

		''' <summary>保存配置</summary>
		Public Function SaveSetting() As Object Implements IPlugin.SaveSetting
			Return Nothing
		End Function

	End Class
End Namespace