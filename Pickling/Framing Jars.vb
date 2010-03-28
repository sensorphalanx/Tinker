﻿Namespace Pickling
    '''<summary>Pickles values with data of a specified size.</summary>
    Public NotInheritable Class FixedSizeFramingJar(Of T)
        Inherits BaseJar(Of T)

        Private ReadOnly _subJar As IJar(Of T)
        Private ReadOnly _dataSize As Integer

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(_subJar IsNot Nothing)
            Contract.Invariant(_dataSize >= 0)
        End Sub

        Public Sub New(ByVal subJar As IJar(Of T),
                       ByVal dataSize As Integer)
            Contract.Requires(subJar IsNot Nothing)
            Contract.Requires(dataSize >= 0)
            Me._subJar = subJar
            Me._dataSize = dataSize
        End Sub

        Public Overrides Function Pack(Of TValue As T)(ByVal value As TValue) As IPickle(Of TValue)
            Dim pickle = _subJar.Pack(value)
            If pickle.Data.Count <> _dataSize Then Throw New PicklingException("Packed data did not take exactly {0} bytes.".Frmt(_dataSize))
            Return pickle
        End Function

        Public Overrides Function Parse(ByVal data As IReadableList(Of Byte)) As IPickle(Of T)
            If data.Count < _dataSize Then Throw New PicklingNotEnoughDataException()
            Dim result As IPickle(Of T)
            Try
                result = _subJar.Parse(data.SubView(0, _dataSize))
            Catch ex As PicklingException
                '[Only wrap the exception as 'too limited data' if allowing all data causes it to go away]
                Try
                    Dim pickle = _subJar.Parse(data)
                    Throw New PicklingException("Pickled value could not be parsed from limited data.", ex)
                Catch exIgnored As PicklingException
                End Try
                Throw
            End Try
            If result.Data.Count <> _dataSize Then Throw New PicklingException("Parsed value did not use exactly {0} bytes.".Frmt(_dataSize))
            Return result
        End Function

        Public Overrides Function ValueToControl(ByVal value As T) As Control
            Return _subJar.ValueToControl(value)
        End Function
        Public Overrides Function ControlToValue(ByVal control As Control) As T
            Return _subJar.ControlToValue(control)
        End Function
    End Class

    '''<summary>Pickles values with data up to a maximum size.</summary>
    Public Class LimitedSizeFramingJar(Of T)
        Inherits BaseJar(Of T)

        Private ReadOnly _subJar As IJar(Of T)
        Private ReadOnly _maxDataCount As Integer

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(_subJar IsNot Nothing)
            Contract.Invariant(_maxDataCount >= 0)
        End Sub

        Public Sub New(ByVal subJar As IJar(Of T),
                       ByVal maxDataCount As Integer)
            Contract.Requires(subJar IsNot Nothing)
            Contract.Requires(maxDataCount >= 0)
            Me._subJar = subJar
            Me._maxDataCount = maxDataCount
        End Sub

        Public Overrides Function Pack(Of TValue As T)(ByVal value As TValue) As IPickle(Of TValue)
            Dim pickle = _subJar.Pack(value)
            If pickle.Data.Count > _maxDataCount Then Throw New PicklingException("Packed data did not fit in {0} bytes.".Frmt(_maxDataCount))
            Return pickle
        End Function

        Public Overrides Function Parse(ByVal data As IReadableList(Of Byte)) As IPickle(Of T)
            Try
                Return _subJar.Parse(data.SubView(0, Math.Min(data.Count, _maxDataCount)))
            Catch ex As PicklingException
                '[Only wrap the exception as 'too limited data' if allowing all data causes it to go away]
                Try
                    Dim pickle = _subJar.Parse(data)
                    Throw New PicklingException("Pickled value could not be parsed from limited data.", ex)
                Catch exIgnored As PicklingException
                End Try
                Throw
            End Try
        End Function

        Public Overrides Function ValueToControl(ByVal value As T) As Control
            Return _subJar.ValueToControl(value)
        End Function
        Public Overrides Function ControlToValue(ByVal control As Control) As T
            Return _subJar.ControlToValue(control)
        End Function
    End Class

    '''<summary>Pickles values with data prefixed by a count of the number of bytes (not counting the prefix).</summary>
    Public NotInheritable Class SizePrefixedFramingJar(Of T)
        Inherits BaseJar(Of T)

        Private ReadOnly _subJar As IJar(Of T)
        Private ReadOnly _prefixSize As Integer

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(_subJar IsNot Nothing)
            Contract.Invariant(_prefixSize > 0)
            Contract.Invariant(_prefixSize <= 8)
        End Sub

        Public Sub New(ByVal subJar As IJar(Of T),
                       ByVal prefixSize As Integer)
            Contract.Requires(prefixSize > 0)
            Contract.Requires(subJar IsNot Nothing)
            If prefixSize > 8 Then Throw New ArgumentOutOfRangeException("prefixSize", "prefixSize must be less than or equal to 8.")
            Me._subJar = subJar
            Me._prefixSize = prefixSize
        End Sub

        Public Overrides Function Pack(Of TValue As T)(ByVal value As TValue) As IPickle(Of TValue)
            Dim pickle = _subJar.Pack(value)
            Dim sizeBytes = CULng(pickle.Data.Count).Bytes.Take(_prefixSize)
            If sizeBytes.Take(_prefixSize).ToUValue <> pickle.Data.Count Then Throw New PicklingException("Unable to fit byte count into size prefix.")
            Dim data = sizeBytes.Concat(pickle.Data).ToReadableList
            Return pickle.With(jar:=Me, data:=data)
        End Function

        <ContractVerification(False)>
        Public Overrides Function Parse(ByVal data As IReadableList(Of Byte)) As IPickle(Of T)
            If data.Count < _prefixSize Then Throw New PicklingNotEnoughDataException()
            Dim dataSize = data.SubView(0, _prefixSize).ToUValue
            If data.Count < _prefixSize + dataSize Then Throw New PicklingNotEnoughDataException()

            Dim datum = data.SubView(0, CInt(_prefixSize + dataSize))
            Dim pickle = _subJar.Parse(datum.SubView(_prefixSize))
            If pickle.Data.Count < dataSize Then Throw New PicklingException("Fragmented data.")
            Return pickle.With(jar:=Me, data:=datum)
        End Function

        Public Overrides Function ValueToControl(ByVal value As T) As Control
            Return _subJar.ValueToControl(value)
        End Function
        Public Overrides Function ControlToValue(ByVal control As Control) As T
            Return _subJar.ControlToValue(control)
        End Function
    End Class

    '''<summary>Pickles lists of values, where the serialized form is prefixed by the number of items.</summary>
    Public NotInheritable Class ItemCountPrefixedFramingJar(Of T)
        Inherits BaseJar(Of IReadableList(Of T))
        Private ReadOnly _subJar As IJar(Of T)
        Private ReadOnly _prefixSize As Integer
        Private ReadOnly _useSingleLineDescription As Boolean

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(_prefixSize > 0)
            Contract.Invariant(_prefixSize <= 8)
            Contract.Invariant(_subJar IsNot Nothing)
        End Sub

        Public Sub New(ByVal subJar As IJar(Of T),
                       ByVal prefixSize As Integer,
                       Optional ByVal useSingleLineDescription As Boolean = False)
            Contract.Requires(subJar IsNot Nothing)
            Contract.Requires(prefixSize > 0)
            If prefixSize > 8 Then Throw New ArgumentOutOfRangeException("prefixSize", "prefixSize must be less than or equal to 8.")
            Me._subJar = subJar
            Me._prefixSize = prefixSize
            Me._useSingleLineDescription = useSingleLineDescription
        End Sub

        Public Overrides Function Pack(Of TValue As IReadableList(Of T))(ByVal value As TValue) As IPickle(Of TValue)
            Contract.Assume(value IsNot Nothing)
            Dim pickles = (From e In value Select _subJar.Pack(e)).Cache
            Dim sizeData = CULng(value.Count).Bytes.Take(_prefixSize)
            Dim pickleData = Concat(From p In pickles Select (p.Data))
            Dim data = Concat(sizeData, pickleData).ToReadableList
            Return value.Pickled(Me, data, Function() pickles.MakeListDescription(_useSingleLineDescription))
        End Function

        Public Overrides Function Parse(ByVal data As IReadableList(Of Byte)) As IPickle(Of IReadableList(Of T))
            'Parse
            Dim pickles = New List(Of IPickle(Of T))
            Dim curOffset = 0
            'List Size
            If data.Count < _prefixSize Then Throw New PicklingNotEnoughDataException()
            Dim numElements = data.SubView(0, _prefixSize).ToUValue
            curOffset += _prefixSize
            'List Elements
            For repeat = 1UL To numElements
                'Value
                Dim p = _subJar.Parse(data.SubView(curOffset, data.Count - curOffset))
                pickles.Add(p)
                'Size
                Dim n = p.Data.Count
                curOffset += n
                If curOffset > data.Count Then Throw New InvalidStateException("Subjar '{0}' reported taking more data than was available.".Frmt(_subJar.GetType.Name))
            Next repeat

            Dim value = (From p In pickles Select (p.Value)).ToReadableList
            Dim datum = data.SubView(0, curOffset)
            Dim desc = Function() pickles.MakeListDescription(_useSingleLineDescription)
            Return value.Pickled(Me, datum, desc)
        End Function

        Public Overrides Function ValueToControl(ByVal value As IReadableList(Of T)) As Control
            Dim control = New TableLayoutPanel()
            control.ColumnCount = 1
            control.AutoSize = True
            control.AutoSizeMode = AutoSizeMode.GrowAndShrink
            control.BorderStyle = BorderStyle.FixedSingle

            For Each item In value
                Dim c = _subJar.ValueToControl(item)
                control.Controls.Add(c)
                c.Width = control.Width
                c.Anchor = AnchorStyles.Left Or AnchorStyles.Top Or AnchorStyles.Right
            Next item

            Return control
        End Function
        Public Overrides Function ControlToValue(ByVal control As Control) As IReadableList(Of T)
            Return (From i In control.Controls.Count.Range
                    Select _subJar.ControlToValue(control.Controls(i))
                    ).ToReadableList
        End Function
    End Class

    '''<summary>Pickles values with data followed by a null terminator.</summary>
    Public Class NullTerminatedFramingJar(Of T)
        Inherits BaseJar(Of T)

        Private ReadOnly _subJar As IJar(Of T)

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(_subJar IsNot Nothing)
        End Sub

        Public Sub New(ByVal subJar As IJar(Of T))
            Contract.Requires(subJar IsNot Nothing)
            Me._subJar = subJar
        End Sub

        Public Overrides Function Pack(Of TValue As T)(ByVal value As TValue) As IPickle(Of TValue)
            Dim pickle = _subJar.Pack(value)
            Return pickle.With(jar:=Me, data:=pickle.Data.Append(0).ToReadableList)
        End Function

        Public Overrides Function Parse(ByVal data As IReadableList(Of Byte)) As IPickle(Of T)
            'Find terminator
            Dim p = data.IndexOf(0)
            If p < 0 Then Throw New PicklingException("No null terminator found.")
            'Parse
            Dim pickle = _subJar.Parse(data.SubView(0, p))
            If pickle.Data.Count <> p Then Throw New PicklingException("Leftover data before null terminator.")
            Return pickle.With(jar:=Me, data:=data.SubView(0, p + 1))
        End Function

        Public Overrides Function ValueToControl(ByVal value As T) As Control
            Return _subJar.ValueToControl(value)
        End Function
        Public Overrides Function ControlToValue(ByVal control As Control) As T
            Return _subJar.ControlToValue(control)
        End Function
    End Class

    '''<summary>Pickles values which may or may not be included in the data.</summary>
    Public NotInheritable Class OptionalFramingJar(Of T)
        Inherits BaseJar(Of Tuple(Of Boolean, T))

        Private ReadOnly _subJar As IJar(Of T)

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(_subJar IsNot Nothing)
        End Sub

        Public Sub New(ByVal subJar As IJar(Of T))
            Contract.Requires(subJar IsNot Nothing)
            Me._subJar = subJar
        End Sub

        Public Overrides Function Pack(Of TValue As Tuple(Of Boolean, T))(ByVal value As TValue) As IPickle(Of TValue)
            Contract.Assume(value IsNot Nothing)
            If value.Item1 Then
                Contract.Assume(value.Item2 IsNot Nothing)
                Dim pickle = _subJar.Pack(value.Item2)
                Return pickle.With(jar:=Me, value:=value)
            Else
                Return value.Pickled(Me, New Byte() {}.AsReadableList, Function() "[Not Included]")
            End If
        End Function

        <ContractVerification(False)>
        Public Overrides Function Parse(ByVal data As IReadableList(Of Byte)) As IPickle(Of Tuple(Of Boolean, T))
            If data.Count > 0 Then
                Dim pickle = _subJar.Parse(data)
                Return pickle.With(jar:=Me, value:=Tuple.Create(True, pickle.Value))
            Else
                Dim value = Tuple.Create(False, CType(Nothing, T))
                Return value.Pickled(Me, data, Function() "[Not Included]")
            End If
        End Function

        Public Overrides Function ValueToControl(ByVal value As Tuple(Of Boolean, T)) As Control
            Dim control = New TableLayoutPanel()
            control.ColumnCount = 1
            control.AutoSize = True
            control.AutoSizeMode = AutoSizeMode.GrowAndShrink
            control.BorderStyle = BorderStyle.FixedSingle

            If value.Item1 Then
                Dim c = _subJar.ValueToControl(value.Item2)
                control.Controls.Add(c)
                c.Width = control.Width
                c.Anchor = AnchorStyles.Left Or AnchorStyles.Top Or AnchorStyles.Right
            Else
                Dim label = New Label()
                label.Text = "[Not Included]"
                label.AutoSize = True
                control.Controls.Add(label)
            End If

            Return control
        End Function
        Public Overrides Function ControlToValue(ByVal control As Control) As Tuple(Of Boolean, T)
            If TypeOf control.Controls(0) Is Label Then
                Return Tuple.Create(False, DirectCast(Nothing, T))
            Else
                Return Tuple.Create(True, _subJar.ControlToValue(control.Controls(0)))
            End If
        End Function
    End Class

    '''<summary>Pickles values which may be included side-by-side in the data multiple times (including 0 times).</summary>
    Public NotInheritable Class RepeatedFramingJar(Of T)
        Inherits BaseJar(Of IReadableList(Of T))
        Private ReadOnly _subJar As IJar(Of T)
        Private ReadOnly _useSingleLineDescription As Boolean

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(_subJar IsNot Nothing)
        End Sub

        Public Sub New(ByVal subJar As IJar(Of T),
                       Optional ByVal useSingleLineDescription As Boolean = False)
            Contract.Requires(subJar IsNot Nothing)
            Me._subJar = subJar
            Me._useSingleLineDescription = useSingleLineDescription
        End Sub

        Public Overrides Function Pack(Of TValue As IReadableList(Of T))(ByVal value As TValue) As IPickle(Of TValue)
            Contract.Assume(value IsNot Nothing)
            Dim pickles = (From e In value Select CType(_subJar.Pack(e), IPickle(Of T))).Cache
            Dim data = Concat(From p In pickles Select (p.Data)).ToReadableList
            Return value.Pickled(Me, data, Function() pickles.MakeListDescription(_useSingleLineDescription))
        End Function

        'verification disabled due to stupid verifier (1.2.30118.5)
        <ContractVerification(False)>
        Public Overrides Function Parse(ByVal data As IReadableList(Of Byte)) As IPickle(Of IReadableList(Of T))
            'Parse
            Dim pickles = New List(Of IPickle(Of T))
            Dim curCount = data.Count
            Dim curOffset = 0
            'List Elements
            While curOffset < data.Count
                'Value
                Dim p = _subJar.Parse(data.SubView(curOffset, curCount))
                pickles.Add(p)
                'Size
                Dim n = p.Data.Count
                curCount -= n
                curOffset += n
            End While

            Dim datum = data.SubView(0, curOffset)
            Dim value = (From p In pickles Select (p.Value)).ToReadableList
            Return value.Pickled(Me, datum, Function() pickles.MakeListDescription(_useSingleLineDescription))
        End Function

        Public Overrides Function ValueToControl(ByVal value As IReadableList(Of T)) As Control
            Dim control = New TableLayoutPanel()
            control.ColumnCount = 1
            control.AutoSize = True
            control.AutoSizeMode = AutoSizeMode.GrowAndShrink
            control.BorderStyle = BorderStyle.FixedSingle

            For Each item In value
                Dim c = _subJar.ValueToControl(item)
                control.Controls.Add(c)
                c.Width = control.Width
                c.Anchor = AnchorStyles.Left Or AnchorStyles.Top Or AnchorStyles.Right
            Next item

            Return control
        End Function
        Public Overrides Function ControlToValue(ByVal control As Control) As IReadableList(Of T)
            Return (From i In control.Controls.Count.Range
                    Select _subJar.ControlToValue(control.Controls(i))
                    ).ToReadableList
        End Function
    End Class

    '''<summary>Pickles values with data prefixed by a checksum.</summary>
    Public NotInheritable Class ChecksumPrefixedFramingJar(Of T)
        Inherits BaseJar(Of T)

        Private ReadOnly _subJar As IJar(Of T)
        Private ReadOnly _checksumFunction As Func(Of IReadableList(Of Byte), IReadableList(Of Byte))
        Private ReadOnly _checksumSize As Integer

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(_subJar IsNot Nothing)
            Contract.Invariant(_checksumFunction IsNot Nothing)
            Contract.Invariant(_checksumSize > 0)
        End Sub

        Public Sub New(ByVal subJar As IJar(Of T),
                       ByVal checksumSize As Integer,
                       ByVal checksumFunction As Func(Of IReadableList(Of Byte), IReadableList(Of Byte)))
            Contract.Requires(checksumSize > 0)
            Contract.Requires(subJar IsNot Nothing)
            Contract.Requires(checksumFunction IsNot Nothing)
            Me._subJar = subJar
            Me._checksumSize = checksumSize
            Me._checksumFunction = checksumFunction
        End Sub

        Public Overrides Function Pack(Of TValue As T)(ByVal value As TValue) As IPickle(Of TValue)
            Dim pickle = _subJar.Pack(value)
            Dim checksum = _checksumFunction(pickle.Data)
            Contract.Assume(checksum IsNot Nothing)
            Contract.Assume(checksum.Count = _checksumSize)
            Dim data = checksum.Concat(pickle.Data).ToReadableList
            Return pickle.With(jar:=Me, data:=data)
        End Function

        <ContractVerification(False)>
        Public Overrides Function Parse(ByVal data As IReadableList(Of Byte)) As IPickle(Of T)
            If data.Count < _checksumSize Then Throw New PicklingNotEnoughDataException()
            Dim checksum = data.SubView(0, _checksumSize)
            Dim pickle = _subJar.Parse(data.SubView(_checksumSize))
            If Not _checksumFunction(pickle.Data).SequenceEqual(checksum) Then Throw New PicklingException("Checksum didn't match.")
            Dim datum = data.SubView(0, _checksumSize + pickle.Data.Count)
            Return pickle.With(jar:=Me, data:=datum)
        End Function

        Public Overrides Function ValueToControl(ByVal value As T) As Control
            Return _subJar.ValueToControl(value)
        End Function
        Public Overrides Function ControlToValue(ByVal control As Control) As T
            Return _subJar.ControlToValue(control)
        End Function
    End Class

    '''<summary>Pickles values with reversed data.</summary>
    Public Class ReversedFramingJar(Of T)
        Inherits BaseJar(Of T)

        Private ReadOnly _subJar As IJar(Of T)

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(_subJar IsNot Nothing)
        End Sub

        Public Sub New(ByVal subJar As IJar(Of T))
            Contract.Requires(subJar IsNot Nothing)
            Me._subJar = subJar
        End Sub

        Public Overrides Function Pack(Of TValue As T)(ByVal value As TValue) As IPickle(Of TValue)
            Dim pickle = _subJar.Pack(value)
            Dim data = pickle.Data.Reverse.ToReadableList
            Return pickle.With(jar:=Me, data:=data)
        End Function

        Public Overrides Function Parse(ByVal data As IReadableList(Of Byte)) As IPickle(Of T)
            Dim pickle = _subJar.Parse(data.Reverse.ToReadableList)
            If pickle.Data.Count <> data.Count Then Throw New PicklingException("Leftover reversed data.")
            Return pickle.With(jar:=Me, data:=data)
        End Function

        Public Overrides Function ValueToControl(ByVal value As T) As Control
            Return _subJar.ValueToControl(value)
        End Function
        Public Overrides Function ControlToValue(ByVal control As Control) As T
            Return _subJar.ControlToValue(control)
        End Function
    End Class
End Namespace
