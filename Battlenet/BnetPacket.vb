''HostBot - Warcraft 3 game hosting bot
''Copyright (C) 2008 Craig Gidney
''
''This program is free software: you can redistribute it and/or modify
''it under the terms of the GNU General Public License as published by
''the Free Software Foundation, either version 3 of the License, or
''(at your option) any later version.
''
''This program is distributed in the hope that it will be useful,
''but WITHOUT ANY WARRANTY; without even the implied warranty of
''MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
''GNU General Public License for more details.
''You should have received a copy of the GNU General Public License
''along with this program.  If not, see http://www.gnu.org/licenses/

Namespace Bnet
    '''<summary>Header values for packets to/from BNET</summary>
    '''<source>BNETDocs.org</source>
    Public Enum PacketId As Byte
        Null = &H0
        '''<summary>Client requests server close the client's listed game.</summary>
        CloseGame3 = &H2
        ServerList = &H4
        ClientId = &H5
        StartVersioning = &H6
        ReportVersion = &H7
        StartAdvex = &H8
        QueryGamesList = &H9
        '''<summary>Request/response for entering chat.</summary>
        EnterChat = &HA
        GetChannelList = &HB
        '''<summary>Request/response for joining a channel ... duh</summary>
        JoinChannel = &HC
        ChatCommand = &HE
        ChatEvent = &HF
        LeaveChat = &H10
        LocaleInfo = &H12
        FloodDetected = &H13
        UdpPingResponse = &H14
        CheckAd = &H15
        ClickAd = &H16
        Registry = &H18
        MessageBox = &H19
        StartAdvex2 = &H1A
        GameDataAddress = &H1B
        '''<summary>Request/response for listing a game.</summary>
        CreateGame3 = &H1C
        LogOnChallengeEx = &H1D
        ClientId2 = &H1E
        LeaveGame = &H1F
        DisplayAd = &H21
        NotifyJoin = &H22
        Ping = &H25
        ReadUserData = &H26
        WriteUserData = &H27
        LogOnChallenge = &H28
        LogOnResponse = &H29
        CreateAccount = &H2A
        SystemInfo = &H2B
        GameResult = &H2C
        GetIconData = &H2D
        GetLadderData = &H2E
        FindLadderUser = &H2F
        CDKey = &H30
        ChangePassword = &H31
        CheckDataFile = &H32
        GetFileTime = &H33
        QueryRealms = &H34
        Profile = &H35
        CDKey2 = &H36
        LogOnResponse2 = &H3A
        CheckDataFile2 = &H3C
        CreateAccount2 = &H3D
        LogOnRealmEx = &H3E
        StartVersioning2 = &H3F
        QueryRealms2 = &H40
        QueryAdUrl = &H41
        WarcraftGeneral = &H44
        'client: ff 44 39 00; 06; 01 00 00 00; e8 21 00 0b; b0 0b e6 4a; 02 88 90 96; e8 45 0e dc; af a0 b3 05; 1b a5 43 d7; be c6 a7 70; 7c; 00 00 00 00; 01 00; ff 0f 00 00; 08; 6c 96 dc 19; 02 00 00 00

        '''<summary>Client tells server what port it will listen for other clients on when hosting games.</summary>
        NetGamePort = &H45
        NewsInfo = &H46
        OptionalWork = &H4A
        ExtraWork = &H4B
        RequiredWork = &H4C
        Tournament = &H4E
        '''<summary>Introductions, server authentication, and server challenge to client.</summary>
        AuthenticationBegin = &H50
        '''<summary>The client authenticates itself against the server's challenge</summary>
        AuthenticationFinish = &H51
        AccountCreate = &H52
        AccountLogOnBegin = &H53
        '''<summary>Exchange of password proofs</summary>
        AccountLogOnFinish = &H54
        AccountChange = &H55
        AccountChangeProof = &H56
        AccountUpgrade = &H57
        AccountUpgradeProof = &H58
        AccountSetEmail = &H59
        AccountResetPassword = &H5A
        AccountChangeEmail = &H5B
        SwitchProduct = &H5C
        Warden = &H5E

        ArrangedTeamPlayerList = &H60

        ArrangedTeamInvitePlayers = &H61
        ArrangedTeamInvitation = &H63

        FriendsList = &H65
        FriendsUpdate = &H66
        FriendsAdd = &H67
        FriendsRemove = &H68
        FriendsPosition = &H69
        ClanFindCandidates = &H70
        ClanInviteMultiple = &H71
        ClanCreationInvitation = &H72
        ClanDisband = &H73
        ClanMakeChieftain = &H74
        ClanInfo = &H75
        ClanQuitNotify = &H76
        ClanInvitation = &H77
        ClanRemoveMember = &H78
        ClanInvitationResponse = &H79
        ClanRankChange = &H7A
        ClanSetMessageOfTheDay = &H7B
        ClanMessageOfTheDay = &H7C
        ClanMemberList = &H7D
        ClanMemberRemoved = &H7E
        ClanMemberStatusChange = &H7F
        ClanMemberRankChange = &H81
        ClanMemberInformation = &H82
    End Enum

    Public NotInheritable Class Packet
        Public Const PacketPrefixValue As Byte = &HFF
        Private ReadOnly _payload As IPickle(Of Object)
        Public ReadOnly id As PacketId
        Private Shared ReadOnly packetJar As ManualSwitchJar = MakeBnetPacketJar()
        Public ReadOnly Property Payload As IPickle(Of Object)
            Get
                Contract.Ensures(Contract.Result(Of IPickle(Of Object))() IsNot Nothing)
                Return _payload
            End Get
        End Property

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(_payload IsNot Nothing)
        End Sub

        Private Sub New(ByVal id As PacketId, ByVal payload As IPickle(Of Object))
            Contract.Requires(payload IsNot Nothing)
            Me._payload = payload
            Me.id = id
        End Sub
        Private Sub New(ByVal id As PacketId, ByVal value As Object)
            Me.New(id, packetJar.Pack(id, value))
            Contract.Requires(value IsNot Nothing)
        End Sub
        Public Shared Function FromData(ByVal id As PacketId, ByVal data As ViewableList(Of Byte)) As Packet
            Contract.Requires(data IsNot Nothing)
            Contract.Ensures(Contract.Result(Of Packet)() IsNot Nothing)
            Return New Packet(id, packetJar.Parse(id, data))
        End Function

