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
' 	地区控制器
'
' 	name: AreaModel
' 	create: 2023-01-29
' 	memo: 地区控制器
'
' ------------------------------------------------------------

Imports System.ComponentModel
Imports DaLi.Utils.App.Base
Imports Microsoft.AspNetCore.Mvc

Namespace Area

	''' <summary>地区控制器</summary>
	<Route("public/area")>
	Public Class AreaController
		Inherits CtrBase

		Private ReadOnly _Pro As AreaProvider

		Public Sub New(pro As AreaProvider)
			_Pro = pro
		End Sub

		''' <summary>获取区域</summary>
		<Description("获取区域")>
		<HttpGet("{id}")>
		<ResponseCache(Duration:=864000)>
		Public Function Item(id As Long) As IActionResult
			Dim res = _Pro.Item(id)
			Return If(res IsNot Nothing, Succ(res), Err())
		End Function

		''' <summary>区域列表</summary>
		<Description("区域列表")>
		<HttpGet("list")>
		<ResponseCache(Duration:=864000, VaryByQueryKeys:={"level"})>
		Public Function List(Optional level As Integer = 2) As IActionResult
			Dim res = _Pro.List(level)
			Return If(res IsNot Nothing, Succ(res), Err())
		End Function

		''' <summary>区域搜索</summary>
		<Description("区域搜索")>
		<HttpGet("search")>
		<ResponseCache(Duration:=864000, VaryByQueryKeys:={"keyword", "level"})>
		Public Function Search(keyword As String, Optional level As Integer = 2) As IActionResult
			Dim res = _Pro.Search(keyword, level)
			Return If(res IsNot Nothing, Succ(res), Err())
		End Function

	End Class
End Namespace