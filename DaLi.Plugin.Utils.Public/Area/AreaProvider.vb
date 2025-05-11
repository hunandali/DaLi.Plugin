' ------------------------------------------------------------
'
' 	Copyright © 2022 湖南大沥网络科技有限公司.
' 	Dali.Framework Is licensed under Mulan PSL v2.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	i@woodcoal.cn
' 	homepage:	http://www.hunandali.com/
'
' 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
'
' ------------------------------------------------------------
'
' 	地区辅助功能
'
' 	name: AreaProvider
' 	create: 2023-01-29
' 	memo: 地区辅助功能
'
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports System.Net.Http
Imports System.Text.RegularExpressions
Imports DaLi.Plugin.Utils.ExchangeRate
Imports DaLi.Utils.App.Base
Imports DaLi.Utils.App.[Interface]
Imports DaLi.Utils.App.Model
Imports Microsoft.Extensions.Logging

Namespace Area

	''' <summary>地区辅助功能</summary>
	Public Class AreaProvider
		Inherits ProviderBase

		''' <summary>数据列表</summary>
		Private _Instance As ImmutableList(Of DataList) = ImmutableList.Create(Of DataList)

		''' <summary>锁</summary>
		Private ReadOnly _Lock As New Object

		Public Sub New(db As IDatabaseProvider, log As ILogger(Of AreaProvider))
			MyBase.New(db.GetDb(VAR_DATABASE_CONNECTION_EXTEND), log, E_PUBLIC_AREA_RELOAD)

			' 启动检查数据是否存在，不存在尝试初始化
			If Not MyBase.Db.Select(Of AreaEntity).Any Then UpdateAsync()
		End Sub

		''' <summary>加载数据</summary>
		Public Overrides Sub Reload()
			SyncLock _Lock
				Dim GetIndex = Function(pinyin As String) As String
								   If pinyin.IsEmpty Then Return ""

								   Dim chars = pinyin.Split(" ").Where(Function(x) x.NotEmpty).Select(Function(x) x.FirstOrDefault).ToArray
								   Return New String(chars)
							   End Function

				Dim datas = Db.Select(Of AreaEntity).
					Where(Function(x) x.Enabled).
					ToList.
					Select(Function(x) New DataList With {.Value = x.ID, .Text = x.Name, .Parent = x.ParentId, .Disabled = False, .Ext = (x.Level, x.PinYin, GetIndex(x.PinYin))}).
					ToList

				_Instance = datas.ToImmutableList
			End SyncLock

			Log.LogInformation("加载 {name} 数据 {count} 条", "地区", _Instance.Count)
		End Sub

		''' <summary>通过标识获取地区信息</summary>
		Public Function Item(id As Long) As DataTree
			If id < 1 Then Return Nothing

			Dim data = _Instance.Where(Function(x) x.Value = id).FirstOrDefault
			If data Is Nothing Then Return Nothing

			Return New DataTree(data) With {
				.Children = _Instance.Where(Function(x) x.Parent = data.Value).Select(Function(x) New DataTree(x)).ToList
			}
		End Function

		''' <summary>按层级获取地区列表</summary>
		Public Function List(Optional level As Integer = 2) As List(Of DataList)
			level = level.Range(1, 4)

			Return _Instance.Where(Function(x)
									   Dim ext As (Level As Byte, PinYin As String, Index As String) = x.Ext
									   Return ext.Level < level
								   End Function).ToList
		End Function

		''' <summary>搜索</summary>
		Public Function Search(keyword As String, Optional level As Integer = 2) As List(Of DataList)
			If keyword.IsEmpty Then Return Nothing

			' 检索层级
			level = level.Range(1, 4)
			Dim ret = _Instance.Where(Function(x)
										  Dim ext As (Level As Byte, PinYin As String, Index As String) = x.Ext
										  Return ext.Level < level
									  End Function).ToList

			If ret.Count < 1 Then Return Nothing

			' 检索指定层级后的数据
			If Regex.IsMatch(keyword, "^[a-zA-Z\s]+$") Then
				' 检索拼音
				Return ret.Where(Function(x)
									 Dim ext As (Level As Byte, PinYin As String, Index As String) = x.Ext
									 Return ext.PinYin.StartsWith(keyword, StringComparison.OrdinalIgnoreCase) OrElse ext.Index.StartsWith(keyword, StringComparison.OrdinalIgnoreCase)
								 End Function).ToList

			ElseIf Regex.IsMatch(keyword, "^[0-9]+$") Then
				' 检索标识
				Return ret.Where(Function(x) x.Value.StartsWith(keyword)).ToList

			Else
				' 检索名称
				Return ret.Where(Function(x) x.Text.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList
			End If
		End Function

		''' <summary>初始化地区数据</summary>
		Public Async Sub UpdateAsync()
			' 获取数据
			Dim Url = SYS.GetSetting(Of AreaSetting).Url
			Dim http = SYS.GetService(Of IHttpClientFactory).CreateClient
			Dim html = Await http.GetStringAsync(Url)
			If html.IsEmpty Then Return

			' 分析表格数据
			Dim table As String = html.Cut("<table", "</table>", False, True)
			Dim trs As String() = table.Cut("<tr", "</tr>", True, True)
			If trs.IsEmpty Then Return

			Dim Areas As New List(Of DataTree(Of Integer, Integer))
			For Each tr In trs
				' 每行组成，前6个字符为行政区划，后面为地区名称
				tr = tr.Replace(vbCr, "").Replace(vbLf, "").
					Replace(" ", "").Replace(" ", "").
					Replace("</td>", " ").
					ClearHtml("all", "trim").TrimFull

				If tr.Length < 8 Then Continue For

				' 前 6 位为数字则表示有效
				Dim code = tr.Substring(0, 6)
				If code.ToInteger.ToString <> code Then Continue For

				' 层级：1:省份、2:地级市、3:县级市
				Dim level = 3

				If code.EndsWith("0000") Then
					level = 1
				ElseIf code.EndsWith("00") Then
					level = 2
				End If

				' 地区名称
				Dim name = tr.Substring(7).TrimFull

				Areas.Add(New DataTree(Of Integer, Integer) With {.Value = code, .Text = name, .Ext = level})
			Next
			If Areas.IsEmpty Then Return

			' 分析上级
			' 对于直辖市，直辖市没有 level 2
			' 名称包含标记“*”的行政区划代码第三、四位90，表示省（自治区）直辖县级行政区划汇总码。即上级为省直属
			Areas.ForEach(Sub(area)
							  Select Case area.Ext
								  Case 1
									  ' 1 级：省，直辖市不处理

								  Case 2
									  ' 2 级：市级
									  area.Parent = Math.Floor(area.Value / 10000) * 10000

								  Case 3
									  ' 3 级：区县
									  area.Parent = Math.Floor(area.Value / 100) * 100

									  ' 对于直辖市，可能不存在直接上级，需要再次分析
									  If Not Areas.Any(Function(x) x.Value = area.Parent) Then
										  ' 直接使用省，直辖市级
										  area.Parent = Math.Floor(area.Value / 10000) * 10000
										  area.Ext = 2
									  End If
							  End Select

							  ' 移除 *
							  area.Text = area.Text.Replace("*", "")

							  ' 是否省辖地区
							  Dim Provincial = area.Value.ToString.Substring(2, 2) = "90"

							  Dim entity As New AreaEntity With {
									.ID = area.Value,
									.ParentId = area.Parent,
									.Name = area.Text,
									.AnotherName = area.Text,
									.Level = area.Ext,
									.PinYin = area.Text.ToPinYin,
									.Provincial = Provincial,
									.Enabled = True
							  }

							  ' 入库
							  Using repo = Db.GetRepository(Of AreaEntity)
								  Dim item = repo.Where(Function(x) x.ID = entity.ID).ToOne
								  If item Is Nothing Then
									  repo.Insert(entity)
								  Else
									  If item.Name <> entity.Name Then
										  item.AnotherName = entity.AnotherName
										  item.Name = entity.Name
										  item.PinYin = entity.PinYin

										  repo.Update(item)
									  End If
								  End If
							  End Using
						  End Sub)
		End Sub
	End Class
End Namespace