#Region "Enums"
        Public Enum AuthenticationBeginLogOnType As Byte
            Warcraft3 = 2
        End Enum
        Public Enum AccountLogOnFinishResult As UInteger
            Passed = 0
            IncorrectPassword = 2
            NeedEmail = 14
            CustomError = 15
        End Enum
        Public Enum AccountLogOnBeginResult As UInteger
            Passed = 0
            BadUserName = 1
            UpgradeAccount = 5
        End Enum
        Public Enum AuthenticationFinishResult As UInteger
            Passed = 0
            OldVersion = &H101
            InvalidVersion = &H102
            FutureVersion = &H103
            InvalidCDKey = &H200
            UsedCDKey = &H201
            BannedCDKey = &H202
            WrongProduct = &H203
        End Enum
        Public Enum ChatEventId
            ShowUser = &H1
            UserJoined = &H2
            UserLeft = &H3
            Whisper = &H4
            Talk = &H5
            Broadcast = &H6
            Channel = &H7
            UserFlags = &H9
            WhisperSent = &HA
            ChannelFull = &HD
            ChannelDoesNotExist = &HE
            ChannelRestricted = &HF
            Info = &H12
            Errors = &H13
            Emote = &H17
        End Enum
        <Flags()>
        Public Enum GameStates As UInteger
            [Private] = 1 << 0
            Full = 1 << 1
            NotEmpty = 1 << 2 'really unsure about this one
            InProgress = 1 << 3
            Unknown0x10 = 1 << 4
        End Enum
        Public Enum JoinChannelType As UInteger
            NoCreate = 0
            FirstJoin = 1
            ForcedJoin = 2
            Diablo2Join = 3
        End Enum
