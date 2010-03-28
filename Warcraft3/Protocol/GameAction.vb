﻿Imports Tinker.Pickling

Namespace WC3.Protocol
    <DebuggerDisplay("{ToString}")>
    Public NotInheritable Class GameAction
        Private Shared ReadOnly ActionJar As New KeyPrefixedJar(Of GameActionId)(
            keyJar:=New EnumByteJar(Of GameActionId)(),
            valueJars:=GameActions.AllDefinitions.ToDictionary(
                keySelector:=Function(e) e.Id,
                elementSelector:=Function(e) DirectCast(e.Jar, ISimpleJar).AsNonNull))

        Private ReadOnly _id As GameActionId
        Private ReadOnly _payload As ISimplePickle

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(_payload IsNot Nothing)
        End Sub

        Private Sub New(ByVal id As GameActionId, ByVal payload As ISimplePickle)
            Contract.Requires(payload IsNot Nothing)
            Me._id = id
            Me._payload = payload
        End Sub

        Public Shared Function FromValue(Of T)(ByVal actionDefinition As GameActions.Definition(Of T),
                                               ByVal value As T) As GameAction
            Contract.Requires(actionDefinition IsNot Nothing)
            Contract.Requires(value IsNot Nothing)
            Contract.Ensures(Contract.Result(Of GameAction)() IsNot Nothing)
            Return New GameAction(actionDefinition.Id, actionDefinition.Jar.Pack(value))
        End Function

        Public ReadOnly Property Id As GameActionId
            Get
                Return _id
            End Get
        End Property
        Public ReadOnly Property Payload As ISimplePickle
            Get
                Contract.Ensures(Contract.Result(Of ISimplePickle)() IsNot Nothing)
                Return _payload
            End Get
        End Property

        <ContractVerification(False)>
        Public Shared Function FromData(ByVal data As IReadableList(Of Byte)) As GameAction
            Contract.Requires(data IsNot Nothing)
            Contract.Ensures(Contract.Result(Of GameAction)() IsNot Nothing)
            Dim pickle = ActionJar.Parse(data)
            Return New GameAction(pickle.Value.Key, pickle.Value.Value)
        End Function

        Public Overrides Function ToString() As String
            Return "{0}: {1}".Frmt(id, Payload.Description.Value())
        End Function
    End Class

    Public NotInheritable Class GameActionJar
        Inherits BaseJar(Of GameAction)

        Public Overrides Function Pack(Of TValue As GameAction)(ByVal value As TValue) As IPickle(Of TValue)
            Contract.Assume(value IsNot Nothing)
            Return value.Pickled(Me, New Byte() {value.Id}.Concat(value.Payload.Data).ToReadableList)
        End Function

        'verification disabled due to stupid verifier (1.2.30118.5)
        <ContractVerification(False)>
        Public Overrides Function Parse(ByVal data As IReadableList(Of Byte)) As IPickle(Of GameAction)
            Dim value = GameAction.FromData(data)
            Dim datum = data.SubView(0, value.Payload.Data.Count + 1) 'include the id
            Return value.Pickled(Me, datum)
        End Function
    End Class
End Namespace
