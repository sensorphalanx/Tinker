Namespace Pickling
    '''<summary>The identity jar. Pickles data as itself.</summary>
    Public Class DataJar
        Inherits BaseJar(Of IReadableList(Of Byte))

        Public Overrides Function Pack(Of TValue As IReadableList(Of Byte))(ByVal value As TValue) As IPickle(Of TValue)
            Contract.Assume(value IsNot Nothing)
            Return value.Pickled(Me, value, Function() "[{0}]".Frmt(value.ToHexString))
        End Function

        Public Overrides Function Parse(ByVal data As IReadableList(Of Byte)) As IPickle(Of IReadableList(Of Byte))
            Return data.Pickled(Me, data, Function() "[{0}]".Frmt(data.ToHexString))
        End Function
    End Class
End Namespace
