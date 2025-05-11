' ------------------------------------------------------------
'
' 	Copyright © 2023 湖南大沥网络科技有限公司.
' 	Dali.App Is licensed under Mulan PSL v2.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	i@woodcoal.cn
' 	homepage:	http://www.hunandali.com/
'
' 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
'
' ------------------------------------------------------------
'
' 	网站入口
'
' 	name: Program
' 	create: 2023-03-02
' 	memo: 网站入口
'
' ------------------------------------------------------------

Imports DaLi.Plugin.Utils
Imports DaLi.Plugin.Utils.ExchangeRate

Public Class Program
	Public Shared Sub Main()
		Dim x = 0
		Call Extend.Start()

		' 无实际意义，用于系统能自动加载插件接口
		If x = 1 Then
			Dim a As New SwashbuckleSetting
			Dim b As New SegmentSetting
			Dim c As New AreaSetting
			Dim d As New BlackListSetting
		End If
	End Sub

End Class
