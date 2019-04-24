Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Workflow.Activities.Rules.Design
Imports System.Reflection
Imports System.Workflow.Activities.Rules
Imports System.Workflow.ComponentModel.Serialization
Imports System.IO
Imports System.Xml
Imports System.CodeDom

Public Class RuleSampleMain

    ''' <summary>
    ''' 指定のルールファイルを読み込むサンプル
    ''' </summary>
    Public Function ruleRead(ByVal fileName As String) As RuleSet

        'Ruleset to work with
        Dim ruleSet As RuleSet = Nothing

        'GUIで作成したルールを読み込み
        Dim rulesReader_init As XmlTextReader = New XmlTextReader(fileName)
        Dim serializer_init As WorkflowMarkupSerializer = New WorkflowMarkupSerializer()
        ruleSet = serializer_init.Deserialize(rulesReader_init)
        rulesReader_init.Close()

        Return ruleSet

    End Function

    ''' <summary>
    ''' ルールをXmlにしてファイルに保存するサンプル
    ''' </summary>
    ''' <param name="ruleSet"></param>
    Public Sub ruleWrite(ByVal ruleSet As RuleSet, ByVal fileName As String)
        '作成後のRuleをファイルに保存
        'Serialize to a .rules file
        Dim serializer As WorkflowMarkupSerializer = New WorkflowMarkupSerializer()
        Dim rulesWriter As XmlWriter = XmlWriter.Create(fileName)
        serializer.Serialize(rulesWriter, ruleSet)
        rulesWriter.Close()
    End Sub

    ''' <summary>
    ''' ルールとファクト(要素)を与えて、実行
    ''' </summary>
    ''' <param name="ruleSet"></param>
    ''' <param name="fact"></param>
    Public Sub executeRule(ByVal ruleSet As RuleSet, fact As Object)
        'Execute the rules And print the entity's properties
        Dim validation As RuleValidation = New RuleValidation(GetType(UsageFee), Nothing)
        Dim execution As RuleExecution = New RuleExecution(validation, fact)
        ruleSet.Execute(execution)

        '実行結果の印字
        PrintProperties(fact)
    End Sub

    ''' <summary>
    ''' 印字
    ''' </summary>
    ''' <param name="myUsageFee"></param>
    Private Sub PrintProperties(ByVal myUsageFee As UsageFee)
        Dim orderType As Type = myUsageFee.GetType
        For Each pi As PropertyInfo In orderType.GetProperties
            My.Application.Log.WriteEntry(pi.Name + " = " + pi.GetValue(myUsageFee, New Object(-1) {}).ToString)
            'Console.WriteLine("{0}= {1}", pi.Name, pi.GetValue(myUsageFee, New Object(-1) {}))
        Next
        My.Application.Log.WriteEntry("個人負担額 = " + myUsageFee.PersonalBurden.ToString)
        'Console.WriteLine(("個人負担額 = " + myUsageFee.PersonalBurden.ToString))
    End Sub

    ''' <summary>
    ''' 自前でルール作成 (Createrの元ネタに使う予定)
    ''' </summary>
    ''' <returns></returns>
    Public Function ruleCreate() As RuleSet

        '内部でルール作成
        Dim ruleSet As RuleSet = createRuleSet()
        Return ruleSet

    End Function