#End Region

#Region "Definition"
        Private Shared Sub regPack(ByVal jar As ManualSwitchJar,
                                   ByVal id As PacketId,
                                   ByVal ParamArray subJars() As IPackJar(Of Object))
            Contract.Requires(jar IsNot Nothing)
            Contract.Requires(subJars IsNot Nothing)
            jar.AddPacker(id, New TuplePackJar(id.ToString(), subJars).Weaken)
        End Sub
        Private Shared Sub regParse(ByVal jar As ManualSwitchJar,
                                    ByVal id As PacketId,
                                    ByVal ParamArray subJars() As IParseJar(Of Object))
            Contract.Requires(jar IsNot Nothing)
            Contract.Requires(subJars IsNot Nothing)
            jar.AddParser(id, New TupleParseJar(id.ToString(), subJars))
        End Sub

        Private Shared Function MakeBnetPacketJar() As ManualSwitchJar
            Contract.Ensures(Contract.Result(Of ManualSwitchJar)() IsNot Nothing)
            Dim jar As New ManualSwitchJar

            'Connection
            regPack(jar, PacketId.AuthenticationBegin,
                    New UInt32Jar("protocol").Weaken,
                    New StringJar("platform", False, True, 4).Weaken,
                    New StringJar("product", False, True, 4).Weaken,
                    New UInt32Jar("product version").Weaken,
                    New StringJar("product language", False, , 4).Weaken,
                    New WC3.IPAddressJar("internal ip").Weaken,
                    New UInt32Jar("time zone offset").Weaken,
                    New UInt32Jar("location id").Weaken,
                    New UInt32Jar("language id").Weaken,
                    New StringJar("country abrev").Weaken,
                    New StringJar("country name").Weaken)
            regParse(jar, PacketId.AuthenticationBegin,
                    New EnumUInt32Jar(Of AuthenticationBeginLogOnType)("logon type").Weaken,
                    New ArrayJar("server cd key salt", 4),
                    New ArrayJar("udp value", 4),
                    New ArrayJar("mpq file time", 8),
                    New StringJar("mpq number string"),
                    New StringJar("mpq hash challenge"),
                    New ArrayJar("server signature", 128))
            regPack(jar, PacketId.AuthenticationFinish,
                    New ArrayJar("client cd key salt", 4).Weaken,
                    New ArrayJar("exe version", 4).Weaken,
                    New ArrayJar("mpq challenge response", 4).Weaken,
                    New UInt32Jar("# cd keys").Weaken,
                    New ValueJar("spawn [unused]", 4, "0=false, 1=true").Weaken,
                    New CDKeyJar("ROC cd key").Weaken,
                    New CDKeyJar("TFT cd key").Weaken,
                    New StringJar("exe info").Weaken,
                    New StringJar("owner").Weaken)
            regParse(jar, PacketId.AuthenticationFinish,
                    New EnumUInt32Jar(Of AuthenticationFinishResult)("result").Weaken,
                    New StringJar("info"))
            regPack(jar, PacketId.AccountLogOnBegin,
                    New ArrayJar("client public key", 32).Weaken,
                    New StringJar("username").Weaken)
            regParse(jar, PacketId.AccountLogOnBegin,
                    New EnumUInt32Jar(Of AccountLogOnBeginResult)("result").Weaken,
                    New ArrayJar("account password salt", 32),
                    New ArrayJar("server public key", 32))
            regParse(jar, PacketId.AccountLogOnFinish,
                    New EnumUInt32Jar(Of AccountLogOnFinishResult)("result").Weaken,
                    New ArrayJar("server password proof", 20),
                    New StringJar("custom error info"))
            regPack(jar, PacketId.AccountLogOnFinish,
                    New ArrayJar("client password proof", 20).Weaken)
            regParse(jar, PacketId.RequiredWork,
                    New StringJar("filename"))

            'Interaction
            regParse(jar, PacketId.ChatEvent,
                    New EnumUInt32Jar(Of ChatEventId)("event id").Weaken,
                    New ArrayJar("flags", 4),
                    New UInt32Jar("ping").Weaken,
                    New WC3.IPAddressJar("ip"),
                    New ArrayJar("acc#", 4),
                    New ArrayJar("authority", 4),
                    New StringJar("username"),
                    New StringJar("text"))
            regParse(jar, PacketId.MessageBox,
                    New UInt32Jar("style").Weaken,
                    New StringJar("text"),
                    New StringJar("caption"))
            regPack(jar, PacketId.ChatCommand,
                    New StringJar("text").Weaken)
            regParse(jar, PacketId.FriendsUpdate,
                    New ByteJar("entry number").Weaken,
                    New ByteJar("location id").Weaken,
                    New ByteJar("status").Weaken,
                    New StringJar("product id", False, True, 4),
                    New StringJar("location"))
            regPack(jar, PacketId.QueryGamesList,
                    New EnumUInt32Jar(Of WC3.GameTypes)("filter").Weaken,
                    New EnumUInt32Jar(Of WC3.GameTypes)("filter mask").Weaken,
                    New UInt32Jar("unknown0").Weaken,
                    New UInt32Jar("list count").Weaken,
                    New StringJar("game name", True, , , "empty means list games").Weaken,
                    New StringJar("game password", True).Weaken,
                    New StringJar("game stats", True).Weaken)
            regParse(jar, PacketId.QueryGamesList, New ListParseJar(Of Dictionary(Of String, Object))("games", numSizePrefixBytes:=4, subJar:=New TupleParseJar("game",
                    New EnumUInt32Jar(Of WC3.GameTypes)("game type").Weaken,
                    New UInt32Jar("language id").Weaken,
                    New WC3.AddressJar("host address"),
                    New EnumUInt32Jar(Of GameStates)("game state").Weaken,
                    New UInt32Jar("elapsed seconds").Weaken,
                    New StringJar("game name", True),
                    New StringJar("game password", True),
                    New TextHexValueJar("num free slots", numdigits:=1).Weaken,
                    New TextHexValueJar("game id", numdigits:=8).Weaken,
                    New WC3.GameStatsJar("game statstring").Weaken)))

            'State
            regPack(jar, PacketId.EnterChat,
                    New StringJar("username", , , , "[unused]").Weaken,
                    New StringJar("statstring", , , , "[unused]").Weaken)
            regParse(jar, PacketId.EnterChat,
                    New StringJar("chat username"),
                    New StringJar("statstring", , True),
                    New StringJar("account username"))
            regPack(jar, PacketId.CreateGame3,
                    New EnumUInt32Jar(Of GameStates)("game state").Weaken,
                    New UInt32Jar("seconds since creation").Weaken,
                    New EnumUInt32Jar(Of WC3.GameTypes)("game type").Weaken,
                    New UInt32Jar("unknown1=1023").Weaken,
                    New ValueJar("ladder", 4, "0=false, 1=true)").Weaken,
                    New StringJar("name").Weaken,
                    New StringJar("password").Weaken,
                    New TextHexValueJar("num free slots", numdigits:=1).Weaken,
                    New TextHexValueJar("game id", numdigits:=8).Weaken,
                    New WC3.GameStatsJar("statstring").Weaken)
            regParse(jar, PacketId.CreateGame3,
                    New ValueJar("result", 4, "0=success").Weaken)
            regPack(jar, PacketId.CloseGame3)
            regPack(jar, PacketId.JoinChannel,
                    New ArrayJar("flags", 4, , , "0=no create, 1=first join, 2=forced join, 3=diablo2 join").Weaken,
                    New StringJar("channel").Weaken)
            regPack(jar, PacketId.NetGamePort,
                    New UInt16Jar("port").Weaken)

            'Periodic
            regParse(jar, PacketId.Null)
            regPack(jar, PacketId.Null)
            regParse(jar, PacketId.Ping,
                    New UInt32Jar("salt").Weaken)
            regPack(jar, PacketId.Ping,
                    New UInt32Jar("salt").Weaken)
            regParse(jar, PacketId.Warden,
                    New ArrayJar("encrypted data", takeRest:=True))
            regPack(jar, PacketId.Warden,
                    New ArrayJar("encrypted data", takeRest:=True).Weaken)

            Return jar
        End Function
