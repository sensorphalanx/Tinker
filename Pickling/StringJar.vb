Imports System.Text

Namespace Pickling
    '''<summary>Pickles strings using a System.Text.Encoding.</summary>
    Public Class StringJar
        Inherits BaseJar(Of String)

        Private ReadOnly _encoding As Encoding
        Private ReadOnly _minCharCount As Integer
        Private ReadOnly _maxCharCount As Integer?

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(_encoding IsNot Nothing)
            Contract.Invariant(_minCharCount >= 0)
            'Contract.Invariant(Not _maxCharCount.HasValue OrElse _maxCharCount.Value >= _minCharCount)
        End Sub

        Public Sub New(ByVal encoding As Encoding,
                       Optional ByVal minCharCount As Integer = 0,
                       Optional ByVal maxCharCount As Integer? = Nothing)
            Contract.Requires(encoding IsNot Nothing)
            Contract.Requires(minCharCount >= 0)
            Contract.Assume(Not maxCharCount.HasValue OrElse maxCharCount.Value >= minCharCount)
            Me._encoding = encoding
            Me._minCharCount = minCharCount
            Me._maxCharCount = maxCharCount
        End Sub

        Public NotOverridable Overrides Function Pack(Of TValue As String)(ByVal value As TValue) As IPickle(Of TValue)
            Contract.Assume(value IsNot Nothing)
            If value.Length < _minCharCount Then Throw New PicklingException("Need at least {0} characters.".Frmt(_minCharCount))
            If _maxCharCount.HasValue AndAlso value.Length > _maxCharCount Then Throw New PicklingException("Need at most {0} characters.".Frmt(_maxCharCount))
            Dim data = _encoding.GetBytes(value)
            If _encoding.GetChars(data) <> value Then Throw New PicklingException("""{0}"" is not encodable using {1}.".Frmt(value, _encoding.GetType))
            Return value.Pickled(Me, data.AsReadableList, Function() """{0}""".Frmt(value))
        End Function

        Public NotOverridable Overrides Function Parse(ByVal data As IReadableList(Of Byte)) As IPickle(Of String)
            Dim value = New String(_encoding.GetChars(data.ToArray))
            If value.Length < _minCharCount Then Throw New PicklingException("Need at least {0} characters.".Frmt(_minCharCount))
            If _maxCharCount.HasValue AndAlso value.Length > _maxCharCount Then Throw New PicklingException("Need at most {0} characters.".Frmt(_maxCharCount))
            If Not _encoding.GetBytes(value).SequenceEqual(data) Then Throw New PicklingException("[{0}] is not decodable using {1}.".Frmt(data.ToHexString, _encoding.GetType))
            Return value.Pickled(Me, data, Function() """{0}""".Frmt(value))
        End Function
    End Class

    '''<summary>Pickles strings using a System.Text.UTF8Encoding.</summary>
    Public NotInheritable Class UTF8Jar
        Inherits StringJar

        Public Sub New(Optional ByVal minCharCount As Integer = 0,
                       Optional ByVal maxCharCount As Integer? = Nothing)
            MyBase.New(New UTF8Encoding(), minCharCount, maxCharCount)
            Contract.Requires(minCharCount >= 0)
            Contract.Assume(Not maxCharCount.HasValue OrElse maxCharCount.Value >= minCharCount)
        End Sub
    End Class

    '''<summary>Pickles strings using a System.Text.ASCIIEncoding.</summary>
    Public NotInheritable Class ASCIIJar
        Inherits StringJar

        Public Sub New(Optional ByVal minCharCount As Integer = 0,
                       Optional ByVal maxCharCount As Integer? = Nothing)
            MyBase.New(New ASCIIEncoding(), minCharCount, maxCharCount)
            Contract.Requires(minCharCount >= 0)
            Contract.Assume(Not maxCharCount.HasValue OrElse maxCharCount.Value >= minCharCount)
        End Sub
    End Class
End Namespace
