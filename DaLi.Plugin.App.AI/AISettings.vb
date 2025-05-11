' ------------------------------------------------------------
'
' 	Copyright © 2023 湖南大沥网络科技有限公司.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	i@woodcoal.cn
' 	homepage:	http://www.hunandali.com/
'
' 	Dali.App Is licensed under GPLv3
'
' ------------------------------------------------------------
'
' 	AI 相关参数设置
'
' 	name: Settings
' 	create: 2024-06-10
' 	memo: AI 相关参数设置
'
' ------------------------------------------------------------

Imports System.ComponentModel
Imports System.ComponentModel.DataAnnotations
Imports DaLi.Utils.AI.Model
Imports DaLi.Utils.App.Attribute
Imports DaLi.Utils.App.Base
Imports DaLi.Utils.App.Model

''' <summary>AI 相关参数设置</summary>
Public Class AISettings
	Inherits DbSettingBase(Of AISettings)

#Region "OLLAMA"

	''' <summary>Ollama 默认服务器地址 http://***:11434</summary>
	<Description("Ollama 默认服务器地址")>
	<FieldType(FieldValidateEnum.URL)>
	Public Property OLLAMA_URL As String

	''' <summary>Ollama 默认模型名称</summary>
	<Description("Ollama 默认模型名称")>
	<MinLength(1)>
	<FieldType(FieldValidateEnum.ASCII)>
	Public Property OLLAMA_MODEL As String

#End Region

#Region "OPENAI"

	''' <summary>OPENAI 默认服务器地址 http://***/</summary>
	<Description("OPENAI 默认服务器地址")>
	<FieldType(FieldValidateEnum.URL)>
	Public Property OPENAI_URL As String

	''' <summary>OPENAI 默认模型名称</summary>
	<Description("OPENAI 默认模型名称")>
	<MinLength(1)>
	<FieldType(FieldValidateEnum.ASCII)>
	Public Property OPENAI_MODEL As String

	''' <summary>OPENAI ApiKey</summary>
	<Description("OPENAI 默认 Api-Key")>
	<MinLength(12)>
	<FieldType(FieldValidateEnum.ASCII)>
	Public Property OPENAI_KEY As String

#End Region

	'#Region "DIFY"

	'	''' <summary>Dify 默认服务器地址 http://ai.hndl.vip/</summary>
	'	<FieldType(FieldValidateEnum.URL)>
	'	<Description("Dify 默认服务器地址")>
	'	Public Property DIFY_URL As String

	'	''' <summary>Dify 助手鉴权 ApiKey</summary>
	'	<Description("Dify Agent 鉴权 Api-Key")>
	'	<MinLength(12)>
	'	<FieldType(FieldValidateEnum.ASCII)>
	'	Public Property DIFY_KEY_AGENT As String

	'	''' <summary>Dify 文本补全鉴权 ApiKey</summary>
	'	<Description("Dify 文本补全鉴权 Api-Key")>
	'	<MinLength(12)>
	'	<FieldType(FieldValidateEnum.ASCII)>
	'	Public Property DIFY_KEY_TEXT As String

	'	''' <summary>Dify 对话鉴权 ApiKey</summary>
	'	<Description("Dify 对话流鉴权 Api-Key")>
	'	<MinLength(12)>
	'	<FieldType(FieldValidateEnum.ASCII)>
	'	Public Property DIFY_KEY_CHAT As String

	'	''' <summary>Dify 流程鉴权 ApiKey</summary>
	'	<Description("Dify 工作流鉴权 Api-Key")>
	'	<MinLength(12)>
	'	<FieldType(FieldValidateEnum.ASCII)>
	'	Public Property DIFY_KEY_WORKFLOW As String

	'	''' <summary>Dify 知识库鉴权 ApiKey (dataset-******)</summary>
	'	<Description("Dify 知识库鉴权 Api-Key")>
	'	<MinLength(12)>
	'	<FieldType(FieldValidateEnum.ASCII)>
	'	Public Property DIFY_KEY_DATASETS As String

	'#End Region

