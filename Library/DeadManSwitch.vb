﻿''' <summary>
''' A class which raises an event if it is armed and not continuously reset.
''' </summary>
<DebuggerDisplay("{ToString()}")>
Public NotInheritable Class DeadManSwitch
    Private ReadOnly _period As TimeSpan
    Private _isArmed As Boolean
    Private _timer As ClockTimer
    Private _waitRunning As Boolean
    Private ReadOnly inQueue As CallQueue = MakeTaskedCallQueue()

    Public Event Triggered(sender As DeadManSwitch)

    <ContractInvariantMethod()> Private Sub ObjectInvariant()
        Contract.Invariant(_period.Ticks > 0)
        Contract.Invariant(_timer IsNot Nothing)
        Contract.Invariant(inQueue IsNot Nothing)
    End Sub

    Public Sub New(period As TimeSpan, clock As IClock)
        Contract.Assume(period.Ticks > 0)
        Contract.Assume(clock IsNot Nothing)
        Me._period = period
        Me._timer = clock.StartTimer()
    End Sub

    ''' <summary>
    ''' Starts the countdown timer.
    ''' No effect if the timer is already started.
    ''' </summary>
    Public Function QueueArm() As Task
        Contract.Ensures(Contract.Result(Of Task)() IsNot Nothing)
        Return inQueue.QueueAction(AddressOf Arm)
    End Function
    Private Async Sub Arm()
        If _isArmed Then Return
        _isArmed = True
        _timer = _timer.Restarted
        If _waitRunning Then Return
        _waitRunning = True
        While _isArmed AndAlso _timer.ElapsedTime < _period
            Await _timer.At(_period)
        End While
        _waitRunning = False
        If _isArmed Then
            _isArmed = False
            RaiseEvent Triggered(Me)
        End If
    End Sub
    ''' <summary>
    ''' Resets the countdown timer.
    ''' No effect if the timer is stopped.
    ''' </summary>
    Public Function QueueReset() As Task
        Contract.Ensures(Contract.Result(Of Task)() IsNot Nothing)
        Return inQueue.QueueAction(AddressOf Reset)
    End Function
    Private Sub Reset()
        If _timer.ElapsedTime < _period Then '[Prevent Resets occuring after the timeout from racing the OnTimeout callback]
            _timer = _timer.Restarted
        End If
    End Sub
    ''' <summary>
    ''' Cancels the countdown timer.
    ''' No effect if the timer is already stopped.
    ''' </summary>
    Public Function QueueDisarm() As Task
        Contract.Ensures(Contract.Result(Of Task)() IsNot Nothing)
        Return inQueue.QueueAction(AddressOf Disarm)
    End Function
    Private Sub Disarm()
        _isArmed = False
    End Sub

    Public Overrides Function ToString() As String
        If _isArmed Then
            Return "Armed: {0} remaining out of {1}".Frmt({0.Seconds, _timer.ElapsedTime - _period}.AssumeAny().Max, _period)
        Else
            Return "Disarmed: {0}".Frmt(_period)
        End If
    End Function
End Class
