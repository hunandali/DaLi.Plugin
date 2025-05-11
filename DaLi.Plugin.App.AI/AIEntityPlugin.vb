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
' 	对实体数据进行 AI 处理
'
' 	name: AIEntityPlugin
' 	create: 2024-06-28
' 	memo: 对实体数据进行 AI 处理，如分析关键词，总结等操作。
' 		  只处理属性上存在 EntityCustomAttribute 属性的字段，如： EntityCustom(Provider = "ai", Action = "keyword", Source = "content")；
' 		  EntityCustomAttribute 的 Provider 必须是 ai，Action 必须是关键词或者总结、续写。
'
' ------------------------------------------------------------

Imports DaLi.Utils.AI
Imports DaLi.Utils.App.Base
Imports DaLi.Utils.App.Extension
Imports DaLi.Utils.App.[Interface]
Imports DaLi.Utils.App.Model
Imports DaLi.Utils.Extension.StringExtension
Imports DaLi.Utils.Json

''' <summary>对实体数据进行 AI 处理</summary>
Public Class AIEntityPlugin
	Inherits EntityPluginBase

	''' <summary>默认操作源名称(EntityCustom 中 Provider 的值)</summary>
	Protected Overrides ReadOnly Property ProviderName As String
		Get
			Return "ai"
		End Get
	End Property

	''' <summary>默认允许的操作，为空则表示不设置才生效(EntityCustom 中 Action 是否必须在此范围)</summary>
	Protected Overrides ReadOnly Property EnabledActions As String()
		Get
			Return {"keyword", "keywords", "summary", "write"}
		End Get
	End Property

	''' <summary>是否强制需要存在来源字段才能处理(EntityCustom 中 Source 是否必须有效)</summary>
	Protected Overrides ReadOnly Property SourceForce As Boolean
		Get
			Return True
		End Get
	End Property

	''' <summary>总结关键词操作</summary>
	Private Shared ReadOnly _KeywordAction As String() = {"summary", "keyword", "keywords"}

	''' <summary>关键词字段</summary>
	Private Shared ReadOnly _Keywords As String() = {"keyword", "keywords"}

	''' <summary>项目操作完成后处理事件</summary>
	''' <param name="action">操作类型：item/add/edit/delete/list/export...</param>
	''' <param name="data">单项操作时单个数值，多项时为数组</param>
	''' <param name="source">单项编辑时更新前的原始值</param>
	Public Overrides Sub ExecuteFinish(Of T As IEntity)(action As EntityActionEnum, data As ObjectArray(Of T), context As IAppContext, errorMessage As ErrorMessage, db As IFreeSql, Optional source As T = Nothing)
		' AI 处理涉及到网络请求，可能存在异常，故使用异步请求处理
		Select Case action
			Case EntityActionEnum.ADD, EntityActionEnum.EDIT
				Task.Run(Sub() data.ForEach(Sub(entity) DoAction(entity, Nothing, db)))
		End Select
	End Sub

	Private Sub DoAction(Of T As IEntity)(entity As T, context As IAppContext, db As IFreeSql)
		' 添加，编辑模式下检查关键词
		Dim pros = GetProperties(entity)
		If pros Is Nothing Then Return

		' 参数
		Dim settings = SYS.GetSetting(Of AISettings)

		' 需要更新的字段与值
		Dim UpdateColumns As New List(Of String)

		' 处理字段
		' 注意续写优先处理，防止总结或者关键词来自此内容
		For Each kv In pros
			' 不需要续写则跳过
			If Not ExistAction(kv.Value, "write") Then Continue For

			' 少于 10 个字符，不处理
			Dim content = kv.Key.GetValue(entity)?.ToString
			If content.IsEmpty OrElse content.Length < 10 Then Continue For

			' 获取文本字段长度
			Dim Lens = kv.Value.Select(Function(x) x.Target).
				Distinct.
				ToDictionary(Function(x) x, Function(x) x.GetStringAttributeLength).
				Where(Function(x) x.Value >= 10 OrElse x.Value = 0).
				ToDictionary(Function(x) x.Key, Function(x) x.Value)

			If Lens.IsEmpty Then Continue For

			' 要修改的属性，必须是文本字段，且允许长度大于0
			Dim attrs = kv.Value.Where(Function(x) Lens.ContainsKey(x.Target)).ToList

			' 续写处理
			Dim ai As New AIChatProvider(settings.ENTITY_AI, settings.ENTITY_AI_URL, settings.ENTITY_AI_KEY, settings.ENTITY_AI_MODEL) With {.SystemPrompt = settings.ENTITY_AI_PROMPT_WRITE}
			Dim write = ai.Chat(content)?.Message.Text
			If write.NotEmpty Then
				UpdateValue(entity, attrs, context,
							write, "write",
							Function(pro, value)
								Dim len = Lens(pro)
								value = value.ToString.Left(len, "……")
								UpdateColumns.Add(pro.Name)
								Return value
							End Function)
			End If
		Next

		' 总结，关键词
		For Each kv In pros
			If Not ExistAction(kv.Value, _KeywordAction) Then Continue For

			' 少于 50 个字符，不处理
			Dim content = kv.Key.GetValue(entity)?.ToString
			If content.IsEmpty OrElse content.Length < 50 Then Continue For

			' 获取文本字段长度
			Dim Lens = kv.Value.Select(Function(x) x.Target).
				Distinct.
				ToDictionary(Function(x) x, Function(x) x.GetStringAttributeLength).
				Where(Function(x) x.Value >= 10 OrElse x.Value = 0).
				ToDictionary(Function(x) x.Key, Function(x) x.Value)

			If Lens.IsEmpty Then Continue For

			' 要修改的属性，必须是文本字段，且允许长度大于0
			Dim attrs = kv.Value.Where(Function(x) Lens.ContainsKey(x.Target)).ToList

			Dim ai As New AIChatProvider(settings.ENTITY_AI, settings.ENTITY_AI_URL, settings.ENTITY_AI_KEY, settings.ENTITY_AI_MODEL) With {.SystemPrompt = settings.ENTITY_AI_PROMPT, .JsonResult = True}
			Dim str = ai.Chat(content)?.Message.Text
			If str.Contains("```") Then
				str = str.Cut("```", "```", False, False)
				str = str.TrimFull
				If str.StartsWith("json") Then str = str.Substring(4)
			End If

			Dim data = New KeyValueDictionary(str.ToJsonDictionary)
			If data.IsEmpty Then
				' 通过分词根据关键词进行总结
				Dim segSummary = Segment.Default.Summary(content, 250, True)
				If segSummary.NotEmpty Then data.Add("summary", segSummary)

				Dim segKeywords = Segment.Default.Keywords(content, 20, JiebaNet.Segmenter.Constants.NounPos, -20)
				If segKeywords.NotEmpty Then data.Add("keywords", segKeywords)
			End If

			If data.IsEmpty Then Continue For

			' 总结
			Dim summary = data.GetValue("summary")
			If summary.NotEmpty Then
				UpdateValue(entity, attrs, context,
							summary, "summary",
							Function(pro, value)
								Dim len = Lens(pro)
								value = value.ToString.Left(len, "……")
								UpdateColumns.Add(pro.Name)
								Return value
							End Function)
			End If

			' 关键词
			Dim keywords = data.GetValue("keywords").SplitEx
			If keywords.NotEmpty Then
				UpdateValue(entity, attrs, context,
							keywords, _Keywords,
							Function(pro, value)
								Dim len = Lens(pro)
								value = keywords.JoinString(",", len)

								UpdateColumns.Add(pro.Name)
								Return value
							End Function)
			End If
		Next

		' 数据入库
		If UpdateColumns.IsEmpty Then Return

		db.Update(Of Object).AsType(entity.GetType).SetSource(entity).UpdateColumns(UpdateColumns.ToArray).ExecuteAffrows()
	End Sub

