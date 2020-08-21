'--------------------------------------------------   
'   
'   缓存操作
'   
'   namespace: API.Cache
'   author: 木炭(WoodCoal)
'   homepage: http://www.woodcoal.cn/   
'   memo: 缓存操作
'   release: 2020-07-22
'   
'-------------------------------------------------- 

Imports BeetleX.FastHttpApi

Namespace API

	''' <summary>缓存操作</summary>
	<Controller(BaseUrl:="_gateway/cache", SkipPublicFilter:=True)>
	<Options(AllowOrigin:="*", AllowHeaders:="*", AllowMethods:="OPTIONS,GET,POST", AllowMaxAge:="86400")>
	Public Class Cache

		''' <summary>更新缓存</summary>
		Public Function Update(api As String, time As Integer) As Boolean
			Return xDatabase.CacheUpdate(api, time)
		End Function

		''' <summary>移除缓存</summary>
		Public Function Remove(api As String) As Boolean
			Return xDatabase.CacheRemove(api)
		End Function

		''' <summary>缓存列表</summary>
		Public Function List() As Dictionary(Of String, Integer)
			Return xDatabase.CacheList
		End Function

	End Class

End Namespace