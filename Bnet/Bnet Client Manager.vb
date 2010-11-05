﻿Imports Tinker.Commands
Imports Tinker.Components
Imports Tinker.Bot

Namespace Bnet
    Public NotInheritable Class ClientManager
        Inherits DisposableWithTask
        Implements IBotComponent

        Private ReadOnly _commands As New CommandSet(Of ClientManager)()
        Private ReadOnly inQueue As CallQueue = New TaskedCallQueue
        Private ReadOnly _bot As Bot.MainBot
        Private ReadOnly _name As InvariantString
        Private ReadOnly _client As Bnet.Client
        Private ReadOnly _control As Control
        Private ReadOnly _hooks As New List(Of Task(Of IDisposable))
        Private ReadOnly _userGameSetMap As New Dictionary(Of BotUser, WC3.GameSet)

        <ContractInvariantMethod()> Private Sub ObjectInvariant()
            Contract.Invariant(inQueue IsNot Nothing)
            Contract.Invariant(_userGameSetMap IsNot Nothing)
            Contract.Invariant(_bot IsNot Nothing)
            Contract.Invariant(_client IsNot Nothing)
            Contract.Invariant(_hooks IsNot Nothing)
            Contract.Invariant(_control IsNot Nothing)
            Contract.Invariant(_commands IsNot Nothing)
        End Sub

        Public Sub New(ByVal name As InvariantString,
                       ByVal bot As Bot.MainBot,
                       ByVal client As Bnet.Client)
            Contract.Requires(bot IsNot Nothing)
            Contract.Requires(client IsNot Nothing)

            Me._bot = bot
            Me._name = name
            Me._client = client
            Me._control = New BnetClientControl(Me)

            Me._hooks.Add(client.QueueAddPacketHandler(Protocol.Packets.ServerToClient.ChatEvent,
                                                       Function(pickle) OnReceivedChatEvent(pickle.Value)))

            client.DisposalTask.ContinueWithAction(Sub() Me.Dispose())
        End Sub
        Public Shared Function FromProfileAsync(ByVal clientName As InvariantString,
                                                ByVal profileName As InvariantString,
                                                ByVal clock As IClock,
                                                ByVal bot As Bot.MainBot) As Task(Of ClientManager)
            Contract.Requires(clock IsNot Nothing)
            Contract.Requires(bot IsNot Nothing)
            Contract.Ensures(Contract.Result(Of Task(Of ClientManager))() IsNot Nothing)

            Dim profile = (From p In bot.Settings.ClientProfiles Where p.name = profileName).FirstOrDefault
            If profile Is Nothing Then Throw New ArgumentException("No profile named '{0}'".Frmt(profileName))
            Dim logger = New Logger

            Dim authenticator As IProductAuthenticator
            If profile.CKLServerAddress Like "*:#*" Then
                Dim remoteHost = profile.CKLServerAddress.Split(":"c)(0)
                Dim port = UShort.Parse(profile.CKLServerAddress.Split(":"c)(1).AssumeNotNull, CultureInfo.InvariantCulture)
                authenticator = New CKL.Client(remoteHost, port, clock, logger)
            Else
                authenticator = New CDKeyProductAuthenticator(profile.cdKeyROC, profile.cdKeyTFT)
            End If

            Dim client = New Bnet.Client(profile, New CachedWC3InfoProvider, authenticator, clock, logger)
            Return New Bnet.ClientManager(clientName, bot, client).AsTask
        End Function

        Public ReadOnly Property Client As Bnet.Client
            Get
                Contract.Ensures(Contract.Result(Of Bnet.Client)() IsNot Nothing)
                Return _client
            End Get
        End Property
        Public ReadOnly Property Bot As Bot.MainBot
            Get
                Contract.Ensures(Contract.Result(Of Bot.MainBot)() IsNot Nothing)
                Return _bot
            End Get
        End Property
        Public ReadOnly Property Name As InvariantString Implements IBotComponent.Name
            Get
                Return _name
            End Get
        End Property
        Public ReadOnly Property Type As InvariantString Implements IBotComponent.Type
            Get
                Return "Client"
            End Get
        End Property
        Public ReadOnly Property Logger As Logger Implements IBotComponent.Logger
            Get
                Return _client.Logger
            End Get
        End Property
        Public ReadOnly Property HasControl As Boolean Implements IBotComponent.HasControl
            Get
                Contract.Ensures(Contract.Result(Of Boolean)())
                Return True
            End Get
        End Property
        Public Function IsArgumentPrivate(ByVal argument As String) As Boolean Implements IBotComponent.IsArgumentPrivate
            Return _commands.IsArgumentPrivate(argument)
        End Function
        Public ReadOnly Property Control As Control Implements IBotComponent.Control
            Get
                Return _control
            End Get
        End Property

        Public Function InvokeCommand(ByVal user As BotUser, ByVal argument As String) As Task(Of String) Implements IBotComponent.InvokeCommand
            Return _commands.Invoke(Me, user, argument)
        End Function

        <ContractVerification(False)>
        Private Async Function OnReceivedChatEvent(ByVal vals As NamedValueMap) As Task
            Contract.Requires(vals IsNot Nothing)

            Dim id = vals.ItemAs(Of Bnet.Protocol.ChatEventId)("event id")
            Dim user = _client.Profile.Users(vals.ItemAs(Of String)("username"))
            Dim text = vals.ItemAs(Of String)("text")

            'Check
            Dim commandPrefix = My.Settings.commandPrefix.AssumeNotNull
            If user Is Nothing Then
                Return 'user not allowed
            ElseIf id <> Bnet.Protocol.ChatEventId.Talk AndAlso id <> Bnet.Protocol.ChatEventId.Whisper Then
                Return 'not a message
            ElseIf text = Tinker.Bot.MainBot.TriggerCommandText Then '?trigger command
                If user.Name.Length <= 0 Then Throw New InvalidStateException("Empty user name.")
                _client.QueueSendWhisper(user.Name, "Command prefix: {0}".Frmt(My.Settings.commandPrefix))
                Return
            ElseIf Not text.StartsWith(commandPrefix, StringComparison.OrdinalIgnoreCase) Then 'not a command
                Return 'not a command
            End If

            Try
                'Run command
                Dim command = text.Substring(commandPrefix.Length)
                Dim commandResult = Me.InvokeCommand(user, argument:=command)

                'Setup busy message
                Dim finishedLock = New OnetimeLock()
                Call New SystemClock().AsyncWait(2.Seconds).ContinueWithAction(
                    Sub()
                        If Not finishedLock.TryAcquire Then Return
                        _client.QueueSendWhisper(user.Name, "Command '{0}' is running...".Frmt(text))
                    End Sub
                )

                'Await result
                Dim message = Await commandResult
                finishedLock.TryAcquire()
                _client.QueueSendWhisper(user.Name, If(message, "Command Succeeded"))
            Catch ex As Exception
                _client.QueueSendWhisper(user.Name, "Failed: {0}".Frmt(ex.Summarize))
            End Try
        End Function

        Protected Overrides Function PerformDispose(ByVal finalizing As Boolean) As Task
            For Each hook In _hooks
                Contract.Assume(hook IsNot Nothing)
                hook.ContinueWithAction(Sub(value) value.Dispose()).IgnoreExceptions()
            Next hook
            _client.Dispose()
            _control.AsyncInvokedAction(Sub() _control.Dispose()).IgnoreExceptions()
            Return Nothing
        End Function

        Private _autoHook As Task(Of IDisposable)
        Private Async Sub SetAutomatic(ByVal slaved As Boolean)
            'Do nothing if already in the correct state
            If slaved = (_autoHook IsNot Nothing) Then
                Return
            End If

            If slaved Then
                _autoHook = _bot.QueueCreateActiveGameSetsAsyncView(
                    adder:=Sub(sender, server, gameSet)
                               If gameSet.GameSettings.IsAdminGame Then Return
                               _client.QueueAddAdvertisableGame(gameDescription:=gameSet.GameSettings.GameDescription,
                                                                isPrivate:=gameSet.GameSettings.IsPrivate)
                           End Sub,
                    remover:=Sub(sender, server, gameSet)
                                 _client.QueueRemoveAdvertisableGame(gameSet.GameSettings.GameDescription)
                             End Sub)
            Else
                Dim oldHook = _autoHook
                _autoHook = Nothing
                Await oldHook
                oldHook.Dispose()
            End If
        End Sub
        Public Function QueueSetAutomatic(ByVal slaved As Boolean) As Task
            Contract.Ensures(Contract.Result(Of Task)() IsNot Nothing)
            Return inQueue.QueueAction(Sub() SetAutomatic(slaved))
        End Function

        Private Property UserGameSet(ByVal user As BotUser) As WC3.GameSet
            Get
                Contract.Requires(user IsNot Nothing)
                If Not _userGameSetMap.ContainsKey(user) Then Return Nothing
                Return _userGameSetMap(user)
            End Get
            Set(ByVal value As WC3.GameSet)
                Contract.Requires(user IsNot Nothing)
                If value Is Nothing Then
                    _userGameSetMap.Remove(user)
                Else
                    _userGameSetMap(user) = value
                End If
            End Set
        End Property
        Public Function QueueTryGetUserGameSet(ByVal user As BotUser) As Task(Of WC3.GameSet)
            Contract.Requires(user IsNot Nothing)
            Contract.Ensures(Contract.Result(Of Task(Of WC3.GameSet))() IsNot Nothing)
            Return inQueue.QueueFunc(Function() UserGameSet(user))
        End Function
        Public Function QueueSetUserGameSet(ByVal user As BotUser, ByVal gameSet As WC3.GameSet) As Task
            Contract.Requires(user IsNot Nothing)
            Contract.Ensures(Contract.Result(Of Task)() IsNot Nothing)
            Return inQueue.QueueAction(Sub() UserGameSet(user) = gameSet)
        End Function
        Public Function QueueResetUserGameSet(ByVal user As BotUser, ByVal gameSet As WC3.GameSet) As Task
            Contract.Requires(user IsNot Nothing)
            Contract.Ensures(Contract.Result(Of Task)() IsNot Nothing)
            Return inQueue.QueueAction(Sub() If UserGameSet(user) Is gameSet Then UserGameSet(user) = Nothing)
        End Function

        Private Function IncludeCommandImpl(ByVal command As ICommand(Of IBotComponent)) As Task(Of IDisposable) Implements IBotComponent.IncludeCommand
            Return IncludeCommand(command)
        End Function
        Public Function IncludeCommand(ByVal command As ICommand(Of ClientManager)) As Task(Of IDisposable)
            Contract.Requires(command IsNot Nothing)
            Contract.Ensures(Contract.Result(Of Task(Of IDisposable))() IsNot Nothing)
            Return _commands.IncludeCommand(command).AsTask()
        End Function
    End Class
End Namespace