#Region "自前でルール作成"
    ''' <summary>
    ''' RuleSetを作成
    ''' </summary>
    ''' <returns></returns>
    Private Function createRuleSet() As RuleSet
        Dim retRuleSet As RuleSet = New RuleSet()
        Dim rule1 = New Rule("コードで作成したルール")
        Dim thisRef As CodeThisReferenceExpression = New CodeThisReferenceExpression()

        '条件部分の組み立て(this.nextMonthRent > this.upperLimit の場合)
        Dim cond As CodeBinaryOperatorExpression = CreateCondition(thisRef)
        rule1.Condition = New RuleExpressionCondition(cond)

        '条件成立(Then Action)の組み立て
        Dim thenActions As List(Of CodeAssignStatement) = CreateThenAction(thisRef)

        For Each thenAction As CodeAssignStatement In thenActions
            rule1.ThenActions.Add(New RuleStatementAction(thenAction))
        Next

        '計算検証用
        'Dim testAction As CodeAssignStatement = calcTest(thisRef)
        Dim testAction As CodeAssignStatement = calcTest2(thisRef)
        rule1.ThenActions.Add(New RuleStatementAction(testAction))


        '条件不成立(Else Action)の組み立て
        Dim elseActions As List(Of CodeAssignStatement) = CreateElseAction(thisRef)

        For Each elseAction As CodeAssignStatement In elseActions
            rule1.ElseActions.Add(New RuleStatementAction(elseAction))
        Next

        '生成したルールをRuleSetに追加
        retRuleSet.Rules.Add(rule1)

        Return retRuleSet
    End Function

    ''' <summary>
    ''' Conditionの作成
    ''' </summary>
    ''' <returns></returns>
    Private Function CreateCondition(thisRef As CodeThisReferenceExpression) As CodeBinaryOperatorExpression
        '条件部分の組み立て(this.nextMonthRent > this.upperLimit の場合)
        Dim cond As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        'Dim condRefObjLeft As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "nextMonthRent")
        'Dim condRefObjRight As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "upperLimit")
        'cond.Left = condRefObjLeft
        'cond.Operator = CodeBinaryOperatorType.GreaterThan
        'cond.Right = condRefObjRight

        '条件部分の組み立て(this.nextMonthRent > this.upperLimit AND this.administrationCosts >= 100 の場合)
        Dim cond_1 As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        Dim condRefObjLeft_1 As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "nextMonthRent")
        Dim condRefObjRight_1 As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "upperLimit")
        cond_1.Left = condRefObjLeft_1
        cond_1.Operator = CodeBinaryOperatorType.GreaterThan
        cond_1.Right = condRefObjRight_1

        Dim cond_2 As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        Dim condRefObjLeft_2 As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "administrationCosts")
        cond_2.Left = condRefObjLeft_2
        cond_2.Operator = CodeBinaryOperatorType.GreaterThanOrEqual
        cond_2.Right = New CodePrimitiveExpression(100)

        cond.Left = cond_1
        cond.Operator = CodeBinaryOperatorType.BooleanAnd
        cond.Right = cond_2

        Return cond
    End Function

    ''' <summary>
    ''' Then Actionを構築
    ''' </summary>
    ''' <returns></returns>
    Private Function CreateThenAction(thisRef As CodeThisReferenceExpression) As List(Of CodeAssignStatement)
        Dim thenActions As New List(Of CodeAssignStatement)

        '条件成立(Then)の組み立て(this.CampanyBurden = this.upperLimit * (1 - this.PersonalBurdenRate) の場合)
        '右辺に格納する計算式から順に構築していく
        'ここでは  this.upperLimit * (1 - this.PersonalBurdenRate)

        '(1 - this.PersonalBurdenRate)部分
        Dim innerOpe_2 As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        Dim innerRefObj_2 As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "PersonalBurdenRate")
        innerOpe_2.Left = New CodePrimitiveExpression(1)
        '減算演算子
        innerOpe_2.Operator = CodeBinaryOperatorType.Subtract
        innerOpe_2.Right = innerRefObj_2

        'this.upperLimit * (1 - this.PersonalBurdenRate)部分
        Dim innerOpe As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        Dim innerRefObj As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "upperLimit")
        innerOpe.Left = innerRefObj
        '乗算演算子
        innerOpe.Operator = CodeBinaryOperatorType.Multiply
        '右辺に(1 - this.PersonalBurdenRate)部分を設定
        innerOpe.Right = innerOpe_2

        Dim mainRefObj As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "CampanyBurden")
        '代入式
        Dim mainOpe As CodeAssignStatement = New CodeAssignStatement()
        mainOpe.Left = mainRefObj
        mainOpe.Right = innerOpe

        thenActions.Add(mainOpe)

        '二つ目のThenAction（this.PersonalBurden = this.nextMonthRent - this.CampanyBurden + this.administrationCosts）
        'カッコでの優先度がない為、左の値から順に計算するような式を構築
        'this.nextMonthRent - this.CampanyBurden部分
        Dim secondInnerOpe_2 As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        Dim secondInnerRefObjLeft_2 As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "nextMonthRent")
        Dim secondInnerRefObjRight_2 As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "CampanyBurden")
        secondInnerOpe_2.Left = secondInnerRefObjLeft_2
        secondInnerOpe_2.Operator = CodeBinaryOperatorType.Subtract
        secondInnerOpe_2.Right = secondInnerRefObjRight_2

        '左辺「this.nextMonthRent - this.CampanyBurden」 + 右辺「this.administrationCosts」として構築
        Dim secondInnerOpe As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        Dim secondInnerRefObjRight As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "administrationCosts")
        secondInnerOpe.Left = secondInnerOpe_2
        secondInnerOpe.Operator = CodeBinaryOperatorType.Add
        secondInnerOpe.Right = secondInnerRefObjRight

        Dim secondMainRefObj As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "PersonalBurden")
        '代入式
        Dim secondMainOpe As CodeAssignStatement = New CodeAssignStatement()
        secondMainOpe.Left = secondMainRefObj
        secondMainOpe.Right = secondInnerOpe
        thenActions.Add(secondMainOpe)

        Return thenActions
    End Function

    ''' <summary>
    ''' Else Actionを構築
    ''' </summary>
    ''' <returns></returns>
    Private Function CreateElseAction(thisRef As CodeThisReferenceExpression) As List(Of CodeAssignStatement)
        Dim elseActions As New List(Of CodeAssignStatement)

        '条件不成立(Else)の組み立て(this.CampanyBurden = this.nextMonthRent * (1 - this.PersonalBurdenRate)) の場合)
        '右辺に格納する計算式から順に構築していく
        'ここでは  this.nextMonthRent * (1 - this.PersonalBurdenRate)

        '(1 - this.PersonalBurdenRate)部分
        Dim innerOpe_2 As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        Dim innerRefObj_2 As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "PersonalBurdenRate")
        innerOpe_2.Left = New CodePrimitiveExpression(1)
        '減算演算子
        innerOpe_2.Operator = CodeBinaryOperatorType.Subtract
        innerOpe_2.Right = innerRefObj_2

        'this.nextMonthRent * (1 - this.PersonalBurdenRate)部分
        Dim innerOpe As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        Dim innerRefObj As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "nextMonthRent")
        innerOpe.Left = innerRefObj
        '乗算演算子
        innerOpe.Operator = CodeBinaryOperatorType.Multiply
        '右辺に(1 - this.PersonalBurdenRate)部分を設定
        innerOpe.Right = innerOpe_2

        Dim mainRefObj As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "CampanyBurden")
        '代入式
        Dim mainOpe As CodeAssignStatement = New CodeAssignStatement()
        mainOpe.Left = mainRefObj
        mainOpe.Right = innerOpe

        elseActions.Add(mainOpe)

        '二つ目のThenAction（this.PersonalBurden = this.nextMonthRent * this.PersonalBurdenRate + this.administrationCosts）
        '
        'this.nextMonthRent * this.PersonalBurdenRate部分
        'カッコでの優先度がない為、左の値から順に計算するような式を構築
        'this.nextMonthRent - this.CampanyBurden部分
        Dim secondInnerOpe_2 As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        Dim secondInnerRefObjLeft_2 As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "nextMonthRent")
        Dim secondInnerRefObjRight_2 As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "PersonalBurdenRate")
        secondInnerOpe_2.Left = secondInnerRefObjLeft_2
        secondInnerOpe_2.Operator = CodeBinaryOperatorType.Multiply
        secondInnerOpe_2.Right = secondInnerRefObjRight_2

        '左辺「this.nextMonthRent * this.PersonalBurdenRate」 + 右辺「this.administrationCosts」として構築
        Dim secondInnerOpe As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        Dim secondInnerRefObjRight As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "administrationCosts")
        secondInnerOpe.Left = secondInnerOpe_2
        secondInnerOpe.Operator = CodeBinaryOperatorType.Add
        secondInnerOpe.Right = secondInnerRefObjRight

        Dim secondMainRefObj As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "PersonalBurden")
        '代入式
        Dim secondMainOpe As CodeAssignStatement = New CodeAssignStatement()
        secondMainOpe.Left = secondMainRefObj
        secondMainOpe.Right = secondInnerOpe
        elseActions.Add(secondMainOpe)

        Return elseActions
    End Function

    ''' <summary>
    ''' 計算式検証用Action作成
    ''' </summary>
    ''' <param name="thisRef"></param>
    ''' <returns></returns>
    Private Function calcTest(thisRef As CodeThisReferenceExpression) As CodeAssignStatement
        Dim retAction As New CodeAssignStatement

        ''12÷3×2
        'Dim secondInnerOpe_2 As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        'secondInnerOpe_2.Left = New CodePrimitiveExpression(12)
        'secondInnerOpe_2.Operator = CodeBinaryOperatorType.Divide
        'secondInnerOpe_2.Right = New CodePrimitiveExpression(3)

        'Dim secondInnerOpe As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        'secondInnerOpe.Left = secondInnerOpe_2
        'secondInnerOpe.Operator = CodeBinaryOperatorType.Multiply
        'secondInnerOpe.Right = New CodePrimitiveExpression(2)

        'Dim secondMainRefObj As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "calcResult")
        'retAction.Left = secondMainRefObj
        'retAction.Right = secondInnerOpe
        'Return retAction

        ''9-3+2
        'Dim secondInnerOpe_2 As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        'secondInnerOpe_2.Left = New CodePrimitiveExpression(9)
        'secondInnerOpe_2.Operator = CodeBinaryOperatorType.Subtract
        'secondInnerOpe_2.Right = New CodePrimitiveExpression(3)

        'Dim secondInnerOpe As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        'secondInnerOpe.Left = secondInnerOpe_2
        'secondInnerOpe.Operator = CodeBinaryOperatorType.Add
        'secondInnerOpe.Right = New CodePrimitiveExpression(2)

        'Dim secondMainRefObj As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "calcResult")
        'retAction.Left = secondMainRefObj
        'retAction.Right = secondInnerOpe
        'Return retAction


        '(12+3)×(5×2)+150
        Dim secondInnerOpe_1 As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        secondInnerOpe_1.Left = New CodePrimitiveExpression(12)
        secondInnerOpe_1.Operator = CodeBinaryOperatorType.Add
        secondInnerOpe_1.Right = New CodePrimitiveExpression(3)

        Dim secondInnerOpe_2 As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        secondInnerOpe_2.Left = New CodePrimitiveExpression(5)
        secondInnerOpe_2.Operator = CodeBinaryOperatorType.Multiply
        secondInnerOpe_2.Right = New CodePrimitiveExpression(2)

        Dim secondInnerOpe_3 As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        secondInnerOpe_3.Left = secondInnerOpe_1
        secondInnerOpe_3.Operator = CodeBinaryOperatorType.Multiply
        secondInnerOpe_3.Right = secondInnerOpe_2

        Dim secondInnerOpe As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        secondInnerOpe.Left = secondInnerOpe_3
        secondInnerOpe.Operator = CodeBinaryOperatorType.Add
        secondInnerOpe.Right = New CodePrimitiveExpression(150)

        Dim secondMainRefObj As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "calcResult")
        retAction.Left = secondMainRefObj
        retAction.Right = secondInnerOpe

        Return retAction
    End Function

    ''' <summary>
    ''' 計算式検証用Action作成その2
    ''' </summary>
    ''' <param name="thisRef"></param>
    ''' <returns></returns>
    Private Function calcTest2(thisRef As CodeThisReferenceExpression) As CodeAssignStatement
        Dim retAction As New CodeAssignStatement

        '5×300÷3
        Dim innerOpe_1 As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        innerOpe_1.Left = New CodePrimitiveExpression(5)
        innerOpe_1.Operator = CodeBinaryOperatorType.Multiply
        innerOpe_1.Right = New CodePrimitiveExpression(300)

        Dim innerOpe As CodeBinaryOperatorExpression = New CodeBinaryOperatorExpression()
        innerOpe.Left = innerOpe_1
        innerOpe.Operator = CodeBinaryOperatorType.Divide
        innerOpe.Right = New CodePrimitiveExpression(3)

        Dim mainRefObj As CodePropertyReferenceExpression = New CodePropertyReferenceExpression(thisRef, "calcResult")
        retAction.Left = mainRefObj
        retAction.Right = innerOpe

        Return retAction
    End Function

#End Region

End Class
