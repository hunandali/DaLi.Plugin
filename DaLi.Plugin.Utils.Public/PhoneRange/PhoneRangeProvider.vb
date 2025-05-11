' ------------------------------------------------------------
'
' 	Copyright © 2021 湖南大沥网络科技有限公司.
' 	Dali.Utils Is licensed under Mulan PSL v2.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	a@hndl.vip
' 	homepage:	http://www.hunandali.com/
'
' 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
'
' ------------------------------------------------------------
'
' 	归属地操作
'
' 	name: PhoneRangeProvider.vb
' 	create: 2024
' 	memo: 归属地操作
'
' ------------------------------------------------------------

Imports DaLi.Utils.App.[Interface]
Imports DaLi.Utils.Http
Imports DaLi.Utils.Json

Namespace PhoneRange
	''' <summary>归属地操作</summary>
	Public Class PhoneRangeProvider

		''' <summary>数据库对象</summary>
		Protected Db As IFreeSql

		Public Sub New(db As IDatabaseProvider)
			Me.Db = db.GetDb(VAR_DATABASE_CONNECTION_EXTEND)
		End Sub

		''' <summary>添加归属地</summary>
		''' <param name="phone">手机号或者归属地(7/11位)</param>
		''' <param name="province">省份</param>
		''' <param name="city">城市</param>
		''' <param name="isp">服务商</param>
		Public Function Insert(phone As String, province As String, city As String, isp As String) As Boolean
			If province.IsEmpty AndAlso city.IsEmpty AndAlso isp.IsEmpty Then Return False

			If phone.IsEmpty Then Return False
			If phone.Length = 7 Then phone &= "0000"
			If Not phone.IsMobilePhone Then Return False

			Dim no = phone.Substring(0, 7).ToLong

			' 检查是否已经存在
			Using repo = Db.GetRepository(Of PhoneRangeEntity)
				Dim item = repo.Where(Function(x) x.ID = no).ToOne
				If item Is Nothing Then
					item = New PhoneRangeEntity With {
						.ID = no,
						.Province = province,
						.City = city,
						.ISP = isp
					}

					repo.Insert(item)
				Else
					If item.Province.IsEmpty AndAlso province.NotEmpty Then item.Province = province
					If item.City.IsEmpty AndAlso city.NotEmpty Then item.City = city
					If item.ISP.IsEmpty AndAlso isp.NotEmpty Then item.ISP = isp

					repo.Update(item)
				End If
			End Using

			Return True
		End Function

		''' <summary>归属地查询</summary>
		''' <param name="phone">手机号或者归属地(7/11位)</param>
		Public Function Query(phone As String) As PhoneRangeEntity
			If phone.IsEmpty Then Return Nothing
			If phone.Length = 7 Then phone &= "0000"
			If Not phone.IsMobilePhone Then Return Nothing

			Dim no = phone.Substring(0, 7).ToInteger
			Dim item = Db.Select(Of PhoneRangeEntity).Where(Function(x) x.ID = no).ToOne

			' 不存在，采集
			If item Is Nothing Then
				item = CollectPhone(no)
				If item IsNot Nothing Then Db.Insert(Of PhoneRangeEntity).AppendData(item).ExecuteAffrows()
			End If

			' 返回
			If item Is Nothing Then
				Return Nothing
			Else
				Return item
			End If
		End Function

		''' <summary>归属地查询</summary>
		''' <param name="phones">手机号或者归属地(7/11位)</param>
		Public Function Query(ParamArray phones As String()) As List(Of PhoneRangeEntity)
			If phones.IsEmpty Then Return Nothing

			Dim nos = phones.
				Select(Function(x)
						   If x.Length = 7 Then x &= "0000"
						   Return If(x.IsPhone, x.Substring(0, 7).ToInteger, 0)
					   End Function).
				Where(Function(x) x > 0).
				Distinct()
			If nos.IsEmpty Then Return Nothing

			' 查询
			Dim rets = Db.Select(Of PhoneRangeEntity).Where(Function(x) nos.Contains(x.ID)).ToList

			' 不存在的项目异步获取
			nos = nos.Except(rets.Select(Function(x) x.ID))
			If nos.NotEmpty Then
				' 分析数据
				Task.Run(Sub()
							 For Each no In nos
								 Dim item = CollectPhone(no)
								 If item IsNot Nothing Then Db.Insert(Of PhoneRangeEntity).AppendData(item).ExecuteAffrows()
							 Next
						 End Sub)
			End If

			Return rets
		End Function

		''' <summary>采集数据</summary>
		Public Shared Function CollectPhone(phone As Long) As PhoneRangeEntity
			'{
			'    "code": 0,
			'    "data": {
			'        "province": "江苏",
			'        "city": "常州",
			'        "isp": "移动",
			'        "postcode": "213000",
			'        "citycode": "0519",
			'        "areacode": "320400",
			'        "version": "20221011"
			'    }
			'}

			Dim http As New HttpClient With {
				.Url = "https://www.qqzeng-ip.com/api/phone",
				.UserAgent = "MicroMessenger/8.0.28.2240",
				.Referer = "https://servicewechat.com/",
				.Method = Model.HttpMethodEnum.POST,
				.PostType = Model.HttpPostEnum.DEFAULT
			}
			http.SetPostContent("phone", phone)
			http.Execute()

			Dim Json = http.GetString()
			Dim dic = Json.ToJsonDictionary
			Dim data = Nothing
			If dic?.TryGetValue("data", data) Then
				Dim value = TryCast(data, IDictionary(Of String, Object))
				If value?.ContainsKey("isp") Then
					Dim item As New PhoneRangeEntity With {
						.ID = phone,
						.Province = value("province"),
						.City = value("city"),
						.ISP = value("isp")
					}

					If item.ISP.NotEmpty Then
						If item.ISP = "不足" Then Throw New Exception("已经被禁用")
						Return item
					End If
				End If
			End If

			Return Nothing
		End Function

	End Class
End Namespace