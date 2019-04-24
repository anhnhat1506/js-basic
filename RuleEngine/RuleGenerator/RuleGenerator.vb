Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Workflow.Activities.Rules.Design
Imports System.Reflection
Imports System.Workflow.Activities.Rules
Imports System.Workflow.ComponentModel.Serialization
Imports System.IO
Imports System.Xml
Public Class RuleGenerator

    Public Sub generate()
        'RuleSet の作成
        Dim addedRuleSet As AddedRuleSetAction = New AddedRuleSetAction()
        Dim ruleSet As RuleSet = addedRuleSet.RuleSetDefinition()

        '評価式の生成
        'Dim condition As RuleCondition = 
        'thenAction の生成
        'elseAction の生成

        'Rule の生成
        Dim rule As Rule = New Rule("SAMPLE_RULE", Nothing, Nothing, Nothing)


    End Sub
End Class
