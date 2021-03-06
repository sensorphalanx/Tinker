Imports Tinker.Pickling

Namespace Bnet.Protocol
    Public NotInheritable Class TextHexUInt32Jar
        Inherits BaseFixedSizeJar(Of UInt32)
        Private ReadOnly _digitCount As Byte
        Private ReadOnly _byteOrder As ByteOrder

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(_digitCount > 0)
        End Sub

        Public Sub New(digitCount As Byte,
                       Optional byteOrder As ByteOrder = ByteOrder.LittleEndian)
            Contract.Requires(digitCount > 0)
            Contract.Requires(digitCount <= 8)
            Me._digitCount = digitCount
            Me._byteOrder = byteOrder
        End Sub

        Public Overrides Function Pack(value As UInteger) As IRist(Of Byte)
            Dim digits = value.ToString("x{0}".Frmt(_digitCount), CultureInfo.InvariantCulture).ToAsciiBytes().ToRist()
            Contract.Assume(digits.Count >= _digitCount)
            If digits.Count > _digitCount Then Throw New PicklingException("Value {0} is too large to fit into {1} hex digits.".Frmt(value, _digitCount))
            Select Case _byteOrder
                Case ByteOrder.BigEndian : Return digits  'no change
                Case ByteOrder.LittleEndian : Return digits.Reverse
                Case Else : Throw _byteOrder.MakeImpossibleValueException()
            End Select
            Return digits
        End Function

        Protected Overrides ReadOnly Property DataSize As UInt16
            Get
                Return _digitCount
            End Get
        End Property
        Protected Overrides Function FixedSizeParse(data As IRist(Of Byte)) As UInteger
            Return CUInt(data.ToAsciiChars.FromHexToUInt64(_byteOrder))
        End Function

        Public Overrides Function Parse(text As String) As UInteger
            Return New UInt32Jar().Parse(text)
        End Function
    End Class
End Namespace
