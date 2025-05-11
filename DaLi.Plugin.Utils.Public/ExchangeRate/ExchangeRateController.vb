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
' 	汇率控制器
'
' 	name: ExchangeRateController
' 	create: 2024-08-21
' 	memo: 汇率控制器
'
' ------------------------------------------------------------

Imports System.ComponentModel
Imports System.Net.Http
Imports DaLi.Utils.App.Base
Imports DaLi.Utils.App.Extension
Imports DaLi.Utils.Json
Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.Extensions.Caching.Memory

Namespace ExchangeRate
	''' <summary>汇率控制器</summary>
	<Route("public/exchangerate")>
	Public Class ExchangeRateController
		Inherits CtrBase

		Private ReadOnly _Cache As IMemoryCache
		Private Shared ReadOnly _Fields As String() = {"time_last_update_unix", "base_code", "rates"}

		Public Sub New(cache As IMemoryCache)
			_Cache = cache
		End Sub

		''' <summary>获取汇率数据</summary>
		<Description("获取汇率数据")>
		<HttpGet()>
		<ResponseCache(Duration:=3600)>
		Public Async Function ExchangeRateAsync() As Task(Of IActionResult)
			' 获取汇率数据
			Dim read As Func(Of Task(Of IDictionary(Of String, Object))) =
				Async Function()
					Dim Setting = SYS.GetSetting(Of ExchangeRateSetting)
					Dim http = SYS.GetService(Of IHttpClientFactory).CreateClient
					Dim html = Await http.GetStringAsync(Setting.Url)

					' 未获取到则从设置种获取
					If html.IsEmpty Then
						html = Setting.Data
						Return html?.ToJsonDictionary
					Else
						' 记录，仅保留有效的数据：time_last_update_unix  / base_code / rates
						Dim data = html.ToJsonDictionary
						data = data?.Where(Function(x) _Fields.Contains(x.Key)).ToDictionary(Function(x) x.Key, Function(x) x.Value)
						Setting.Update(NameOf(Setting.Data), data?.ToJson)

						Return data
					End If
				End Function

			' 缓存键
			Dim key = [GetType].FullName

			' 缓存内容
			Dim value = Await _Cache.ReadOrSave(key, read, 3600)

			' 返回数据
			Return If(value.IsEmpty, Err, Succ(value))
		End Function
	End Class
End Namespace