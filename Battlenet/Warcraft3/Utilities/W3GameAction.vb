﻿Namespace Warcraft3
    ''' <source>
    ''' http://www.wc3c.net/tools/specs/W3GActions.txt
    ''' </source>
    Public Enum W3GameActionId As Byte
        PauseGame = &H1
        ResumeGame = &H2
        SetGameSpeed = &H3
        IncreaseGameSpeed = &H4
        DecreaseGameSpeed = &H5
        SaveGameStarted = &H6
        SaveGameFinished = &H7
        _unseen_0x08 = &H8
        _unseen_0x09 = &H9
        _unseen_0x0A = &HA
        _unseen_0x0B = &HB
        _unseen_0x0C = &HC
        _unseen_0x0D = &HD
        _unseen_0x0E = &HE
        _unseen_0x0F = &HF
        SelfOrder = &H10
        PointOrder = &H11
        ObjectOrder = &H12
        DropOrGiveItem = &H13
        FogObjectOrder = &H14
        _unseen_0x15 = &H15
        ChangeSelection = &H16
        AssignGroupHotkey = &H17
        SelectGroupHotkey = &H18
        SelectSubgroup = &H19
        _PreSubSelection = &H1A
        _unknown0x1B = &H1B
        SelectGroundItem = &H1C
        CancelHeroRevive = &H1D
        DequeueBuildingOrder = &H1E
        _unseen_0x1F = &H1F
        CheatFastCooldown = &H20
        _unseen_0x21 = &H21
        CheatInstantDefeat = &H22
        CheatSpeedConstruction = &H23
        CheatFastDeathDecay = &H24
        CheatNoFoodLimit = &H25
        CheatGodMode = &H26
        CheatGold = &H27
        CheatLumber = &H28
        CheatUnlimitedMana = &H29
        CheatNoDefeat = &H2A
        CheatDisableVictoryConditions = &H2B
        CheatEnableResearch = &H2C
        CheatGoldAndLumber = &H2D
        CheatSetTimeOfDay = &H2E
        CheatRemoveFogOfWar = &H2F
        CheatDisableTechRequirements = &H30
        CheatResearchUpgrades = &H31
        CheatInstantVictory = &H32
        _unseen_0x33 = &H33
        _unseen_0x34 = &H34
        _unseen_0x35 = &H35
        _unseen_0x36 = &H36
        _unseen_0x37 = &H37
        _unseen_0x38 = &H38
        _unseen_0x39 = &H39
        _unseen_0x3A = &H3A
        _unseen_0x3B = &H3B
        _unseen_0x3C = &H3C
        _unseen_0x3D = &H3D
        _unseen_0x3E = &H3E
        _unseen_0x3F = &H3F
        _unseen_0x40 = &H40
        _unseen_0x41 = &H41
        _unseen_0x42 = &H42
        _unseen_0x43 = &H43
        _unseen_0x44 = &H44
        _unseen_0x45 = &H45
        _unseen_0x46 = &H46
        _unseen_0x47 = &H47
        _unseen_0x48 = &H48
        _unseen_0x49 = &H49
        _unseen_0x4A = &H4A
        _unseen_0x4B = &H4B
        _unseen_0x4C = &H4C
        _unseen_0x4D = &H4D
        _unseen_0x4E = &H4E
        _unseen_0x4F = &H4F
        ChangeAllyOptions = &H50
        TransferResources = &H51
        _unseen_0x52 = &H52
        _unseen_0x53 = &H53
        _unseen_0x54 = &H54
        _unseen_0x55 = &H55
        _unseen_0x56 = &H56
        _unseen_0x57 = &H57
        _unseen_0x58 = &H58
        _unseen_0x59 = &H59
        _unseen_0x5A = &H5A
        _unseen_0x5B = &H5B
        _unseen_0x5C = &H5C
        _unseen_0x5D = &H5D
        _unseen_0x5E = &H5E
        _unseen_0x5F = &H5F
        TriggerChatEvent = &H60
        PressedEscape = &H61
        TriggerWaitFinished = &H62
        _unseen_0x63 = &H63
        TriggerMouseClickedTrackable = &H64
        TriggerMouseTouchedTrackable = &H65
        EnterChooseHeroSkillSubmenu = &H66
        EnterChooseBuildingSubmenu = &H67
        MinimapPing = &H68
        TriggerDialogButtonClicked2 = &H69
        TriggerDialogButtonClicked = &H6A
        GameCacheSyncInteger = &H6B
        GameCacheSyncReal = &H6C
        GameCacheSyncBoolean = &H6D
        GameCacheSyncString = &H6E 'this is a guess; the SyncStoredString function does nothing so I've never actually seen the packet
        GameCacheSyncUnit = &H6F
        _unseen_0x70 = &H70
        _unseen_0x71 = &H71
        _unseen_0x72 = &H72
        _unseen_0x73 = &H73
        _unseen_0x74 = &H74
        TriggerArrowKeyPressed = &H75
    End Enum

    Public Class W3GameAction
        Public ReadOnly id As W3GameActionId
        Public ReadOnly payload As IPickle(Of Object)
        Public Shared ReadOnly packetJar As PrefixSwitchJar(Of W3GameActionId) = MakeJar()

        Private Sub New(ByVal payload As IPickle(Of PrefixSwitchJar(Of W3GameActionId).PrefixPickle))
            Contract.Requires(payload IsNot Nothing)
            Me.payload = payload.Value.payload
            Me.id = payload.Value.index
        End Sub

        Private Shared Sub reg(ByVal jar As PrefixSwitchJar(Of W3GameActionId),
                               ByVal id As W3GameActionId,
                               ByVal ParamArray subjars() As IJar(Of Object))
            jar.reg(id, New TupleJar("data", subjars).Weaken)
        End Sub

        Public Enum GameSpeedSetting As Byte
            Slow = 0
            Normal = 1
            Fast = 2
        End Enum
        Public Enum OrderFlags As UShort
            Queue = 1 << 0
            Train = 1 << 1
            Construct = 1 << 2
            Group = 1 << 3
            NoFormation = 1 << 4
            unknown_0x0020 = 1 << 5
            SubGroup = 1 << 6
            unknown_0x0080 = 1 << 7
            AutocastOn = 1 << 8
        End Enum
        Public Enum SelectionOperation As Byte
            Add = 1
            Remove = 2
        End Enum
        Public Enum AllianceFlags As UInteger
            Allied = (1 << 0) Or (1 << 1) Or (1 << 2) Or (1 << 3) Or (1 << 4)
            SharedVision = 1 << 5
            SharedControl = 1 << 6
            AlliedVictory = 1 << 10
        End Enum
        Public Enum ArrowKeyButton As Byte
            Left = 0
            Right = 2
            Down = 4
            Up = 6
        End Enum

        Public Enum OrderId As UInteger
            Smart = &HD0003 'right-click
            [Stop] = &HD0004
            SetRallyPoint = &HD000C
            Attack = &HD000F
            AttackGround = &HD0010
            Move = &HD0012
            Patrol = &HD0016
            HoldPosition = &HD0019
            GiveOrDropItem = &HD0021
            SwapItemWithItemInSlot1 = &HD0022
            SwapItemWithItemInSlot2 = &HD0023
            SwapItemWithItemInSlot3 = &HD0024
            SwapItemWithItemInSlot4 = &HD0025
            SwapItemWithItemInSlot5 = &HD0026
            SwapItemWithItemInSlot6 = &HD0027
            UseItemInSlot1 = &HD0028
            UseItemInSlot2 = &HD0029
            UseItemInSlot3 = &HD002A
            UseItemInSlot4 = &HD002B
            UseItemInSlot5 = &HD002C
            UseItemInSlot6 = &HD002D
            ReturnResources = &HD0031
            Gather = &HD0032
            '... many many more ...
        End Enum

        Private Shared Function MakeJar() As PrefixSwitchJar(Of W3GameActionId)
            Dim jar = New PrefixSwitchJar(Of W3GameActionId)("W3GameAction")

            'Game controls
            reg(jar, W3GameActionId.PauseGame)
            reg(jar, W3GameActionId.ResumeGame)
            reg(jar, W3GameActionId.SetGameSpeed,
                        New EnumByteJar(Of GameSpeedSetting)("speed").Weaken)
            reg(jar, W3GameActionId.IncreaseGameSpeed)
            reg(jar, W3GameActionId.DecreaseGameSpeed)
            reg(jar, W3GameActionId.SaveGameStarted,
                        New StringJar("filename").Weaken)
            reg(jar, W3GameActionId.SaveGameFinished,
                        New UInt32Jar("unknown1_0x01").Weaken)

            'Orders
            reg(jar, W3GameActionId.SelfOrder,
                        New EnumUInt16Jar(Of OrderFlags)("flags", flags:=True).Weaken,
                        New OrderIdJar("order id").Weaken,
                        New DoubleObjectIdValueJar("unknown").Weaken)
            reg(jar, W3GameActionId.PointOrder,
                        New EnumUInt16Jar(Of OrderFlags)("flags", flags:=True).Weaken,
                        New OrderIdJar("order id").Weaken,
                        New DoubleObjectIdValueJar("unknown").Weaken,
                        New FloatSingleJar("target x").Weaken,
                        New FloatSingleJar("target y").Weaken)
            reg(jar, W3GameActionId.ObjectOrder,
                        New EnumUInt16Jar(Of OrderFlags)("flags", flags:=True).Weaken,
                        New OrderIdJar("order id").Weaken,
                        New DoubleObjectIdValueJar("unknown").Weaken,
                        New FloatSingleJar("x").Weaken,
                        New FloatSingleJar("y").Weaken,
                        New DoubleObjectIdValueJar("target").Weaken)
            reg(jar, W3GameActionId.DropOrGiveItem,
                        New EnumUInt16Jar(Of OrderFlags)("flags", flags:=True).Weaken,
                        New OrderIdJar("order id").Weaken,
                        New DoubleObjectIdValueJar("unknown").Weaken,
                        New FloatSingleJar("x").Weaken,
                        New FloatSingleJar("y").Weaken,
                        New DoubleObjectIdValueJar("receiver").Weaken,
                        New DoubleObjectIdValueJar("item").Weaken)
            reg(jar, W3GameActionId.FogObjectOrder,
                        New EnumUInt16Jar(Of OrderFlags)("flags", flags:=True).Weaken,
                        New OrderIdJar("order id").Weaken,
                        New DoubleObjectIdValueJar("unknown1").Weaken,
                        New FloatSingleJar("fog target x").Weaken,
                        New FloatSingleJar("fog target y").Weaken,
                        New TypeIdJar("fog target type").Weaken,
                        New ArrayJar("unknown2", expectedSize:=9).Weaken,
                        New FloatSingleJar("actual target x").Weaken,
                        New FloatSingleJar("actual target y").Weaken)

            'Other orders
            reg(jar, W3GameActionId.EnterChooseHeroSkillSubmenu)
            reg(jar, W3GameActionId.EnterChooseBuildingSubmenu)
            reg(jar, W3GameActionId.PressedEscape)
            reg(jar, W3GameActionId.CancelHeroRevive,
                        New DoubleObjectIdValueJar("target").Weaken)
            reg(jar, W3GameActionId.DequeueBuildingOrder,
                        New ByteJar("slot number").Weaken,
                        New TypeIdJar("type").Weaken)
            reg(jar, W3GameActionId.MinimapPing,
                        New FloatSingleJar("x").Weaken,
                        New FloatSingleJar("y").Weaken,
                        New ArrayJar("unknown", expectedSize:=4).Weaken)

            'Alliance
            reg(jar, W3GameActionId.ChangeAllyOptions,
                        New ByteJar("player slot id").Weaken,
                        New EnumUInt32Jar(Of AllianceFlags)("flags", flags:=True).Weaken)
            reg(jar, W3GameActionId.TransferResources,
                        New ByteJar("player slot id").Weaken,
                        New UInt32Jar("gold").Weaken,
                        New UInt32Jar("lumber").Weaken)

            'Selection
            reg(jar, W3GameActionId.ChangeSelection,
                        New EnumByteJar(Of SelectionOperation)("operation", flags:=True).Weaken,
                        New ListJar(Of Object)("targets",
                            New DoubleObjectIdValueJar("target").Weaken, prefixsize:=2).Weaken)
            reg(jar, W3GameActionId.AssignGroupHotkey,
                        New ByteJar("group index").Weaken,
                        New ListJar(Of Object)("targets",
                            New DoubleObjectIdValueJar("target").Weaken, prefixsize:=2).Weaken)
            reg(jar, W3GameActionId.SelectGroupHotkey,
                        New ByteJar("group index").Weaken,
                        New ByteJar("unknown1_0x03").Weaken)
            reg(jar, W3GameActionId.SelectSubgroup,
                        New TypeIdJar("unit type").Weaken,
                        New DoubleObjectIdValueJar("target").Weaken)
            reg(jar, W3GameActionId._PreSubSelection)
            reg(jar, W3GameActionId.SelectGroundItem,
                        New ByteJar("unknown1_0x04").Weaken,
                        New DoubleObjectIdValueJar("target").Weaken)

            'Cheats
            reg(jar, W3GameActionId.CheatDisableTechRequirements)
            reg(jar, W3GameActionId.CheatDisableVictoryConditions)
            reg(jar, W3GameActionId.CheatEnableResearch)
            reg(jar, W3GameActionId.CheatFastCooldown)
            reg(jar, W3GameActionId.CheatFastDeathDecay)
            reg(jar, W3GameActionId.CheatGodMode)
            reg(jar, W3GameActionId.CheatInstantDefeat)
            reg(jar, W3GameActionId.CheatInstantVictory)
            reg(jar, W3GameActionId.CheatNoDefeat)
            reg(jar, W3GameActionId.CheatNoFoodLimit)
            reg(jar, W3GameActionId.CheatRemoveFogOfWar)
            reg(jar, W3GameActionId.CheatResearchUpgrades)
            reg(jar, W3GameActionId.CheatSpeedConstruction)
            reg(jar, W3GameActionId.CheatUnlimitedMana)
            reg(jar, W3GameActionId.CheatSetTimeOfDay,
                        New FloatSingleJar("time").Weaken)
            reg(jar, W3GameActionId.CheatGold,
                        New ByteJar("unknown").Weaken,
                        New UInt32Jar("amount").Weaken)
            reg(jar, W3GameActionId.CheatGoldAndLumber,
                        New ByteJar("unknown").Weaken,
                        New UInt32Jar("amount").Weaken)
            reg(jar, W3GameActionId.CheatLumber,
                        New ByteJar("unknown").Weaken,
                        New UInt32Jar("amount").Weaken)

            'Triggers
            reg(jar, W3GameActionId.TriggerChatEvent,
                        New DoubleObjectIdValueJar("trigger event id").Weaken,
                        New StringJar("text").Weaken)
            reg(jar, W3GameActionId.TriggerWaitFinished,
                        New ObjectIdJar("unknown1").Weaken,
                        New ObjectIdJar("unknown2").Weaken,
                        New UInt32Jar("counter").Weaken)
            reg(jar, W3GameActionId.TriggerMouseTouchedTrackable,
                        New ObjectIdJar("trigger event id").Weaken,
                        New ObjectIdJar("trackable id").Weaken)
            reg(jar, W3GameActionId.TriggerMouseClickedTrackable,
                        New ObjectIdJar("trackable id").Weaken,
                        New ObjectIdJar("trigger event id").Weaken)
            reg(jar, W3GameActionId.TriggerDialogButtonClicked,
                        New DoubleObjectIdValueJar("dialog id").Weaken,
                        New ObjectIdJar("button id").Weaken,
                        New ObjectIdJar("trigger event id").Weaken)
            reg(jar, W3GameActionId.TriggerDialogButtonClicked2,
                        New ObjectIdJar("button id").Weaken,
                        New ObjectIdJar("trigger event id").Weaken,
                        New DoubleObjectIdValueJar("dialog id").Weaken)
            reg(jar, W3GameActionId.TriggerArrowKeyPressed,
                        New EnumByteJar(Of ArrowKeyButton)("button").Weaken)

            'Game Cache
            reg(jar, W3GameActionId.GameCacheSyncInteger,
                        New StringJar("filename").Weaken,
                        New StringJar("mission key").Weaken,
                        New StringJar("key").Weaken,
                        New UInt32Jar("value").Weaken)
            reg(jar, W3GameActionId.GameCacheSyncBoolean,
                        New StringJar("filename").Weaken,
                        New StringJar("mission key").Weaken,
                        New StringJar("key").Weaken,
                        New UInt32Jar("value").Weaken)
            reg(jar, W3GameActionId.GameCacheSyncReal,
                        New StringJar("filename").Weaken,
                        New StringJar("mission key").Weaken,
                        New StringJar("key").Weaken,
                        New FloatSingleJar("value").Weaken)
            reg(jar, W3GameActionId.GameCacheSyncUnit,
                        New StringJar("filename").Weaken,
                        New StringJar("mission key").Weaken,
                        New StringJar("key").Weaken,
                        New TypeIdJar("unit type").Weaken,
                        New ArrayJar("unknown data", expectedSize:=86).Weaken)
            reg(jar, W3GameActionId.GameCacheSyncString,
                        New StringJar("filename").Weaken,
                        New StringJar("mission key").Weaken,
                        New StringJar("key").Weaken,
                        New StringJar("value").Weaken) 'this is a guess based on the other syncs; I've never actually seen this packet.

            'unclassified
            reg(jar, W3GameActionId._unknown0x1B,
                        New ByteJar("unknown0x01").Weaken,
                        New UInt32Jar("unknown2").Weaken,
                        New UInt32Jar("unknown3").Weaken)

            Return jar
        End Function

        Public Shared Function FromData(ByVal data As ViewableList(Of Byte)) As W3GameAction
            Contract.Requires(data IsNot Nothing)
            Contract.Ensures(Contract.Result(Of W3GameAction)() IsNot Nothing)
            Return New W3GameAction(packetJar.Parse(data))
        End Function

        Private Class DoubleObjectIdValueJar
            Inherits Jar(Of IdPair)

            Public Structure IdPair
                Public ReadOnly AllocatedId As UInteger
                Public ReadOnly Unknown As UInteger
                Public Sub New(ByVal allocatedId As UInteger, ByVal unknown As UInteger)
                    Me.AllocatedId = allocatedId
                    Me.Unknown = unknown
                End Sub
            End Structure

            Public Sub New(ByVal name As String)
                MyBase.New(name)
                Contract.Requires(name IsNot Nothing)
            End Sub

            Public NotOverridable Overrides Function Pack(Of R As IdPair)(ByVal value As R) As IPickle(Of R)
                Dim valued As IdPair = value
                Dim data = Concat(valued.AllocatedId.Bytes(ByteOrder.LittleEndian), valued.Unknown.Bytes(ByteOrder.LittleEndian)).ToView
                Return New Pickle(Of R)(Me.Name, value, data, Function() ValueToString(valued))
            End Function

            Public NotOverridable Overrides Function Parse(ByVal data As ViewableList(Of Byte)) As IPickle(Of IdPair)
                data = data.SubView(0, 8)
                Dim value = New IdPair(data.SubView(0, 4).ToUInt32(ByteOrder.LittleEndian),
                                       data.SubView(4, 4).ToUInt32(ByteOrder.LittleEndian))
                Return New Pickle(Of IdPair)(Me.Name, value, data, Function() ValueToString(value))
            End Function

            Protected Overridable Function ValueToString(ByVal value As IdPair) As String
                If value.AllocatedId = UInt32.MaxValue AndAlso value.Unknown = UInt32.MaxValue Then Return "[none]"
                If value.AllocatedId = value.Unknown Then Return value.AllocatedId.ToString
                Return "{0}, unknown={1}".Frmt(value.AllocatedId, value.Unknown)
            End Function
        End Class
        Private Class OrderIdJar
            Inherits EnumUInt32Jar(Of OrderId)

            Public Sub New(ByVal name As String)
                MyBase.New(name)
                Contract.Requires(name IsNot Nothing)
            End Sub

            Protected Overrides Function ValueToString(ByVal value As OrderId) As String
                If value >= &HD0000 AndAlso value < &HE0000 Then Return MyBase.ValueToString(value)
                Return TypeIdJar.IdString(value)
            End Function
        End Class
        Private Class ObjectIdJar
            Inherits UInt32Jar

            Public Sub New(ByVal name As String)
                MyBase.New(name)
                Contract.Requires(name IsNot Nothing)
            End Sub

            Protected Overrides Function ValueToString(ByVal value As UInteger) As String
                If value = UInt32.MaxValue Then Return "[none]"
                Return value.ToString
            End Function
        End Class
        Private Class TypeIdJar
            Inherits UInt32Jar

            Public Sub New(ByVal name As String)
                MyBase.New(name)
                Contract.Requires(name IsNot Nothing)
            End Sub

            Public Shared Function IdString(ByVal value As UInteger) As String
                Dim bytes = value.Bytes(ByteOrder.LittleEndian)
                Dim id = ""
                For i = 0 To bytes.Length - 1
                    If bytes(i) >= 32 And bytes(i) < 128 Then
                        id = Chr(bytes(i)) + id
                    Else
                        id = "."c + id
                    End If
                Next i

                Return "'{0}' = {1}".Frmt(id, bytes.ToHexString)
            End Function
            Protected Overrides Function ValueToString(ByVal value As UInteger) As String
                Return IdString(value)
            End Function
        End Class

        Public Overrides Function ToString() As String
            Return "{0} = {1}".Frmt(id, payload.Description.Value())
        End Function
    End Class

    Public Class W3GameActionJar
        Inherits Jar(Of W3GameAction)
        Public Sub New(ByVal name As String)
            MyBase.New(name)
        End Sub

        Public Overrides Function Pack(Of R As W3GameAction)(ByVal value As R) As Pickling.IPickle(Of R)
            Return New Pickle(Of R)(Name, value, Concat({value.id}, value.payload.Data.ToArray).ToView)
        End Function

        Public Overrides Function Parse(ByVal data As ViewableList(Of Byte)) As Pickling.IPickle(Of W3GameAction)
            Dim val = W3GameAction.FromData(data)
            Dim n = val.payload.Data.Length
            Return New Pickle(Of W3GameAction)(Name, val, data.SubView(0, n + 1))
        End Function
    End Class
End Namespace