#End Region

#Region "Packers: Logon"
        Public Shared Function MakeAuthenticationBegin(ByVal version As UInteger, ByVal localIPAddress As Byte()) As Packet
            Contract.Ensures(Contract.Result(Of Packet)() IsNot Nothing)
            Return New Packet(PacketId.AuthenticationBegin, New Dictionary(Of String, Object) From {
                    {"protocol", 0},
                    {"platform", "IX86"},
                    {"product", "W3XP"},
                    {"product version", version},
                    {"product language", "SUne"},
                    {"internal ip", localIPAddress},
                    {"time zone offset", 240},
                    {"location id", 1033},
                    {"language id", 1033},
                    {"country abrev", "USA"},
                    {"country name", "United States"}
                })
        End Function
        Public Shared Function MakeAuthenticationFinish(ByVal version As Byte(),
                                                        ByVal mpqFolder As String,
                                                        ByVal indexString As String,
                                                        ByVal mpqHashChallenge As String,
                                                        ByVal serverCDKeySalt As Byte(),
                                                        ByVal cdKeyOwner As String,
                                                        ByVal exeInformation As String,
                                                        ByVal cdKeyROC As String,
                                                        ByVal cdKeyTFT As String,
                                                        ByVal secureRandomNumberGenerator As System.Security.Cryptography.RandomNumberGenerator) As Packet
            Contract.Requires(version IsNot Nothing)
            Contract.Requires(mpqFolder IsNot Nothing)
            Contract.Requires(indexString IsNot Nothing)
            Contract.Requires(mpqHashChallenge IsNot Nothing)
            Contract.Requires(serverCDKeySalt IsNot Nothing)
            Contract.Requires(cdKeyOwner IsNot Nothing)
            Contract.Requires(exeInformation IsNot Nothing)
            Contract.Requires(cdKeyROC IsNot Nothing)
            Contract.Requires(cdKeyTFT IsNot Nothing)
            Contract.Requires(secureRandomNumberGenerator IsNot Nothing)
            Contract.Ensures(Contract.Result(Of Packet)() IsNot Nothing)

            Dim clientCdKeySalt(0 To 3) As Byte
            secureRandomNumberGenerator.GetBytes(clientCdKeySalt)

            Return New Packet(PacketId.AuthenticationFinish, New Dictionary(Of String, Object) From {
                    {"client cd key salt", clientCdKeySalt},
                    {"exe version", version},
                    {"mpq challenge response", Bnet.GenerateRevisionCheck(mpqFolder, indexString, mpqHashChallenge).Bytes()},
                    {"# cd keys", 2},
                    {"spawn [unused]", 0},
                    {"ROC cd key", CDKeyJar.PackCDKey(cdKeyROC, clientCdKeySalt.ToView, serverCDKeySalt.ToView)},
                    {"TFT cd key", CDKeyJar.PackCDKey(cdKeyTFT, clientCdKeySalt.ToView, serverCDKeySalt.ToView)},
                    {"exe info", exeInformation},
                    {"owner", cdKeyOwner}
                })
        End Function
        Public Shared Function MakeAccountLogOnBegin(ByVal credentials As ClientCredentials) As Packet
            Contract.Requires(credentials IsNot Nothing)
            Contract.Ensures(Contract.Result(Of Packet)() IsNot Nothing)
            Return New Packet(PacketId.AccountLogOnBegin, New Dictionary(Of String, Object) From {
                    {"client public key", credentials.PublicKeyBytes.ToArray},
                    {"username", credentials.UserName}
                })
        End Function
        Public Shared Function MakeAccountLogOnFinish(ByVal clientPasswordProof As IList(Of Byte)) As Packet
            Contract.Requires(clientPasswordProof IsNot Nothing)
            Contract.Ensures(Contract.Result(Of Packet)() IsNot Nothing)
            Return New Packet(PacketId.AccountLogOnFinish, New Dictionary(Of String, Object) From {
                    {"client password proof", clientPasswordProof.ToArray}
                })
        End Function
        Public Shared Function MakeEnterChat() As Packet
            Contract.Ensures(Contract.Result(Of Packet)() IsNot Nothing)
            Return New Packet(PacketId.EnterChat, New Dictionary(Of String, Object) From {
                    {"username", ""},
                    {"statstring", ""}
                })
        End Function
