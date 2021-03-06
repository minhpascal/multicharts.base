Imports PowerLanguage
Imports PowerLanguage.Function
Imports System
Imports System.Runtime.CompilerServices

Namespace PowerLanguage.Strategy
    <IOGMode(IOGMode.Disabled)> _
    Public Class vb_Parabolic_m_Trail_SX
        Inherits SignalObject
        ' Methods
        Public Sub New(ByVal ctx As Object)
            MyBase.New(ctx)
            Me.NumAtrs = 1.5
            Me.AtrLength = 3
            Me.AccFactorLimit = 0.2
            Me.AccFactorStep = 0.02
        End Sub

        Protected Overrides Sub CalcBar()
            Me.m_MP.Value = MyBase.StrategyInfo.MarketPosition
            If (Me.m_MP.Value < 0) Then
                If (Me.m_MP.Item(1) >= 0) Then
                    Me.m_StopPrice.Value = (MyBase.Bars.High.Item(0) + (Me.AverageTrueRange(Me.AtrLength) * Me.NumAtrs))
                    Me.m_AF.Value = Me.AccFactorStep
                    Me.m_TradeHH.Value = MyBase.Bars.Low.Item(0)
                Else
                    If PublicFunctions.DoubleLess(MyBase.Bars.Low.Item(0), Me.m_TradeHH.Value) Then
                        Me.m_TradeHH.Value = MyBase.Bars.Low.Item(0)
                    End If
                    Me.m_StopPrice.Value = (Me.m_StopPrice.Value - (Me.m_AF.Value * (Me.m_StopPrice.Value - Me.m_TradeHH.Value)))
                    If (PublicFunctions.DoubleLess(Me.m_TradeHH.Value, Me.m_TradeHH.Item(1)) AndAlso PublicFunctions.DoubleLess(Me.m_AF.Value, Me.AccFactorLimit)) Then
                        Me.m_AF.Value = (Me.m_AF.Value + PublicFunctions.Min(Me.AccFactorStep, New Double() { (Me.AccFactorLimit - Me.m_AF.Value) }))
                    End If
                End If
                If PublicFunctions.DoubleLess(Me.m_StopPrice.Value, MyBase.Bars.High.Item(0)) Then
                    Me.m_StopPrice.Value = MyBase.Bars.High.Item(0)
                End If
                Me.m_ParTrSX.Send(Me.m_StopPrice.Value)
            End If
        End Sub

        Protected Overrides Sub Create()
            Me.m_MP = New VariableSeries(Of Integer)(Me)
            Me.m_TradeHH = New VariableSeries(Of Double)(Me)
            Me.m_ParTrSX = MyBase.OrderCreator.Stop(New SOrderParameters(Contracts.Default, "ParTrSX", EOrderAction.BuyToCover, OrderExit.FromAll))
            Me.m_StopPrice = New VariableObject(Of Double)(Me)
            Me.m_AF = New VariableObject(Of Double)(Me)
        End Sub


        ' Properties
        <Input> _
        Public Property AccFactorLimit As Double

        <Input()> _
        Public Property AccFactorStep As Double

        <Input()> _
        Public Property AtrLength As Integer

        <Input()> _
        Public Property NumAtrs As Double


        ' Fields
        Private m_AF As VariableObject(Of Double)
        Private m_MP As VariableSeries(Of Integer)
        Private m_ParTrSX As IOrderPriced
        Private m_StopPrice As VariableObject(Of Double)
        Private m_TradeHH As VariableSeries(Of Double)
    End Class
End Namespace
