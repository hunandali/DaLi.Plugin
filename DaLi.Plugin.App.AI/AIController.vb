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
' 	AI 控制器
'
' 	name: AIController
' 	create: 2022-10-11
' 	memo: AI 控制器
'
' ------------------------------------------------------------

Imports System.ComponentModel
Imports DaLi.App.Attribute
Imports DaLi.Utils.AI
Imports DaLi.Utils.AI.Model
Imports DaLi.Utils.App.Base
Imports DaLi.Utils.Json
Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.Extensions.AI

''' <summary>AI 控制器，登录后可测试</summary>
<Route("ai")>
<Limit(DaLi.App.Model.ActionLimitEnum.USER_LOGIN)>
Public Class AIController
	Inherits CtrBase

	''' <summary>默认参数检查</summary>
	Private Function SettingValidate() As AISettings
		' 默认参数检查
		Dim setting = SYS.GetSetting(Of AISettings)

		If Not setting.ENTITY_AI_URL.IsUrl Then ErrorMessage.Notification = "AI 插件参数中的 ENTITY_AI_URL 配置不正确"
		If setting.ENTITY_AI_KEY.IsEmpty Then ErrorMessage.Notification = "AI 插件参数中的 ENTITY_AI_KEY 配置不正确"
		If setting.ENTITY_AI_MODEL.IsEmpty Then ErrorMessage.Notification = "AI 插件参数中的 ENTITY_AI_MODULE 配置不正确"

		Select Case setting.ENTITY_AI
			Case AIClientEnum.OPENAI, AIClientEnum.OLLAMA
			Case Else
				ErrorMessage.Notification = "AI 插件参数中的 ENTITY_AI 配置不正确"
		End Select

		'If Not ErrorMessage.IsPass Then Return setting

		' AI 测试
		'Dim pro As New AIProvider(setting.ENTITY_AI, setting.ENTITY_AI_URL, setting.ENTITY_AI_KEY, setting.ENTITY_AI_MODEL, setting.ENTITY_AI_OPTIONS.ToJsonDictionary)
		'Dim res = pro.Chat("你好")
		'If Not res.Success Then ErrorMessage.Notification = $"AI 接口异常，无法正常响应 {res.Message?.Content}"

		Return setting
	End Function

	''' <summary>对话</summary>
	<Description("对话")>
	<HttpPost("chat")>
	<HttpGet("chat")>
	Public Function Chat() As IActionResult
		Dim setting = SettingValidate()
		If Not ErrorMessage.IsPass Then Return Err

		' 默认模型
		Dim fields = AppContext.Fields

		' 模型名称
		Dim [module] = fields.GetValue("module", setting.ENTITY_AI_MODEL)

		' 提示词
		Dim prompt = fields.GetValue("prompt")

		' 内容
		Dim content = fields.GetValue("question")
		If content.IsEmpty Then
			ErrorMessage.Notification = "对话的提问不能为空"
			Return Err
		End If

		' 对话历史
		Dim history = fields.GetValue(Of IEnumerable(Of String))("history")

		'' temperature
		'Dim temperature = fields.GetValue(Of Single)("temperature", 0).Range(0, 2)

		'' max_tokens
		'Dim max_tokens = fields.GetValue("max_tokens", 4096)

		'' top_p
		'Dim top_p = fields.GetValue(Of Single)("top_p", 0).Range(0, 1)

		'' top_k
		'Dim top_k = fields.GetValue(Of Single)("top_k", 0).Range(0, 100)

		'' seed
		'Dim seed = fields.GetValue("seed", 0)

		'' 参数
		'Dim params = New KeyValueDictionary
		'If temperature > 0 Then params.Add("temperature", temperature)
		'If top_p > 0 Then params.Add("top_p", top_p)
		'If top_k > 0 Then params.Add("top_k", top_k)
		'If seed > 0 Then params.Add("seed", seed)
		'If max_tokens > 0 Then params.Add("max_tokens", max_tokens)

		Dim ai As New AIChatProvider(setting.ENTITY_AI, setting.ENTITY_AI_URL, setting.ENTITY_AI_KEY, [module]) With {.SystemPrompt = prompt}
		Dim opts = fields.ToJson.FromJson(Of ChatOptions)

		' 对话
		Try
			Dim message = ai.Chat(content, history, opts)
			Return Succ(message.Message.ToString())
		Catch ex As Exception
			Return Err(ex.Message)
		End Try
	End Function

	''' <summary>基于参数的扩写</summary>
	<Description("基于参数的扩写")>
	<HttpPost("write")>
	<HttpGet("write")>
	<ResponseCache(Duration:=864000)>
	Public Function Write() As IActionResult
		Dim setting = SettingValidate()
		If setting.ENTITY_AI_PROMPT_WRITE.IsEmpty Then ErrorMessage.Notification = "AI 插件参数中的 ENTITY_AI_PROMPT_WRITE 配置不正确"
		If Not ErrorMessage.IsPass Then Return Err

		Dim content = AppContext.Fields.GetValue("content")
		If content.IsEmpty OrElse content.Length < 200 Then
			ErrorMessage.Notification = "需要操作的文本内容不能为空，且至少需要200字以上"
			Return Err
		End If

		Dim opts = AppContext.Fields.ToJson.FromJson(Of ChatOptions)
		Dim ai As New AIChatProvider(setting.ENTITY_AI, setting.ENTITY_AI_URL, setting.ENTITY_AI_KEY, setting.ENTITY_AI_MODEL) With {
			.SystemPrompt = setting.ENTITY_AI_PROMPT_WRITE
		}

		Return Succ(ai.Chat(content, Nothing, opts)?.Message.Text)
	End Function

	''' <summary>基于参数的总结与关键词</summary>
	<Description("基于参数的总结与关键词")>
	<HttpPost("summary")>
	<HttpGet("summary")>
	Public Function Summary() As IActionResult
		Dim setting = SettingValidate()
		If setting.ENTITY_AI_PROMPT.IsEmpty Then ErrorMessage.Notification = "AI 插件参数中的 ENTITY_AI_PROMPT 配置不正确"
		If Not ErrorMessage.IsPass Then Return Err

		Dim content = AppContext.Fields.GetValue("content")
		If content.IsEmpty OrElse content.Length < 200 Then
			ErrorMessage.Notification = "需要操作的文本内容不能为空，且至少需要200字以上"
			Return Err
		End If

		Dim opts = AppContext.Fields.ToJson.FromJson(Of ChatOptions)
		Dim ai As New AIChatProvider(setting.ENTITY_AI, setting.ENTITY_AI_URL, setting.ENTITY_AI_KEY, setting.ENTITY_AI_MODEL) With {
			.SystemPrompt = setting.ENTITY_AI_PROMPT,
			.JsonResult = True
		}

		Return Succ(ai.Chat(content, Nothing, opts)?.Message.Text)
	End Function
End Class
