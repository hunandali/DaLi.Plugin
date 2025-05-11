' ------------------------------------------------------------
'
' 	Copyright © 2021 湖南大沥网络科技有限公司.
' 	Dali.Utils Is licensed under Mulan PSL v2.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	i@woodcoal.cn
' 	homepage:	http://www.hunandali.com/
'
' 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
'
' ------------------------------------------------------------
'
' 	AI 文本对话
'
' 	name: Auto.Rule.AIChat
' 	create: 2024-12-21
' 	memo: AI 文本对话
' 	
' ------------------------------------------------------------

Imports System.Threading
Imports DaLi.Utils.AI
Imports DaLi.Utils.AI.Model
Imports DaLi.Utils.Auto
Imports DaLi.Utils.Json
Imports Microsoft.Extensions.AI

Namespace Auto.Rule

	''' <summary>AI 文本对话</summary>
	Public Class AIChat
		Inherits RuleBase

		''' <summary>历史消息结构</summary>
		Private Structure ChatMessage
			''' <summary>角色：user、assistant、system</summary>
			Public Property Role As String

			''' <summary>内容</summary>
			Public Property Content As String

		End Structure

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "AI 文本对话"
			End Get
		End Property

		''' <summary>客户端接口类型</summary>
		Public Property Client As AIClientEnum = AIClientEnum.OPENAI

		''' <summary>原始内容</summary>
		Public Property Source As String

		''' <summary>服务器地址,不设置则使用系统全局参数</summary>
		Public Property Url As String

		''' <summary>ApiKey,不设置则使用系统全局参数</summary>
		Public Property ApiKey As String

		''' <summary>模型</summary>
		Public Property Model As String

		''' <summary>系统提示词</summary>
		Public Property System As String

		''' <summary>模型参数</summary>
		Public Property Options As KeyValueDictionary

		''' <summary>对话历史</summary>
		Public Property History As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "要分析的原始内容未设置"
			If Source.IsEmpty Then Return False

			message = "无效接口类型"
			If Client < 0 OrElse Client > 1 Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary> 
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage, cancel As CancellationToken) As Object
			Dim setting = SYS.GetSetting(Of AISettings)

			Dim url = AutoHelper.GetVarString(Me.Url, data)
			Dim apiKey = AutoHelper.GetVarString(Me.ApiKey, data)
			Dim model = AutoHelper.GetVarString(Me.Model, data)

			Select Case Client
				Case AIClientEnum.OPENAI
					url = url.EmptyValue(setting.OPENAI_URL)
					apiKey = apiKey.EmptyValue(setting.OPENAI_KEY)
					model = model.EmptyValue(setting.OPENAI_MODEL)

				Case AIClientEnum.OLLAMA
					url = url.EmptyValue(setting.OLLAMA_URL)
					model = model.EmptyValue(setting.OLLAMA_MODEL)
			End Select

			' 基础参数检查
			If Not url.IsUrl Then
				message.SetSuccess(False, "AI 服务器地址无效")
				Return Nothing
			End If

			If model.IsEmpty Then
				message.SetSuccess(False, "AI 模型名称无效")
				Return Nothing
			End If

			Select Case Client
				Case AIClientEnum.OLLAMA
				Case Else
					If apiKey.IsEmpty OrElse apiKey.Length < 10 Then
						message.SetSuccess(False, "AI 密钥无效")
						Return Nothing
					End If
			End Select

			' 内容
			Dim content = AutoHelper.GetVarString(Source, data)
			Dim system = AutoHelper.GetVarString(Me.System, data)
			Dim histroy = History?.FromJson(Of List(Of ChatMessage)).
				Select(Function(x)
						   Dim body = AutoHelper.GetVarString(x.Content, data)
						   If body.IsEmpty Then Return ""

						   Dim role = x.Role.EmptyValue("User").ToUpperInvariant.Substring(0, 1)
						   Select Case role
							   Case "U", "A", "S"
							   Case Else
								   role = "U"
						   End Select

						   Return $"{role}:{body}"
					   End Function).
				Where(Function(x) x.NotEmpty).
ToList

			' 操作
			Dim Opts = Options.FormatAction(Function(x) AutoHelper.GetVar(x, data)).ToJson.FromJson(Of ChatOptions)
			Dim ai As New AIChatProvider(Client, url, apiKey, model) With {.SystemPrompt = system}
			Dim chat = ai.Chat(content, histroy, Opts)
			Dim res = chat.Message.Text
			message.SetSuccess(res.NotEmpty, res)
			Return res
		End Function

#End Region

	End Class
End Namespace