#End Region

#Region "Packers: State"
        Public Shared Function MakeNetGamePort(ByVal port As UShort) As Packet
            Contract.Ensures(Contract.Result(Of Packet)() IsNot Nothing)
            Dim vals As New Dictionary(Of String, Object)
            vals("port") = port
            Return New Packet(PacketId.NetGamePort, vals)
        End Function
        Public Shared Function MakeQueryGamesList(Optional ByVal specificGameName As String = "",
                                                  Optional ByVal listCount As Integer = 20) As Packet
            Contract.Ensures(Contract.Result(Of Packet)() IsNot Nothing)
            Return New Packet(PacketId.QueryGamesList, New Dictionary(Of String, Object) From {
                    {"filter", WC3.GameTypes.MaskFilterable},
                    {"filter mask", 0},
                    {"unknown0", 0},
                    {"list count", listCount},
                    {"game name", specificGameName},
                    {"game password", ""},
                    {"game stats", ""}})
        End Function
        Public Shared Function MakeJoinChannel(ByVal flags As JoinChannelType,
                                               ByVal channel As String) As Packet
            Contract.Ensures(Contract.Result(Of Packet)() IsNot Nothing)
            Dim vals As New Dictionary(Of String, Object)
            Return New Packet(PacketId.JoinChannel, New Dictionary(Of String, Object) From {
                    {"flags", CUInt(flags).Bytes()},
                    {"channel", channel}})
        End Function
        Public Shared Function MakeCreateGame3(ByVal game As WC3.IGameDescription) As Packet
            Contract.Requires(game IsNot Nothing)
            Contract.Ensures(Contract.Result(Of Packet)() IsNot Nothing)
            Const MAX_GAME_NAME_LENGTH As UInteger = 31
            If game.Name.Length > MAX_GAME_NAME_LENGTH Then
                Throw New ArgumentException("Game name must be less than 32 characters long.", "name")
            End If

            Return New Packet(PacketId.CreateGame3, New Dictionary(Of String, Object) From {
                    {"game state", game.State},
                    {"seconds since creation", game.AgeSeconds},
                    {"game type", game.Type},
                    {"unknown1=1023", 1023},
                    {"ladder", 0},
                    {"name", game.Name},
                    {"password", ""},
                    {"num free slots", game.TotalSlotCount - game.UsedSlotCount},
                    {"game id", game.GameId},
                    {"statstring", game.GameStats}})
        End Function
        Public Shared Function MakeCloseGame3() As Packet
            Contract.Ensures(Contract.Result(Of Packet)() IsNot Nothing)
            Return New Packet(PacketId.CloseGame3, New Dictionary(Of String, Object))
        End Function
