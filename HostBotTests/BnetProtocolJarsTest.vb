﻿Imports Strilbrary.Collections
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Tinker
Imports Strilbrary.Values
Imports Strilbrary.Time
Imports Tinker.Pickling
Imports System.Collections.Generic
Imports TinkerTests.PicklingTest
Imports Tinker.Bnet.Protocol

<TestClass()>
Public Class BnetProtocolJarsTest
    <TestMethod()>
    Public Sub TextHexUInt32JarTest()
        Dim jar = New TextHexUInt32Jar(digitCount:=8, ByteOrder:=ByteOrder.BigEndian)
        JarTest(jar, &HDEADBEEFUI, "deadbeef".ToAsciiBytes)
        JarTest(jar, 1, "00000001".ToAsciiBytes)
        JarTest(jar, 0, "00000000".ToAsciiBytes)
        JarTest(jar, &HDEADBEEFUI, "deadbeef".ToAsciiBytes)
        jar = New TextHexUInt32Jar(digitCount:=6, ByteOrder:=ByteOrder.LittleEndian)
        JarTest(jar, &HDEADBEUI, "ebdaed".ToAsciiBytes)
        JarTest(jar, 1, "100000".ToAsciiBytes)
        JarTest(jar, 0, "000000".ToAsciiBytes)
        ExpectException(Of PicklingException)(Sub() jar.Pack(&HDEADBEEFUI))
    End Sub
    <TestMethod()>
    Public Sub FileTimeJarTest()
        Dim jar = New FileTimeJar()
        JarTest(jar, New Date(Year:=2010, Month:=1, Day:=16), {0, 224, 177, 95, 96, 150, 202, 1})
        JarTest(jar, New Date(Year:=2009, Month:=2, Day:=16), {0, 96, 185, 9, 235, 143, 201, 1})
    End Sub
    <TestMethod()>
    Public Sub IPAddressJarTest()
        Dim jar = New IPAddressJar()
        Dim equater = Function(a1 As Net.IPAddress, a2 As Net.IPAddress) a1.GetAddressBytes.SequenceEqual(a1.GetAddressBytes)
        JarTest(jar, equater, New Net.IPAddress({0, 0, 0, 0}), {0, 0, 0, 0})
        JarTest(jar, equater, Net.IPAddress.Loopback, {127, 0, 0, 1})
    End Sub
    <TestMethod()>
    Public Sub IPEndPointTest()
        Dim jar = New IPEndPointJar()
        Dim equater = Function(a1 As Net.IPEndPoint, a2 As Net.IPEndPoint) a1.Address.GetAddressBytes.SequenceEqual(a1.Address.GetAddressBytes) AndAlso a1.Port = a2.Port
        JarTest(jar, equater, New Net.IPAddress({0, 0, 0, 0}).WithPort(0), {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0})
        JarTest(jar, equater, Net.IPAddress.Loopback.WithPort(6112), {2, 0, &H17, &HE0, 127, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0})
    End Sub
    <TestMethod()>
    Public Sub QueryGamesListResponseJarTest()
        Dim jar = New QueryGamesListResponseJar(New ManualClock())
        JarTest(jar,
                New QueryGamesListResponse(QueryGameResponse.Ok, MakeRist(TestDesc)),
                New Byte() { _
                 1, 0, 0, 0,
                 8, 0, 0, 0,
                 0, 0, 0, 0,
                 2, 0, &H17, &HE0, 127, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0,
                 1, 0, 0, 0,
                 5, 0, 0, 0,
                 116, 101, 115, 116, 0,
                 0,
                 Asc("c"c),
                 Asc("a"c), Asc("2"c), Asc("0"c), Asc("0"c), Asc("0"c), Asc("0"c), Asc("0"c), Asc("0"c)
                 }.Concat(New WC3.Protocol.GameStatsJar().Pack(TestStats)))
        JarTest(jar,
                New QueryGamesListResponse(QueryGameResponse.NotFound, MakeRist(Of WC3.RemoteGameDescription)()),
                {0, 0, 0, 0,
                 1, 0, 0, 0})
    End Sub
End Class
