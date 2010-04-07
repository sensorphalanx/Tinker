﻿Imports Strilbrary.Values
Imports Strilbrary.Collections
Imports Strilbrary.Time
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Collections.Generic
Imports Tinker.Pickling
Imports Tinker
Imports Tinker.WC3
Imports Tinker.WC3.Protocol
Imports TinkerTests.PicklingTest

<TestClass()>
Public Class WC3ProtocolTest
    <TestMethod()>
    Public Sub ClientConfirmHostLeavingTest()
        JarTest(Packets.ClientConfirmHostLeaving.Jar, data:={}, value:=New Pickling.EmptyJar.EmptyValue)
    End Sub
    <TestMethod()>
    Public Sub ClientMapInfoTest()
        JarTest(Packets.ClientMapInfo.Jar,
                data:={1, 0, 0, 0,
                       3,
                       128, 0, 0, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"map transfer key", 1UI},
                        {"transfer state", MapTransferState.Downloading},
                        {"total downloaded", 128UI}
                    })
    End Sub
    <TestMethod()>
    Public Sub GameActionTest()
        JarTest(Packets.GameAction.Jar,
                equater:=Function(e1, e2) ObjectEqual(e1, e2),
                appendSafe:=False,
                data:={0, 0, 0, 0},
                value:=New List(Of GameAction)().AsReadableList)
    End Sub
    <TestMethod()>
    Public Sub GreetTest()
        JarTest(Packets.Greet.Jar,
                data:={0, 0,
                       2,
                       2, 0, &H17, &HE1, 127, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"slot data", New Byte() {}.AsReadableList},
                        {"assigned id", New PlayerId(2)},
                        {"external address", New Net.IPEndPoint(Net.IPAddress.Loopback, 6113)}
                    })
    End Sub
    <TestMethod()>
    Public Sub HostConfirmHostLeavingTest()
        JarTest(Packets.HostConfirmHostLeaving.Jar, data:={}, value:=New Pickling.EmptyJar.EmptyValue)
    End Sub
    <TestMethod()>
    Public Sub HostMapInfoTest()
        Dim sha1 = CByte(20).Range.ToReadableList
        JarTest(Packets.HostMapInfo.Jar,
                data:=New Byte() _
                      {0, 0, 0, 0,
                       116, 101, 115, 116, 0,
                       15, 0, 0, 0,
                       32, 0, 0, 0,
                       13, 0, 0, 0}.Concat(
                       sha1).ToArray,
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"map transfer key", 0UI},
                        {"path", "test"},
                        {"size", 15UI},
                        {"crc32", 32UI},
                        {"xoro checksum", 13UI},
                        {"sha1 checksum", sha1}
                    })
    End Sub
    <TestMethod()>
    Public Sub KnockTest()
        JarTest(Packets.Knock.Jar,
                data:={42, 0, 0, 0,
                       99, 0, 0, 0,
                       0,
                       &HE0, &H17,
                       16, 0, 0, 0,
                       116, 101, 115, 116, 0,
                       1, 0,
                       2, 0, &H17, &HE1, 127, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"game id", 42UI},
                        {"entry key", 99UI},
                        {"unknown value", CByte(0)},
                        {"listen port", 6112US},
                        {"peer key", 16UI},
                        {"name", "test"},
                        {"peer data", New Byte() {0}.AsReadableList},
                        {"internal address", New Net.IPEndPoint(Net.IPAddress.Loopback, 6113)}
                    })
    End Sub
    <TestMethod()>
    Public Sub LanCreateGameTest()
        JarTest(Packets.LanCreateGame.Jar,
                data:={Asc("3"), Asc("r"), Asc("a"), Asc("w"),
                       20, 0, 0, 0,
                       42, 0, 0, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"product id", "war3"},
                        {"major version", 20UI},
                        {"game id", 42UI}
                    })
    End Sub
    <TestMethod()>
    Public Sub LanGameDetailsTest()
        JarTest(Packets.LanGameDetails.Jar,
                data:=New Byte() _
                      {Asc("3"), Asc("r"), Asc("a"), Asc("w"),
                       20, 0, 0, 0,
                       42, 0, 0, 0,
                       16, 0, 0, 0,
                       116, 101, 115, 116, 0,
                       0}.Concat(
                       New GameStatsJar().Pack(TestStats)).Concat({
                       12, 0, 0, 0,
                       8, 0, 0, 0,
                       2, 0, 0, 0,
                       12, 0, 0, 0,
                       25, 0, 0, 0,
                       &HE0, &H17}).ToArray,
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"product id", "war3"},
                        {"major version", 20UI},
                        {"game id", 42UI},
                        {"entry key", 16UI},
                        {"name", "test"},
                        {"password", ""},
                        {"statstring", TestStats},
                        {"num slots", 12UI},
                        {"game type", GameTypes.AuthenticatedMakerBlizzard},
                        {"num players + 1", 2UI},
                        {"free slots + 1", 12UI},
                        {"age", 25UI},
                        {"listen port", 6112US}
                    })
    End Sub
    <TestMethod()>
    Public Sub LanDestroyGameTest()
        JarTest(Packets.LanDestroyGame.Jar,
                data:={20, 0, 0, 0},
                value:=20)
    End Sub
    <TestMethod()>
    Public Sub LanRefreshGameTest()
        JarTest(Packets.LanRefreshGame.Jar,
                data:={42, 0, 0, 0,
                       2, 0, 0, 0,
                       1, 0, 0, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"game id", 42UI},
                        {"num players", 2UI},
                        {"free slots", 1UI}
                    })
    End Sub
    <TestMethod()>
    Public Sub LanRequestGameTest()
        JarTest(Packets.LanRequestGame.Jar,
                data:={Asc("3"), Asc("r"), Asc("a"), Asc("w"),
                       20, 0, 0, 0,
                       0, 0, 0, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"product id", "war3"},
                        {"major version", 20UI},
                        {"unknown1", 0UI}
                    })
    End Sub
    <TestMethod()>
    Public Sub LeavingTest()
        JarTest(Packets.Leaving.Jar,
                data:={7, 0, 0, 0},
                value:=PlayerLeaveReason.Quit,
                equater:=Function(e1, e2) e1 = e2)
    End Sub
    <TestMethod()>
    Public Sub LobbyStateTest()
        Dim slots = New List(Of Slot)()
        JarTest(Packets.LobbyState.Jar,
                data:={7, 0,
                       0,
                       13, 0, 0, 0,
                       3,
                       12},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"slots", New List(Of NamedValueMap)().ToReadableList},
                        {"random seed", 13UI},
                        {"layout style", LobbyLayoutStyle.FixedPlayerSettings},
                        {"num player slots", CByte(12)}
                    })
    End Sub
    <TestMethod()>
    Public Sub MapFileDataTest()
        JarTest(Packets.MapFileData.Jar,
                appendSafe:=False,
                data:={2,
                       3,
                       0, 0, 0, 0,
                       128, 0, 0, 0,
                       205, 251, 60, 182,
                       1, 2, 3, 4},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"downloader", New PlayerId(2)},
                        {"uploader", New PlayerId(3)},
                        {"map transfer key", 0UI},
                        {"file position", 128UI},
                        {"file data", New Byte() {1, 2, 3, 4}.AsReadableList}
                    })
    End Sub
    <TestMethod()>
    Public Sub MapFileDataProblemTest()
        JarTest(Packets.MapFileDataProblem.Jar,
                data:={2,
                       3,
                       0, 0, 0, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"downloader", New PlayerId(2)},
                        {"uploader", New PlayerId(3)},
                        {"map transfer key", 0UI}
                    })
    End Sub
    <TestMethod()>
    Public Sub MapFileDataReceivedTest()
        JarTest(Packets.MapFileDataReceived.Jar,
                data:={2,
                       3,
                       0, 0, 0, 0,
                       128, 0, 0, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"downloader", New PlayerId(2)},
                        {"uploader", New PlayerId(3)},
                        {"map transfer key", 0UI},
                        {"total downloaded", 128UI}
                    })
    End Sub
    <TestMethod()>
    Public Sub NewHostTest()
        JarTest(Packets.NewHost.Jar,
                data:={1},
                value:=New PlayerId(1))
    End Sub
    <TestMethod()>
    Public Sub NonGameActionTest()
        JarTest(Packets.NonGameAction.Jar,
                data:={3, 1, 2, 3,
                       4,
                       32,
                       1, 0, 0, 0,
                       116, 101, 115, 116, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"requested receivers", {New PlayerId(1), New PlayerId(2), New PlayerId(3)}.ToReadableList},
                        {"sender", New PlayerId(4)},
                        {"value", NonGameActionType.GameChat.KeyValue(Of Object)(New NamedValueMap(New Dictionary(Of InvariantString, Object) From {
                                    {"receiving group", ChatGroup.Allies},
                                    {"message", "test"}
                                }))}})
        JarTest(Packets.NonGameAction.Jar,
                data:={3, 1, 2, 3,
                       4,
                       16,
                       116, 101, 115, 116, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"requested receivers", {New PlayerId(1), New PlayerId(2), New PlayerId(3)}.ToReadableList},
                        {"sender", New PlayerId(4)},
                        {"value", NonGameActionType.LobbyChat.KeyValue(Of Object)(New NamedValueMap(New Dictionary(Of InvariantString, Object) From {
                                    {"message", "test"}
                                }))}})
        JarTest(Packets.NonGameAction.Jar,
                data:={3, 1, 2, 3,
                       4,
                       17,
                       1},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"requested receivers", {New PlayerId(1), New PlayerId(2), New PlayerId(3)}.ToReadableList},
                        {"sender", New PlayerId(4)},
                        {"value", NonGameActionType.SetTeam.KeyValue(Of Object)(CByte(1))}
                    })
        JarTest(Packets.NonGameAction.Jar,
                data:={3, 1, 2, 3,
                       4,
                       20,
                       100},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"requested receivers", {New PlayerId(1), New PlayerId(2), New PlayerId(3)}.ToReadableList},
                        {"sender", New PlayerId(4)},
                        {"value", NonGameActionType.SetHandicap.KeyValue(Of Object)(CByte(100))}
                    })
        JarTest(Packets.NonGameAction.Jar,
                data:={3, 1, 2, 3,
                       4,
                       18,
                       1},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"requested receivers", {New PlayerId(1), New PlayerId(2), New PlayerId(3)}.ToReadableList},
                        {"sender", New PlayerId(4)},
                        {"value", NonGameActionType.SetColor.KeyValue(Of Object)(PlayerColor.Blue)}
                    })
        JarTest(Packets.NonGameAction.Jar,
                data:={3, 1, 2, 3,
                       4,
                       19,
                       2},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"requested receivers", {New PlayerId(1), New PlayerId(2), New PlayerId(3)}.ToReadableList},
                        {"sender", New PlayerId(4)},
                        {"value", NonGameActionType.SetRace.KeyValue(Of Object)(Races.Orc)}
                    })
    End Sub
    <TestMethod()>
    Public Sub OtherPlayerJoinedTest()
        JarTest(Packets.OtherPlayerJoined.Jar,
                data:={27, 0, 0, 0,
                       1,
                       116, 101, 115, 116, 0,
                       1, 42,
                       2, 0, &H17, &HE0, 127, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0,
                       2, 0, &H17, &HE1, 127, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"peer key", 27UI},
                        {"joiner id", New PlayerId(1)},
                        {"name", "test"},
                        {"peer data", New Byte() {42}.AsReadableList},
                        {"external address", New Net.IPEndPoint(Net.IPAddress.Loopback, 6112)},
                        {"internal address", New Net.IPEndPoint(Net.IPAddress.Loopback, 6113)}
                    })
    End Sub
    <TestMethod()>
    Public Sub OtherPlayerLeftTest()
        JarTest(Packets.OtherPlayerLeft.Jar,
                data:={1,
                       7, 0, 0, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"leaver", New PlayerId(1)},
                        {"reason", PlayerLeaveReason.Quit}
                    })
    End Sub
    <TestMethod()>
    Public Sub OtherPlayerReadyTest()
        JarTest(Packets.OtherPlayerReady.Jar,
                data:={3},
                value:=New PlayerId(3))
    End Sub
    <TestMethod()>
    Public Sub PeerConnectionInfoTest()
        JarTest(Packets.PeerConnectionInfo.Jar,
                data:={7, 0},
                value:=7)
    End Sub
    <TestMethod()>
    Public Sub PeerKnockTest()
        JarTest(Packets.PeerKnock.Jar,
                data:={42, 0, 0, 0,
                       0, 0, 0, 0,
                       1,
                       0,
                       7, 0, 0, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"receiver peer key", 42UI},
                        {"unknown1", 0UI},
                        {"sender id", New PlayerId(1)},
                        {"unknown3", CByte(0)},
                        {"sender peer connection flags", 7UI}
                    })
    End Sub
    <TestMethod()>
    Public Sub PeerPingTest()
        JarTest(Packets.PeerPing.Jar,
                data:={&HEF, &HBE, &HAD, &HDE,
                       7, 0, 0, 0,
                       1, 0, 0, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"salt", &HDEADBEEFUI},
                        {"sender peer connection flags", 7UI},
                        {"unknown2", 1UI}
                    })
    End Sub
    <TestMethod()>
    Public Sub PeerPongTest()
        JarTest(Packets.PeerPong.Jar,
                data:={&HEF, &HBE, &HAD, &HDE},
                value:=&HDEADBEEFUI)
    End Sub
    <TestMethod()>
    Public Sub PingTest()
        JarTest(Packets.Ping.Jar,
                data:={&HEF, &HBE, &HAD, &HDE},
                value:=&HDEADBEEFUI)
    End Sub
    <TestMethod()>
    Public Sub PongTest()
        JarTest(Packets.Pong.Jar,
                data:={&HEF, &HBE, &HAD, &HDE},
                value:=&HDEADBEEFUI)
    End Sub
    <TestMethod()>
    Public Sub ReadyTest()
        JarTest(Packets.Ready.Jar, data:={}, value:=New Pickling.EmptyJar.EmptyValue)
    End Sub
    <TestMethod()>
    Public Sub RejectEntryTest()
        JarTest(Packets.RejectEntry.Jar,
                data:={27, 0, 0, 0},
                value:=RejectReason.IncorrectPassword,
                equater:=Function(e1, e2) e1 = e2)
    End Sub
    <TestMethod()>
    Public Sub RemovePlayerFromLagScreenTest()
        JarTest(Packets.RemovePlayerFromLagScreen.Jar,
                data:={4,
                       23, 0, 0, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"lagger", New PlayerId(4)},
                        {"marginal milliseconds used", 23UI}
                    })
    End Sub
    <TestMethod()>
    Public Sub RequestDropLaggersTest()
        JarTest(Packets.RequestDropLaggers.Jar, data:={}, value:=New Pickling.EmptyJar.EmptyValue)
    End Sub
    <TestMethod()>
    Public Sub SetDownloadSourceTest()
        JarTest(Packets.SetDownloadSource.Jar,
                data:={0, 0, 0, 0,
                       2},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"map transfer key", 0UI},
                        {"uploader", New PlayerId(2)}
                    })
    End Sub
    <TestMethod()>
    Public Sub SetUploadTargetTest()
        JarTest(Packets.SetUploadTarget.Jar,
                data:={0, 0, 0, 0,
                       3,
                       128, 0, 0, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"map transfer key", 0UI},
                        {"downloader", New PlayerId(3)},
                        {"starting file pos", 128UI}
                    })
    End Sub
    <TestMethod()>
    Public Sub ShowLagScreenTest()
        Dim lagger = New NamedValueMap(New Dictionary(Of InvariantString, Object) From {
                             {"id", New PlayerId(2)},
                             {"initial milliseconds used", 25UI}
                         })
        JarTest(Packets.ShowLagScreen.Jar,
                equater:=Function(e1 As IReadableList(Of NamedValueMap), e2 As IReadableList(Of NamedValueMap)) ObjectEqual(e1, e2),
                data:={2,
                       2, 25, 0, 0, 0,
                       2, 25, 0, 0, 0},
                value:={lagger, lagger}.ToReadableList)
    End Sub
    <TestMethod()>
    Public Sub StartCountdownTest()
        JarTest(Packets.StartCountdown.Jar, data:={}, value:=New Pickling.EmptyJar.EmptyValue)
    End Sub
    <TestMethod()>
    Public Sub StartLoadingTest()
        JarTest(Packets.StartLoading.Jar, data:={}, value:=New Pickling.EmptyJar.EmptyValue)
    End Sub
    <TestMethod()>
    Public Sub TextTest()
        JarTest(Packets.Text.Jar,
                data:={2, 2, 3,
                       1,
                       32,
                       1, 0, 0, 0,
                       116, 101, 115, 116, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"requested receivers", {New PlayerId(2), New PlayerId(3)}.ToReadableList},
                        {"speaker", New PlayerId(1)},
                        {"type group", ChatType.Game.KeyValue(Of Object)(ChatGroup.Allies)},
                        {"message", "test"}
                    })
        JarTest(Packets.Text.Jar,
                data:={2, 2, 3,
                       1,
                       16,
                       116, 101, 115, 116, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"requested receivers", {New PlayerId(2), New PlayerId(3)}.ToReadableList},
                        {"speaker", New PlayerId(1)},
                        {"type group", ChatType.Lobby.KeyValue(Of Object)(New EmptyJar.EmptyValue)},
                        {"message", "test"}
                    })
    End Sub
    <TestMethod()>
    Public Sub TickTest()
        JarTest(Packets.Tick.Jar,
                appendSafe:=False,
                data:={250, 0,
                       208, 15,
                            1,
                                6, 0,
                                39,
                                2,
                                100, 0, 0, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"time span", 250US},
                        {"player action sets", {New PlayerActionSet(New PlayerId(1),
                                           {New GameAction(GameActionId.CheatGold,
                                                           New Dictionary(Of InvariantString, Object) From {
                                                               {"amount", 100UI},
                                                               {"unknown", CByte(2)}})
                                            }.AsReadableList)
                                     }.AsReadableList.Maybe}
                    })
        JarTest(Packets.Tick.Jar,
                appendSafe:=False,
                data:={100, 0},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"time span", 100US},
                        {"player action sets", New Maybe(Of IReadableList(Of PlayerActionSet))()}})
    End Sub
    <TestMethod()>
    Public Sub TockTest()
        JarTest(Packets.Tock.Jar,
                data:={1, 2, 3, 4, 5},
                value:=New Dictionary(Of InvariantString, Object) From {
                        {"unknown", CByte(1)},
                        {"game state checksum", &H5040302UI}
                    })
    End Sub
End Class
