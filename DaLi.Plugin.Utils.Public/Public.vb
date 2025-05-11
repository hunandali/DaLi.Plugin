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
' 	公共全局启动类
'
' 	name: Public
' 	create: 2024-08-21
' 	memo: 公共全局启动类
'
' ------------------------------------------------------------

Imports DaLi.Plugin.Utils.Area
Imports DaLi.Plugin.Utils.PhoneRange
Imports DaLi.Utils.App.Base
Imports Microsoft.Extensions.DependencyInjection

''' <summary>公共全局启动类</summary>
Public Class [Public]
	Inherits ModuleBase

	''' <summary>注册操作</summary>
	Public Overrides Sub AddServices(services As IServiceCollection)
		services.AddSingleton(Of AreaProvider)
		services.AddSingleton(Of PhoneRangeProvider)
	End Sub
End Class
