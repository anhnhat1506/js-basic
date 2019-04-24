Imports System
Imports System.Collections.Generic
Imports System.Text
Public Class UsageFee
    ' Data Members
    Public Property member As String   '対象者
    Public Property nextMonthRent As Integer    '翌月賃料
    Public Property administrationCosts As Integer '管理費
    Public Property upperLimit As Integer     '上限賃料
    Public Property PersonalBurdenRate As Decimal    '個人負担率
    Public Property CampanyBurden As Decimal    '会社負担額
    Public Property PersonalBurden As Decimal   '個人負担額
    Public Property calcResult As Decimal   '計算結果（計算部分検証用）

    Public Sub New(ByVal member As String, ByVal nextMonthRent As Integer, ByVal administrationCosts As Integer, ByVal upperLimit As Integer)
        Me.member = member
        Me.nextMonthRent = nextMonthRent
        Me.administrationCosts = administrationCosts
        Me.upperLimit = upperLimit
        Me.PersonalBurdenRate = 0.2
        Me.CampanyBurden = 0
        Me.PersonalBurden = 0
        Me.calcResult = 0
    End Sub
End Class
