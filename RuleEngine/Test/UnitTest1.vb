Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports RuleEngine
Imports System.Workflow.Activities.Rules

<TestClass()> Public Class UnitTest1

#Region "設定"
    'FileRootPath
    Dim filePath As String = "C:\temp\"
    'ルールサンプルのファイル読込先
    Dim ruleFileSample As String = filePath + "UsageFeeRule.txt"
    '生成したルールの保存先
    Dim ruleFileCreate As String = filePath + "CreateRule_logic.txt"
#End Region

    ''' <summary>
    ''' GUIで作成したルールファイルを使って、ルールの読込と値を入れての実行テスト
    ''' ↓こんなログが出ます
    ''' DefaultSource Information: 0 : member = 借上さん
    ''' DefaultSource Information :  0 : nextMonthRent = 12
    ''' DefaultSource Information :  0 : administrationCosts = 100
    ''' DefaultSource Information :  0 : upperLimit = 100000
    ''' DefaultSource Information :  0 : PersonalBurdenRate = 0.2
    ''' DefaultSource Information :  0 : CampanyBurden = 9.6
    ''' DefaultSource Information :  0 : PersonalBurden = 102.4
    ''' DefaultSource Information :  0 : calcResult = 0
    ''' DefaultSource Information :  0 : 個人負担額 = 102.4
    ''' </summary>
    <TestMethod()> Public Sub TestMethod1()
        Dim testClass As New RuleSampleMain()

        'ルールのリード
        Dim ruleSet As RuleSet = testClass.ruleRead(ruleFileSample)

        'ファクトの作成
        Dim fact As UsageFee = New UsageFee("借上さん", 12, 100, 100000)

        'ルールの実行
        testClass.executeRule(ruleSet, fact)

    End Sub

    ''' <summary>
    ''' 独自にルールをくみ上げて、ルールの実行と保存のテスト
    ''' ↓こんなログが出ます
    ''' DefaultSource Information: 0 : member = 借上さん
    ''' DefaultSource Information :  0 : nextMonthRent = 12
    ''' DefaultSource Information :  0 : administrationCosts = 100
    ''' DefaultSource Information :  0 : upperLimit = 100000
    ''' DefaultSource Information :  0 : PersonalBurdenRate = 0.2
    ''' DefaultSource Information :  0 : CampanyBurden = 9.6
    ''' DefaultSource Information :  0 : PersonalBurden = 102.4
    ''' DefaultSource Information :  0 : calcResult = 0
    ''' DefaultSource Information :  0 : 個人負担額 = 102.4
    ''' </summary>
    <TestMethod()> Public Sub TestMethod2()
        Dim testClass As New RuleSampleMain()

        'ルールの自前作成
        Dim ruleSet As RuleSet = testClass.ruleCreate()

        'ファクトの作成
        Dim fact As UsageFee = New UsageFee("借上さん", 12, 100, 100000)

        'ルールの実行
        testClass.executeRule(ruleSet, fact)

        'ルールをファイルに保存
        testClass.ruleWrite(ruleSet, ruleFileCreate)

    End Sub

End Class