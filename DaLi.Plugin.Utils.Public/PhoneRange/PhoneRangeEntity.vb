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
' 	归属地模型
'
' 	name: Model
' 	create: 2022-10-11
' 	memo: 归属地模型
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations
Imports DaLi.Utils.App.Attribute
Imports DaLi.Utils.App.Base

Namespace PhoneRange

	''' <summary>归属地模型</summary>
	<DbTable("Pub_PhoneRange")>
	Public Class PhoneRangeEntity
		Inherits EntityBase

		''' <summary>号段</summary>
		<Display(Name:="号段")>
		<DbColumn(IsPrimary:=True)>
		Public Shadows Property ID As Integer

		''' <summary>省份</summary>
		<Display(Name:="省份")>
		<MaxLength(20)>
		Public Property Province As String

		''' <summary>城市</summary>
		<Display(Name:="城市")>
		<MaxLength(20)>
		Public Property City As String

		''' <summary>服务商</summary>
		<Display(Name:="服务商")>
		<MaxLength(20)>
		Public Property ISP As String

	End Class

End Namespace