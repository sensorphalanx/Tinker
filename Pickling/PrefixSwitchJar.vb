Namespace Pickling
    Public NotInheritable Class PrefixPickle(Of T)
        Private ReadOnly _key As T
        Private ReadOnly _payload As IPickle(Of Object)

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(_key IsNot Nothing)
            Contract.Invariant(_payload IsNot Nothing)
        End Sub

        Public Sub New(ByVal key As T, ByVal payload As IPickle(Of Object))
            Contract.Requires(payload IsNot Nothing)
            Me._key = key
            Me._payload = payload
        End Sub
        Public ReadOnly Property Key As T
            Get
                Contract.Ensures(Contract.Result(Of T)() IsNot Nothing)
                Return _key
            End Get
        End Property
        Public ReadOnly Property Payload As IPickle(Of Object)
            Get
                Contract.Ensures(Contract.Result(Of IPickle(Of Object))() IsNot Nothing)
                Return _payload
            End Get
        End Property
    End Class
    Public NotInheritable Class PrefixSwitchJar(Of T)
        Inherits BaseJar(Of PrefixPickle(Of T))
        Private ReadOnly packers(0 To 255) As IPackJar(Of Object)
        Private ReadOnly parsers(0 To 255) As IParseJar(Of Object)

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(packers IsNot Nothing)
            Contract.Invariant(parsers IsNot Nothing)
        End Sub

        Public Sub New(ByVal name As InvariantString)
            MyBase.new(name)
        End Sub

        Public Overrides Function Parse(ByVal data As IReadableList(Of Byte)) As IPickle(Of PrefixPickle(Of T))
            Dim index = CByte(data(0))
            Dim vindex = CType(CType(index, Object), T)
            If parsers(index) Is Nothing Then Throw New PicklingException("No parser registered to " + vindex.ToString())
            Dim payload = New PrefixPickle(Of T)(vindex, parsers(index).Parse(data.SubView(1)))
            Return New Pickle(Of PrefixPickle(Of T))(Name, payload, data.SubView(0, payload.payload.Data.Count + 1))
        End Function
        'verification disabled due to stupid verifier
        <ContractVerification(False)>
        Public Overrides Function Pack(Of TValue As PrefixPickle(Of T))(ByVal value As TValue) As IPickle(Of TValue)
            Dim index = CByte(CType(value.Key, Object))
            If packers(index) Is Nothing Then Throw New PicklingException("No packer registered to " + value.Key.ToString())
            Return New Pickle(Of TValue)(Name, value, Concat({index}, packers(index).Pack(value.payload.Value).Data.ToArray).AsReadableList)
        End Function

        Public Sub AddPackerParser(ByVal index As Byte, ByVal jar As IJar(Of Object))
            Contract.Requires(jar IsNot Nothing)
            If parsers(index) IsNot Nothing Then Throw New InvalidOperationException("Parser already registered to index {0}.".Frmt(index))
            If packers(index) IsNot Nothing Then Throw New InvalidOperationException("Packer already registered to index {0}.".Frmt(index))
            parsers(index) = jar
            packers(index) = jar
        End Sub
        Public Sub AddParser(ByVal index As Byte, ByVal parser As IParseJar(Of Object))
            Contract.Requires(parser IsNot Nothing)
            If parsers(index) IsNot Nothing Then Throw New InvalidOperationException("Parser already registered to index {0}.".Frmt(index))
            parsers(index) = parser
        End Sub
        Public Sub AddPacker(ByVal index As Byte, ByVal packer As IPackJar(Of Object))
            Contract.Requires(packer IsNot Nothing)
            If packers(index) IsNot Nothing Then Throw New InvalidOperationException("Packer already registered to index {0}.".Frmt(index))
            packers(index) = packer
        End Sub
    End Class
End Namespace
