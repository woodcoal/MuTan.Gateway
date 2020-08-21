'--------------------------------------------------   
'   
'   LRU 缓存
'   
'   namespace: Utils.LRU
'   author: 木炭(WoodCoal)   
'   homepage: http://www.woodcoal.cn/   
'   memo: LRU 缓存
'   release: 2020-08-14
'   
'-------------------------------------------------- 

Namespace Utils

	''' <summary>LRU 缓存</summary>
	Public Class LRU(Of K, V)

		Public ReadOnly Instance As Concurrent.ConcurrentDictionary(Of K, Node)
		Private First As Node
		Private Last As Node

		Public Size As Integer

		Public Sub New(Optional Size As Integer = 100)
			Me.Instance = New Concurrent.ConcurrentDictionary(Of K, Node)
			Me.Size = Size
		End Sub

		''' <summary>添加节点</summary>
		Public Sub Put(Key As K, Value As V)
			If Size < 1 Then Exit Sub

			If Key IsNot Nothing Then
				If Instance.ContainsKey(Key) Then
					Dim Node As Node = Nothing
					If Instance.TryGetValue(Key, Node) Then
						Node.UpdateValue(Value)
						Update(Node)
					End If
				Else
					Dim Node As New Node(Key, Value)

					' 第一次添加数据，将赋值默认首尾参数 
					If Instance.Count < 1 Then
						First = Node
						Last = Node
					End If

					If Instance.TryAdd(Key, Node) Then Update(Node)
				End If
			End If
		End Sub

		Public Function [Get](Key As K) As V
			If Size < 1 Then Return Nothing

			If Key IsNot Nothing AndAlso Instance.ContainsKey(Key) Then
				Dim Node As Node = Nothing

				If Instance.TryGetValue(Key, Node) Then
					If Node IsNot Nothing Then
						Update(Node)
						Return Node.Value
					End If
				End If
			End If

			Return Nothing
		End Function

		Public Sub Clear()
			Instance.Clear()
		End Sub

		Public Sub Remove(Key As K)
			Instance.TryRemove(Key, Nothing)
		End Sub

		Public ReadOnly Property Count As Integer
			Get
				Return Instance.Count
			End Get
		End Property

		''' <summary>所有节点数据字典列表</summary>
		Public ReadOnly Property Dictionary As Dictionary(Of K, V)
			Get
				Return Instance.ToDictionary(Function(x) x.Value.Key, Function(x) x.Value.Value)
			End Get
		End Property

		''' <summary>所有节点数据列表</summary>
		Public ReadOnly Property List As List(Of Node)
			Get
				Return Instance.Select(Function(x) x.Value).ToList
			End Get
		End Property

		''' <summary>更新当前节点位置，移动到队首</summary>
		Private Sub Update(Node As Node)
			If Node Is Nothing Then Exit Sub

			' 只有一个数据的时候，无需调整
			If Instance.Count < 2 Then Exit Sub

			' 检查是否队首节点，队首则不处理 
			If Node.Key.Equals(First.Key) Then Exit Sub

			' 检查是否队末节点，队末则需要更新为新数据
			If Node.Key.Equals(Last.Key) Then
				Last = Last.Prv
				Last.Nxt = Nothing
			End If

			' 原位置前后对接
			If Node.Prv IsNot Nothing Then Node.Prv.Nxt = Node.Nxt
			If Node.Nxt IsNot Nothing Then Node.Nxt.Prv = Node.Prv

			' 移到队首
			Node.Prv = Nothing
			Node.Nxt = First
			First.Prv = Node

			' 获取队首
			First = Node
			' 检查对末数据
			If Instance.Count > Size Then
				' 移除最后一个
				If Instance.TryRemove(Last.Key, Nothing) Then
					' 获取新的对末数据
					Last = Last.Prv
					Last.Nxt = Nothing
				End If
			End If
		End Sub

		''' <summary>双向链接节点</summary>
		Public Class Node

			Public Prv As Node
			Public Nxt As Node

			Public ReadOnly Key As K
			Private _Value As V

			Public CountGet As Integer
			Public CountSet As Integer

			Public Sub New(Key As K, Value As V, Optional Prv As Node = Nothing, Optional Nxt As Node = Nothing)
				Me.CountGet = 0
				Me.CountSet = 0

				Me.Key = Key
				Me.UpdateValue(Value)
				Me.Prv = Prv
				Me.Nxt = Nxt
			End Sub

			''' <summary>获取访问次数</summary>
			Public ReadOnly Property Count As Integer
				Get
					Return CountGet + CountSet
				End Get
			End Property

			Public ReadOnly Property Value As V
				Get
					Threading.Interlocked.Increment(CountGet)
					Return _Value
				End Get
			End Property

			Public Sub UpdateValue(Value As V)
				Threading.Interlocked.Increment(CountSet)
				Me._Value = Value
			End Sub

		End Class



	End Class

End Namespace