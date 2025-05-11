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
' 	归属地控制器
'
' 	name: PhoneRangeController
' 	create: 2022-10-11
' 	memo: 归属地控制器
'
' ------------------------------------------------------------

Imports System.ComponentModel
Imports DaLi.Utils.App.Base
Imports Microsoft.AspNetCore.Mvc

Namespace PhoneRange

	''' <summary>归属地控制器</summary>
	<Route("public/phonerange")>
	Public Class PhoneRangeController
		Inherits CtrBase

		''' <summary>获取归属地</summary>
		<Description("获取归属地")>
		<HttpGet("{no}")>
		<ResponseCache(Duration:=864000)>
		Public Function PhoneRange(no As String) As IActionResult
			Dim res = SYS.GetService(Of PhoneRangeProvider).Query(no)
			Return Succ(New With {res.Province, res.City, res.ISP})
		End Function

		''' <summary>获取归属地批量查询</summary>
		<Description("获取归属地批量查询")>
		<HttpPost()>
		Public Function PhoneRanges(nos As String()) As IActionResult
			Dim res = SYS.GetService(Of PhoneRangeProvider).Query(nos)
			Return Succ(res)
		End Function

	End Class
End Namespace