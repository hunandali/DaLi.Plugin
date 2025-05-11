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
' 	汇率参数
'
' 	name: ExchangeRateSetting
' 	create: 2024-08-21
' 	memo: 汇率参数
'
' ------------------------------------------------------------

Imports System.ComponentModel
Imports DaLi.Utils.App.Attribute
Imports DaLi.Utils.App.Base
Imports DaLi.Utils.App.Model.Enums

Namespace ExchangeRate

	''' <summary>汇率参数</summary>
	Public Class ExchangeRateSetting
		Inherits DbSettingBase(Of ExchangeRateSetting)

		''' <summary>汇率 API 地址</summary>
		<Description("汇率 API 地址")>
		<FieldType(FieldValidateEnum.URL)>
		Public Property Url As String = "https://open.er-api.com/v6/latest/CNY"

		''' <summary>默认汇率数据</summary>
		<Description("默认汇率数据")>
		<FieldType(FieldValidateEnum.JSON)>
		Public Property Data As String
	End Class
End Namespace