#Region "Entity 操作"

	''' <summary>实体操作 AI 类型</summary>
	<Description("实体操作 AI 类型（1：Ollama，0：OPENAI）")>
	Public Property ENTITY_AI As AIClientEnum = AIClientEnum.OPENAI

	''' <summary>实体操作 AI 服务器地址</summary>
	<Description("实体操作 AI 服务器地址")>
	<FieldType(FieldValidateEnum.URL)>
	Public Property ENTITY_AI_URL As String

	''' <summary>实体操作 AI Api-Key</summary>
	<Description("实体操作 AI Api-Key")>
	<MinLength(12)>
	<FieldType(FieldValidateEnum.ASCII)>
	<FieldEncode>
	Public Property ENTITY_AI_KEY As String

	''' <summary>实体操作 AI 默认模型名称</summary>
	<Description("实体操作 AI 默认模型名称")>
	<MinLength(1)>
	<FieldType(FieldValidateEnum.ASCII)>
	Public Property ENTITY_AI_MODEL As String

	''' <summary>实体操作 AI 默认模型参数</summary>
	<Description("实体操作 AI 默认模型参数")>
	<FieldType(FieldValidateEnum.JSON)>
	Public Property ENTITY_AI_OPTIONS As String

	''' <summary>实体操作 AI 默认模型参数</summary>
	<Description("实体操作 AI 续写默认模型参数")>
	<FieldType(FieldValidateEnum.JSON)>
	Public Property ENTITY_AI_OPTIONS_WRITE As String

	''' <summary>实体操作 AI 分析关键词、总结文档的提示词</summary>
	<Description("实体操作 AI 分析关键词、总结文档的提示词")>
	<FieldChange(FieldTypeEnum.MARKDOWN)>
	Public Property ENTITY_AI_PROMPT As String = $"# Role: 读懂并理解指定的文本，然后用中文总结内容并提取关键词，最后使用JSON格式输出

## Workflows:
  1. **文本预处理**: 读取并解析提供的文本内容。
  2. **内容理解**: 理解文本上下文和语言模式。
  3. **信息抽取**: 提取文本中的关键事实和数据。
  4. **内容概括**: 基于提取的信息只用中文编写300字左右的文本摘要。
  5. **关键词提取**: 从文本和摘要中挑选关键词汇。
  6. **结果格式化**: 构造JSON格式的结果，包含文本摘要和8个关键词。

## Constrains:
1. 总结内容不超过300字。
2. 关键词数量不超过8个。
3. 必须使用中文回复。
4. 输出格式必须为JSON。
5. 避免误解或曲解原文意思。

## OutputFormat:
```json
{{
  ""summary"": ""这里填入总结内容，确保不超过300字。"",
  ""keywords"": ""关键词1,关键词2,...,关键词8""
}}
```
"

	''' <summary>实体操作 AI 续写文档的提示词</summary>
	<Description("实体操作 AI 续写文档的提示词")>
	<FieldChange(FieldTypeEnum.MARKDOWN)>
	Public Property ENTITY_AI_PROMPT_WRITE As String = $"# Role : 读取并理解给定的内容，并根据此内容写一份相关的文章

## Goals:
1. 根据提供的文本内容，书写一份新闻传播特点的文本，提高文本的可读性和吸引力。
2. 确保书写的内容与原文的准确性和一致性。
2. 确保书写的内容的连贯性和逻辑性。
3. 使用markdown格式精准输出书写的内容。

## Constrains:
1. 必须尊重原作者版权，不得抄袭。
2. 使用准确的数据和信息，避免造成误导。
3. 保持中立、客观的态度，避免个人观点和评论。
4. 书写内容应紧扣原文。
5. 书写使用中文，字数约为2000字。
6. 输出格式必须为markdown。

## Workflows:
1. **阅读和理解**：读取并全面理解文章内容，识别关键点。
2. **要点提取**：提取与内容相关的主要事实和信息。
3. **内容重写**：将文章内容按照新闻稿的风格进行重写和格式编排。
4. **结构布局**：设计并安排新闻稿的布局和结构。
5. **审阅和润色**：审核新闻稿的准确性和质量，进行必要的润色。
6. **格式化与输出**：将书写内容以markdown格式进行输出。

## OutputFormat:
1. 应包括标题、导语、主要内容，可能还包括引用或统计数据。
2. 采用清晰、简洁的语言，避免专业术语，除非公众普遍理解。
3. 强调重要事实，避免冗余或不必要的细节。
"

#End Region

	'Protected Overrides Sub Initialize(provider As ISettingProvider)
	'	Utils.AI.AISettings.OLLAMA_URL = OLLAMA_URL
	'	Utils.AI.AISettings.OLLAMA_MODEL = OLLAMA_MODEL

	'	Utils.AI.AISettings.DIFY_URL = DIFY_URL
	'	Utils.AI.AISettings.DIFY_KEY_AGENT = DIFY_KEY_AGENT
	'	Utils.AI.AISettings.DIFY_KEY_TEXT = DIFY_KEY_TEXT
	'	Utils.AI.AISettings.DIFY_KEY_CHAT = DIFY_KEY_CHAT
	'	Utils.AI.AISettings.DIFY_KEY_WORKFLOW = DIFY_KEY_WORKFLOW
	'	Utils.AI.AISettings.DIFY_KEY_DATASETS = DIFY_KEY_DATASETS

	'	Utils.AI.AISettings.OPENAI_URL = OPENAI_URL
	'	Utils.AI.AISettings.OPENAI_MODEL = OPENAI_MODEL
	'	Utils.AI.AISettings.OPENAI_KEY = OPENAI_KEY
	'End Sub

End Class