End Class

' 关键词，总结
'For Each kv In pros
'	If Not ExistAction(kv.Value, "write") Then Continue For

'	' 少于 50 个字符，不处理
'	Dim content = kv.Key.GetValue(entity)?.ToString
'	If content.IsEmpty OrElse content.Length < 50 Then Continue For

'	' 获取文本字段长度
'	Dim Lens = kv.Value.Select(Function(x) x.Target).
'		Distinct.
'		ToDictionary(Function(x) x, Function(x) x.GetStringLength).
'		Where(Function(x) x.Value >= 10 OrElse x.Value = 0).
'		ToDictionary(Function(x) x.Key, Function(x) x.Value)

'	If Lens.IsEmpty Then Continue For

'	' 要修改的属性，必须是文本字段，且允许长度大于0
'	Dim attrs = kv.Value.Where(Function(x) Lens.ContainsKey(x.Target)).ToList

'	' 需要续写
'	If ExistAction(attrs, "write") Then
'		Dim ai As New Utils.AI.AIProvider(settings.ENTITY_AI, settings.ENTITY_AI_URL, settings.ENTITY_AI_KEY, settings.ENTITY_AI_MODEL, settings.ENTITY_AI_OPTIONS_WRITE.ToJsonDictionary) With {
'			.System = settings.ENTITY_AI_PROMPT_WRITE
'		}
'		Dim write = ai.Text(content)?.Content
'		If write.NotEmpty Then
'			UpdateValue(entity, attrs, context,
'						write, "write",
'						Function(pro, value)
'							Dim len = Lens(pro)
'							value = value.ToString.Left(len, "……")
'							UpdateColumns.Add(pro.Name)
'							Return value
'						End Function)
'		End If
'	End If

'	' 分析获取总结，关键词
'	If ExistAction(attrs, {"summary", "keyword", "keywords"}) Then
'		Dim ai As New Utils.AI.AIProvider(settings.ENTITY_AI, settings.ENTITY_AI_URL, settings.ENTITY_AI_KEY, settings.ENTITY_AI_MODEL, settings.ENTITY_AI_OPTIONS.ToJsonDictionary) With {
'			.System = settings.ENTITY_AI_PROMPT
'		}
'		Dim str = ai.Text(content)?.Content
'		If str.Contains("```") Then
'			str = str.Cut("```", "```", False, False)
'			str = str?.TrimFull
'			If str.StartsWith("json") Then str = str.Substring(4)
'		End If

'		Dim data = New KeyValueDictionary(str.ToJsonDictionary)
'		If data.IsEmpty Then Continue For

'		' 总结
'		Dim summary = data.GetValue("summary")
'		If summary.NotEmpty Then
'			UpdateValue(entity, attrs, context,
'						summary, "summary",
'						Function(pro, value)
'							Dim len = Lens(pro)
'							value = value.ToString.Left(len, "……")
'							UpdateColumns.Add(pro.Name)
'							Return value
'						End Function)
'		End If

'		' 关键词
'		Dim keywords = data.GetValue("keywords").SplitEx
'		If keywords.NotEmpty Then
'			UpdateValue(entity, attrs, context,
'						keywords, {"keyword", "keywords"},
'						Function(pro, value)
'							Dim len = Lens(pro)
'							value = keywords.JoinString(",", len)

'							UpdateColumns.Add(pro.Name)
'							Return value
'						End Function)
'		End If
'	End If
'Next