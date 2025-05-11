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
' 	地区模型
'
' 	name: AreaModel
' 	create: 2023-01-29
' 	memo: 地区模型
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations
Imports DaLi.Utils.App.Attribute
Imports DaLi.Utils.App.Base

Namespace Area

	''' <summary>地区模型</summary>
	<DbTable("Pub_Area")>
	Public Class AreaEntity
		Inherits EntityTreeBase(Of AreaEntity)

		''' <summary>地区级别枚举</summary>
		Public Enum LevelEnum
			''' <summary>省，自治区，直辖市</summary>
			<Display(Name:="省，自治区，直辖市")>
			Province = 1

			''' <summary>市，省辖县</summary>
			<Display(Name:="市，省辖县")>
			City = 2

			''' <summary>县、区</summary>
			<Display(Name:="县、区")>
			County = 3
		End Enum

		''' <summary>名称</summary>
		<Display(Name:="名称")>
		<MaxLength(100)>
		<Required>
		Public Property Name As String

		''' <summary>别称</summary>
		<Display(Name:="别称")>
		<MaxLength(50)>
		<Required>
		Public Property AnotherName As String

		''' <summary>拼音</summary>
		<Display(Name:="拼音")>
		<MaxLength(100)>
		Public Property PinYin As String

		''' <summary>层级</summary>
		<Display(Name:="层级")>
		<Required>
		Public Property Level As LevelEnum

		''' <summary>是否省直辖</summary>
		Public Property Provincial As Boolean

		''' <summary>启用</summary>
		Public Overloads Property Enabled As Boolean

	End Class

End Namespace