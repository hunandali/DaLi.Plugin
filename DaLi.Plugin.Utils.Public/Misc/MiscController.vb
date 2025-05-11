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
' 	其他控制器
'
' 	name: MiscController
' 	create: 2024-08-21
' 	memo: 其他控制器
'
' ------------------------------------------------------------

Imports System.ComponentModel
Imports System.Net.Http
Imports DaLi.Utils.App.Base
Imports DaLi.Utils.App.Extension
Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.Extensions.Caching.Memory

Namespace ExchangeRate
	''' <summary>其他控制器</summary>
	<Route("public/misc")>
	Public Class MiscController
		Inherits CtrBase

		Private ReadOnly _Cache As IMemoryCache

		Public Sub New(cache As IMemoryCache)
			_Cache = cache
		End Sub

		''' <summary>Bing 壁纸</summary>
		<Description("Bing 壁纸")>
		<HttpGet("wallpaper")>
		<ResponseCache(Duration:=3600)>
		Public Async Function WallpaperAsync() As Task(Of IActionResult)
			' 获取汇率数据
			Dim read As Func(Of Task(Of String)) =
				Async Function()
					'{"images":[{"startdate":"20210114","fullstartdate":"202101141600","enddate":"20210115","url":"/th?id=OHR.ChateauBeynac_ZH-CN8777586167_1920x1080.jpg&rf=LaDigue_1920x1080.jpg&pid=hp","urlbase":"/th?id=OHR.ChateauBeynac_ZH-CN8777586167","copyright":"俯瞰着多尔多涅河谷的贝纳克城堡，法国 (© Gareth Kirkland/Alamy)","copyrightlink":"https://www.bing.com/search?q=%E8%B4%9D%E7%BA%B3%E5%85%8B%E5%9F%8E%E5%A0%A1&form=hpcapt&mkt=zh-cn","title":"","quiz":"/search?q=Bing+homepage+quiz&filters=WQOskey:%22HPQuiz_20210114_ChateauBeynac%22&FORM=HPQUIZ","wp":true,"hsh":"771799331e50d74c5dd74fec1e6985ab","drk":1,"top":1,"bot":1,"hs":[]}],"tooltips":{"loading":"正在加载...","previous":"上一个图像","next":"下一个图像","walle":"此图片不能下载用作壁纸。","walls":"下载今日美图。仅限用作桌面壁纸。"}}

					Dim Url = "http://cn.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1"
					Dim http = SYS.GetService(Of IHttpClientFactory).CreateClient
					Dim html = Await http.GetStringAsync(Url)

					If html.NotEmpty Then
						html = html.Cut("url"":""", """", False)
						If html.NotEmpty Then
							Return NetHelper.AbsoluteUrl("http://cn.bing.com/", html)
						End If
					End If

					Return ""
				End Function

			' 缓存键
			Dim key = $"{[GetType].FullName}.Wallpaper"

			' 缓存内容
			Dim value = Await _Cache.ReadOrSave(key, read, 3600)

			' 返回数据
			Return If(value.IsUrl, Redirect(value), Err)
		End Function
	End Class
End Namespace