#End Region

#Region "Packers: CKL"
        Public Shared Function MakeCKLAuthenticationFinish(ByVal version As Byte(),
                                                           ByVal mpqFolder As String,
                                                           ByVal mpqNumberString As String,
                                                           ByVal mpqHashChallenge As String,
                                                           ByVal serverCDKeySalt As Byte(),
                                                           ByVal cdKeyOwner As String,
                                                           ByVal exeInformation As String,
                                                           ByVal remoteHostName As String,
                                                           ByVal remotePort As UShort,
                                                           ByVal secureRandomNumberGenerator As Security.Cryptography.RandomNumberGenerator) As IFuture(Of Packet)
            Contract.Requires(version IsNot Nothing)
            Contract.Requires(mpqFolder IsNot Nothing)
            Contract.Requires(mpqNumberString IsNot Nothing)
            Contract.Requires(mpqHashChallenge IsNot Nothing)
            Contract.Requires(serverCDKeySalt IsNot Nothing)
            Contract.Requires(cdKeyOwner IsNot Nothing)
            Contract.Requires(exeInformation IsNot Nothing)
            Contract.Requires(remoteHostName IsNot Nothing)
            Contract.Requires(secureRandomNumberGenerator IsNot Nothing)
            Contract.Ensures(Contract.Result(Of IFuture(Of Packet))() IsNot Nothing)

            Dim clientCdKeySalt(0 To 3) As Byte
            secureRandomNumberGenerator.GetBytes(clientCdKeySalt)

            Dim vals = New Dictionary(Of String, Object) From {
                {"client cd key salt", clientCdKeySalt},
                {"exe version", version},
                {"mpq challenge response", GenerateRevisionCheck(mpqFolder, mpqNumberString, mpqHashChallenge).Bytes},
                {"# cd keys", 2},
                {"spawn [unused]", 0},
                {"exe info", exeInformation},
                {"owner", cdKeyOwner}}

            'Asynchronously borrow keys from server and return completed packet
            Return CKL.CKLClient.BeginBorrowKeys(remoteHostName, remotePort, clientCdKeySalt, serverCDKeySalt).Select(
                Function(borrowedKeys)
                    vals("ROC cd key") = borrowedKeys.CDKeyROC
                    vals("TFT cd key") = borrowedKeys.CDKeyTFT
                    Return New Packet(PacketId.AuthenticationFinish, vals)
                End Function
            )
        End Function
