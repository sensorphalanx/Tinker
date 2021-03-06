Namespace Bot
    Public NotInheritable Class PluginProfile
        Public name As InvariantString
        Public location As InvariantString
        Public argument As String
        Private Const FormatVersion As UInteger = 0

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(argument IsNot Nothing)
        End Sub

        Public Sub New(name As InvariantString, location As InvariantString, argument As String)
            Contract.Requires(argument IsNot Nothing)
            Me.name = name
            Me.location = location
            Me.argument = argument
        End Sub
        Public Sub New(reader As IO.BinaryReader)
            Contract.Requires(reader IsNot Nothing)
            Dim ver = reader.ReadUInt32()
            If ver > FormatVersion Then Throw New IO.InvalidDataException("Saved PlayerRecord has an unrecognized format version.")
            name = reader.ReadString()
            location = reader.ReadString()
            argument = reader.ReadString()
        End Sub
        Public Sub Save(writer As IO.BinaryWriter)
            Contract.Requires(writer IsNot Nothing)
            writer.Write(CUInt(FormatVersion))
            writer.Write(name)
            writer.Write(location)
            writer.Write(argument)
        End Sub
    End Class
End Namespace
