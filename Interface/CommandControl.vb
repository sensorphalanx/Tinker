﻿'Verification disabled because of many warnings in generated code
<ContractVerification(False)>
Public Class CommandControl
    Private _historyPointer As Integer
    Private ReadOnly _history As New List(Of String) From {""}

    Public Event IssuedCommand(sender As CommandControl, argument As String)

    <ContractInvariantMethod()> Private Sub ObjectInvariant()
        Contract.Invariant(_history IsNot Nothing)
        Contract.Invariant(_history.Count > 0)
        Contract.Invariant(_historyPointer >= 0)
        Contract.Invariant(_historyPointer < _history.Count)
    End Sub

    Private Sub txtCommand_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtCommand.KeyDown
        Select Case e.KeyCode
            Case Keys.Enter
                e.Handled = True
                e.SuppressKeyPress = True
                If txtCommand.Text = "" Then Return
                RaiseEvent IssuedCommand(Me, txtCommand.Text)

                _historyPointer = _history.Count
                _history(_historyPointer - 1) = txtCommand.Text
                _history.Add("")
                txtCommand.Text = ""
            Case Keys.Up
                _history(_historyPointer) = txtCommand.Text
                _historyPointer = (_historyPointer - 1).Between(0, _history.Count - 1)
                txtCommand.Text = _history(_historyPointer)
                txtCommand.SelectionStart = txtCommand.TextLength
                e.Handled = True
            Case Keys.Down
                _history(_historyPointer) = txtCommand.Text
                _historyPointer = (_historyPointer + 1).Between(0, _history.Count - 1)
                txtCommand.Text = _history(_historyPointer)
                txtCommand.SelectionStart = txtCommand.TextLength
                e.Handled = True
        End Select
    End Sub

    Private Sub BotWidgetControl_Resize(sender As Object, e As System.EventArgs) Handles Me.Resize
        If Me.Height <> txtCommand.Height Then Me.Height = txtCommand.Height
    End Sub
End Class