#End Region

#Region "Packers: Misc"
        Public Const MaxChatCommandTextLength As Integer = 222
        Shared Function MakeChatCommand(ByVal text As String) As Packet
            Contract.Requires(text IsNot Nothing)
            Contract.Ensures(Contract.Result(Of Packet)() IsNot Nothing)

            If text.Length > MaxChatCommandTextLength Then
                Throw New ArgumentException("Text cannot exceed {0} characters.".Frmt(MaxChatCommandTextLength), "text")
            End If
            Return New Packet(PacketId.ChatCommand, New Dictionary(Of String, Object) From {
                    {"text", text}})
        End Function
        Public Shared Function MakePing(ByVal salt As UInteger) As Packet
            Contract.Ensures(Contract.Result(Of Packet)() IsNot Nothing)

            Return New Packet(PacketId.Ping, New Dictionary(Of String, Object) From {
                    {"salt", salt}})
        End Function
        Public Shared Function MakeWarden(ByVal encryptedData As Byte()) As Packet
            Contract.Requires(encryptedData IsNot Nothing)
            Contract.Ensures(Contract.Result(Of Packet)() IsNot Nothing)

            Return New Packet(PacketId.Warden, New Dictionary(Of String, Object) From {
                    {"encrypted data", encryptedData}})
        End Function
#End Region

