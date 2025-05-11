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
' 	分词组件参数
'
' 	name: Setting.Segment
' 	create: 2021-07-18
' 	memo: 分词组件参数
'
' ------------------------------------------------------------

Imports System.ComponentModel
Imports DaLi.Utils.App.Base
Imports DaLi.Utils.App.[Interface]

''' <summary>分词组件参数</summary>
Public Class SegmentSetting
	Inherits DbSettingBase(Of SegmentSetting)

	''' <summary>分词时需要添加的新词</summary>
	<Description("分词时需要添加的新词，JSON 文本数组")>
	Public Property KeywordInsert As String()

	''' <summary>分词时需要移除的词组</summary>
	<Description("分词时需要移除的词组，JSON 文本数组")>
	Public Property KeywordRemove As String()

	''' <summary>分析关键词时需要过滤的停用词</summary>
	<Description("分析关键词时需要过滤的停用词，JSON 文本数组")>
	Public Property Stopwords As String()

	Protected Overrides Sub Initialize(provider As ISettingProvider)
		Segment.Segment_Keyword_Insert = KeywordInsert
		Segment.Segment_Keyword_Remove = KeywordRemove
		Segment.Extractor_Stopwords = Stopwords
	End Sub
End Class

