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
' 	地区参数
'
' 	name: AreaSetting
' 	create: 2024-08-21
' 	memo: 地区参数
'
' ------------------------------------------------------------

Imports System.ComponentModel
Imports DaLi.Utils.App.Attribute
Imports DaLi.Utils.App.Base
Imports DaLi.Utils.App.Model.Enums

Namespace ExchangeRate

	''' <summary>地区参数</summary>
	Public Class AreaSetting
		Inherits DbSettingBase(Of AreaSetting)

		''' <summary>民政部行政区划页面网址</summary>
		<Description("民政部行政区划页面网址")>
		<FieldType(FieldValidateEnum.URL)>
		Public Property Url As String = "https://www.mca.gov.cn/mzsj/xzqh/2023/202301xzqh.html"
	End Class
End Namespace