#Region "Jars"
        Public NotInheritable Class TextHexValueJar
            Inherits Jar(Of ULong)
            Private ReadOnly numDigits As Integer
            Private ReadOnly byteOrder As ByteOrder

            <ContractInvariantMethod()> Private Sub ObjectInvariant()
                Contract.Invariant(numDigits > 0)
            End Sub

            Public Sub New(ByVal name As String,
                           ByVal numDigits As Integer,
                           Optional ByVal byteOrder As ByteOrder = byteOrder.LittleEndian)
                MyBase.New(name)
                Contract.Requires(name IsNot Nothing)
                Contract.Requires(numDigits > 0)
                Contract.Requires(numDigits <= 16)
                Me.numDigits = numDigits
                Me.byteOrder = byteOrder
            End Sub

            Public Overrides Function Pack(Of TValue As ULong)(ByVal value As TValue) As IPickle(Of TValue)
                Dim u = CULng(value)
                Dim digits As IList(Of Char) = New List(Of Char)
                For i = 0 To numDigits - 1
                    Dim val = Hex(u And CULng(&HF)).ToLowerInvariant()
                    Contract.Assume(val.Length = 1)
                    digits.Add(val(0))
                    u >>= 4
                Next i

                Select Case byteOrder
                    Case byteOrder.BigEndian
                        digits = digits.Reverse
                    Case byteOrder.LittleEndian
                        'no change
                    Case Else
                        Throw byteOrder.MakeImpossibleValueException()
                End Select

                Return New Pickling.Pickle(Of TValue)(Me.Name, value.AssumeNotNull, New String(digits.ToArray).ToAscBytes().ToView())
            End Function

            Public Overrides Function Parse(ByVal data As ViewableList(Of Byte)) As IPickle(Of ULong)
                If data.Length < numDigits Then Throw New PicklingException("Not enough data")
                data = data.SubView(0, numDigits)
                Dim value = data.ParseChrString(nullTerminated:=False).FromHexStringToBytes(byteOrder)
                Return New Pickling.Pickle(Of ULong)(Me.Name, value, data)
            End Function
        End Class

        Public NotInheritable Class CDKeyJar
            Inherits Pickling.Jars.TupleJar

            Public Sub New(ByVal name As String)
                MyBase.New(name,
                        New UInt32Jar("length").Weaken,
                        New EnumUInt32Jar(Of CDKeyProduct)("product key").Weaken,
                        New UInt32Jar("public key").Weaken,
                        New UInt32Jar("unknown").Weaken,
                        New ArrayJar("hash", 20).Weaken)
                Contract.Requires(name IsNot Nothing)
            End Sub

            Public Shared Function PackCDKey(ByVal wc3Key As String,
                                             ByVal clientToken As ViewableList(Of Byte),
                                             ByVal serverToken As ViewableList(Of Byte)) As Dictionary(Of String, Object)
                Contract.Requires(wc3Key IsNot Nothing)
                Contract.Requires(clientToken IsNot Nothing)
                Contract.Requires(serverToken IsNot Nothing)

                Dim key = CDKey.FromWC3StyleKey(wc3Key)
                Return New Dictionary(Of String, Object) From {
                        {"length", CUInt(wc3Key.Length)},
                        {"product key", key.Product},
                        {"public key", key.PublicKey},
                        {"unknown", 0},
                        {"hash", Concat(clientToken.ToArray,
                                        serverToken.ToArray,
                                        CUInt(key.Product).Bytes,
                                        key.PublicKey.Bytes,
                                        key.PrivateKey.ToArray).SHA1}}
            End Function

            Public Shared Function PackBorrowedCDKey(ByVal data() As Byte) As Dictionary(Of String, Object)
                Contract.Requires(data IsNot Nothing)
                Contract.Requires(data.Length = 36)

                Return New Dictionary(Of String, Object) From {
                        {"length", data.SubArray(0, 4).ToUInt32},
                        {"product key", data.SubArray(4, 4).ToUInt32},
                        {"public key", data.SubArray(8, 4).ToUInt32},
                        {"unknown", data.SubArray(12, 4).ToUInt32},
                        {"hash", data.SubArray(16, 20)}}
            End Function
        End Class
#End Region
    End Class
End Namespace
