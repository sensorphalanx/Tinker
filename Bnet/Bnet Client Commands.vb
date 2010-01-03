Imports Tinker.Bot
Imports Tinker.Commands

Namespace Bnet
    Public NotInheritable Class ClientCommands
        Inherits CommandSet(Of Bnet.ClientManager)

        Public Sub New()
            'AddCommand(AdLink)
            'AddCommand(AdUnlink)
            AddCommand(New CBot())
            AddCommand(AddUser)
            AddCommand(Demote)
            AddCommand(RemoveUser)
            AddCommand(Disconnect)
            AddCommand(New Bot.GenericCommands.FindMaps(Of Bnet.ClientManager))
            AddCommand(New Bot.GenericCommands.DownloadMap(Of Bnet.ClientManager))
            AddCommand(Promote)
            AddCommand(User)
            AddCommand(Connect)
            AddCommand(LogOn)
            AddCommand(AdminCode)
            AddCommand(CancelHost)
            AddCommand(Elevate)
            AddCommand(Game)
            AddCommand(Host)
            AddCommand(Say)
            AddCommand(StartAdvertising)
            AddCommand(StopAdvertising)
            AddCommand(RefreshGamesList)
            AddCommand(Auto)
        End Sub

        Public Overloads Function AddCommand(ByVal command As Command(Of Bnet.Client)) As IDisposable
            Contract.Requires(command IsNot Nothing)
            Contract.Ensures(Contract.Result(Of IDisposable)() IsNot Nothing)
            Return AddCommand(New ProjectedCommand(Of Bnet.ClientManager, Bnet.Client)(
                    command:=command,
                    projection:=Function(manager) manager.Client))
        End Function

        Private NotInheritable Class CBot
            Inherits Command(Of Bnet.ClientManager)
            Public Sub New()
                MyBase.New(Name:="Bot",
                           Format:="...",
                           Description:="Forwards commands to the main bot.",
                           Permissions:="root:1")
            End Sub
            Protected Overrides Function PerformInvoke(ByVal target As ClientManager, ByVal user As BotUser, ByVal argument As String) As IFuture(Of String)
                Return target.Bot.InvokeCommand(user, argument)
            End Function
        End Class

        Private Shared ReadOnly Disconnect As New DelegatedTemplatedCommand(Of Bnet.Client)(
            Name:="Disconnect",
            template:="",
            Description:="Disconnects from bnet.",
            Permissions:="root:4",
            func:=Function(target, user, argument)
                      Return target.QueueDisconnect(expected:=True, reason:="Disconnect Command").EvalOnSuccess(Function() "Disconnected")
                  End Function)

        Private Shared ReadOnly AddUser As New DelegatedTemplatedCommand(Of Bnet.Client)(
            Name:="AddUser",
            template:="username",
            Description:="Adds a new user, with the default new user permissions.",
            Permissions:="users:2",
            func:=Function(target, user, argument)
                      Dim newUser = target.profile.users.CreateNewUser(argument.RawValue(0))
                      Return "Created {0}".Frmt(newUser.Name).Futurized
                  End Function)

        Private Shared ReadOnly RemoveUser As New DelegatedTemplatedCommand(Of Bnet.Client)(
            Name:="RemoveUser",
            template:="username",
            Description:="Removes an existing user.",
            Permissions:="users:4",
            func:=Function(target, user, argument)
                      If Not target.profile.users.ContainsUser(argument.RawValue(0)) Then
                          Throw New ArgumentException("That user does not exist")
                      End If
                      Dim targetUser = target.profile.users(argument.RawValue(0))
                      Contract.Assume(targetUser IsNot Nothing)
                      If user IsNot Nothing AndAlso Not targetUser < user Then
                          Throw New ArgumentException("You can only destroy users with lower permissions.")
                      End If
                      target.profile.users.RemoveUser(argument.RawValue(0))
                      Return "Removed {0}".Frmt(argument.RawValue(0)).Futurized
                  End Function)

        Private Shared ReadOnly Promote As New DelegatedTemplatedCommand(Of Bnet.Client)(
            Name:="Promote",
            template:="username targetPermission newLevel",
            Description:="Increases a user's permission's level.",
            Permissions:="users:2",
            func:=Function(target, user, argument)
                      'Target user
                      If Not target.profile.users.ContainsUser(argument.RawValue(0)) Then
                          Throw New ArgumentException("That user does not exist")
                      End If
                      Dim target_user As BotUser = target.profile.users(argument.RawValue(0))

                      'Level
                      Dim lvl As UInteger
                      If Not UInteger.TryParse(argument.RawValue(2), lvl) Then
                          Throw New ArgumentException("Expected numeric argument for level")
                      End If

                      'Check for demotion in disguise
                      If lvl <= target_user.Permission(argument.RawValue(1)) Then
                          Throw New ArgumentException("That is not a promotion. Jerk.")
                      End If

                      'Check for overpromotion
                      If user IsNot Nothing AndAlso lvl > user.Permission(argument.RawValue(1)) Then
                          Throw New ArgumentException("You can't promote users past your own permission levels.")
                      End If

                      target_user.Permission(argument.RawValue(1)) = lvl
                      Return "{0} had permission in {1} promoted to {2}".Frmt(argument.RawValue(0), argument.RawValue(1), lvl).Futurized
                  End Function)

        Private Shared ReadOnly Demote As New DelegatedTemplatedCommand(Of Bnet.Client)(
            Name:="Demote",
            template:="username targetPermission newLevel",
            Description:="Decreases a user's permission's level.",
            Permissions:="users:3",
            func:=Function(target, user, argument)
                      'Target user
                      If Not target.profile.users.ContainsUser(argument.RawValue(0)) Then
                          Throw New ArgumentException("That user does not exist")
                      End If
                      Dim targetUser = target.profile.users(argument.RawValue(0))

                      'Level
                      Dim lvl As UInteger
                      If Not UInteger.TryParse(argument.RawValue(2), lvl) Then
                          Throw New ArgumentException("Expected numeric argument for level")
                      End If

                      'Check for promotion in disguise
                      If lvl >= targetUser.Permission(argument.RawValue(1)) Then
                          Throw New ArgumentException("That is not a demotion.")
                      End If

                      'Check for overdemotion
                      If user IsNot Nothing AndAlso Not targetUser < user Then
                          Throw New ArgumentException("You can only demote users with lower permissions.")
                      End If

                      targetUser.Permission(argument.RawValue(1)) = lvl
                      Return "{0} had permission in {1} demoted to {2}".Frmt(argument.RawValue(0), argument.RawValue(1), lvl).Futurized
                  End Function)

        Private Shared ReadOnly User As New DelegatedTemplatedCommand(Of Bnet.Client)(
            Name:="User",
            template:="?username",
            Description:="Shows a user's permissions.",
            Permissions:="",
            func:=Function(target, user, argument)
                      Dim username = If(argument.TryGetRawValue(0), If(user Is Nothing, Nothing, user.Name.Value))
                      If username Is Nothing Then
                          Throw New ArgumentException("No user specified.")
                      End If
                      If target.profile.users.ContainsUser(username) Then
                          Return target.profile.users(username).ToString().Futurized
                      ElseIf target.profile.users.ContainsUser(BotUserSet.UnknownUserKey) Then
                          Return "{0} is an unknown user with the permissions of the user '*unknown'".Frmt(username).Futurized
                      Else
                          Return "{0} is an ignored unknown user.".Frmt(username).Futurized
                      End If
                  End Function)

        Private Shared ReadOnly Auto As New DelegatedTemplatedCommand(Of Bnet.ClientManager)(
            Name:="Auto",
            template:="On|Off",
            Description:="Causes the client to automatically advertise any games on any server when 'On'.",
            func:=Function(target, user, argument)
                      Select Case New InvariantString(argument.RawValue(0))
                          Case "On"
                              Return target.QueueSetAutomatic(True).EvalOnSuccess(Function() "Now automatically advertising games.")
                          Case "Off"
                              Return target.QueueSetAutomatic(False).EvalOnSuccess(Function() "Now not automatically advertising games.")
                          Case Else
                              Throw New ArgumentException("Must specify 'On' or 'Off' as an argument.")
                      End Select
                  End Function)

        Private Shared ReadOnly LogOn As New DelegatedTemplatedCommand(Of Bnet.Client)(
            Name:="LogOn",
            template:="username password",
            Description:="Identifies and authenticates the client as a particular bnet user.",
            Permissions:="root:4",
            func:=Function(target, user, argument)
                      Return target.QueueLogOn(New Bnet.ClientCredentials(argument.RawValue(0), argument.RawValue(1))).
                                    EvalOnSuccess(Function() "Logged in as {0}".Frmt(argument.RawValue(0)))
                  End Function)

        Private Shared ReadOnly Host As New DelegatedTemplatedCommand(Of Bnet.ClientManager)(
            Name:="Host",
            template:=Concat({"name=<game name>", "map=<search query>"},
                             WC3.GameSettings.PartialArgumentTemplates,
                             WC3.GameStats.PartialArgumentTemplates).StringJoin(" "),
            Description:="Hosts a game in the custom games list. See 'Help Host *' for more help topics.",
            Permissions:="games:1",
            extraHelp:=Concat(WC3.GameSettings.PartialArgumentHelp,
                              WC3.GameStats.PartialArgumentHelp).StringJoin(Environment.NewLine),
            func:=Function(target, user, argument)
                      Dim futureServer = target.Bot.QueueGetOrConstructGameServer()
                      Dim futureGameSet = (From server In futureServer
                                           Select server.QueueAddGameFromArguments(argument, user)
                                           ).Defuturized
                      Dim futureAdvertised = futureGameSet.select(
                          Function(gameSet)
                              Dim result = target.Client.QueueStartAdvertisingGame(gameDescription:=gameSet.GameSettings.GameDescription,
                                                                                   isPrivate:=gameSet.GameSettings.IsPrivate)
                              Dim onStarted As WC3.GameSet.StateChangedEventHandler
                              onStarted = Sub(sender, active)
                                              If active Then Return
                                              RemoveHandler gameset.StateChanged, onStarted
                                              target.Client.QueueStopAdvertisingGame(id:=gameSet.GameSettings.GameDescription.GameId, reason:="Game Started")
                                          End Sub
                              AddHandler gameSet.StateChanged, onStarted
                              Return result
                          End Function).Defuturized
                              futureAdvertised.Catch(Sub() If futureGameSet.State = FutureState.Succeeded Then futuregameset.value.dispose())
                              Dim futureDesc = futureAdvertised.EvalOnSuccess(Function() futureGameSet.Value.GameSettings.GameDescription)
                              Return futureDesc.select(Function(desc) "Hosted game '{0}' for map '{1}'".Frmt(desc.name, desc.GameStats.relativePath))
                          End Function)

        Private Shared ReadOnly Game As New DelegatedPartialCommand(Of Bnet.Client)(
            Name:="Game",
            headType:="InstanceName",
            Description:="Forwards commands to an instance in your hosted game. The default instance is '0'.",
            func:=Function(target, user, head, rest)
                      'Find hosted server, then find game, then pass command
                      'Return target.QueueGetUserServer(user).Select(
                      'Function(server)
                      'If server Is Nothing Then
                      'Throw New InvalidOperationException("You don't have a hosted game to forward that command to.")
                      'End If

                      ''Find game, then pass command
                      'Return server.QueueFindGame(head).Select(
                      'Function(game)
                      'If game Is Nothing Then
                      'Throw New InvalidOperationException("No game with that name.")
                      'End If

                      ''Pass command
                      'Return game.QueueCommandProcessText(target.Parent, Nothing, rest)
                      'End Function
                      ').Defuturized()
                      'End Function
                      ').Defuturized()
                      Throw New NotImplementedException
                  End Function)

        Private Shared ReadOnly RefreshGamesList As New DelegatedTemplatedCommand(Of Bnet.Client)(
            Name:="RefreshGamesList",
            template:="",
            Description:="Refreshes the bot's game list display. No useful effect from bnet.",
            Permissions:="local:1",
            func:=Function(target, user, argument)
                      Return target.QueueSendPacket(Bnet.Packet.MakeQueryGamesList()).EvalOnSuccess(Function() "Sent request.")
                  End Function)

        Private Shared ReadOnly CancelHost As New DelegatedTemplatedCommand(Of Bnet.Client)(
            Name:="CancelHost",
            template:="",
            Description:="Cancels a host command if it was issued by you, and unlinks the attached server.",
            Permissions:="",
            func:=Function(target, user, argument)
                      Return target.QueueGetUserServer(user).Select(
                          Function(server)
                              If server Is Nothing Then
                                  Throw New InvalidOperationException("You don't have a hosted game to cancel.")
                              End If

                              server.Dispose()
                              Return "Cancelled hosting."
                          End Function
                      )
                          End Function)

        Private Shared ReadOnly AdminCode As New DelegatedTemplatedCommand(Of Bnet.Client)(
            Name:="AdminCode",
            template:="",
            Description:="Returns the admin code for a game you have hosted.",
            Permissions:="",
            func:=Function(target, user, argument) target.QueueGetUserServer(User).Select(
                      Function(server)
                          If server Is Nothing Then
                              Throw New InvalidOperationException("You don't have a hosted game to cancel.")
                          End If
                          Throw New NotImplementedException
                          Return ""
                      End Function))

        Private Shared ReadOnly Say As New DelegatedTemplatedCommand(Of Bnet.Client)(
            Name:="Say",
            template:="text",
            Description:="Causes the bot to say the given text, including any bnet commands.",
            Permissions:="root:1",
            func:=Function(target, user, argument)
                      target.QueueSendText(argument.RawValue(0))
                      Return "Said {0}".Frmt(argument.RawValue(0)).Futurized
                  End Function)

        Private Shared ReadOnly StartAdvertising As New DelegatedTemplatedCommand(Of Bnet.Client)(
            Name:="StartAdvertising",
            template:="name=<game name> map=<search query> -private -p",
            Description:="Places a game in the custom games list, but doesn't start a new game server to accept the joiners.",
            Permissions:="root:4,games:5",
            func:=Function(target, user, argument)
                      Dim map = WC3.Map.FromArgument(argument.NamedValue("map"))
                      Return target.QueueStartAdvertisingGame(
                          WC3.LocalGameDescription.FromArguments(argument.NamedValue("name"),
                                                          map,
                                                          New WC3.GameStats(map, If(user Is Nothing, Application.ProductName, user.Name.Value), argument)),
                          isPrivate:=argument.HasOptionalSwitch("Private") OrElse argument.hasoptionalswitch("p")).EvalOnSuccess(Function() "Started advertising")
                  End Function)

        Private Shared ReadOnly StopAdvertising As New DelegatedTemplatedCommand(Of Bnet.Client)(
            Name:="StopAdvertising",
            template:="",
            Description:="Stops placing a game in the custom games list and unlinks from any linked server.",
            Permissions:="root:4,games:5",
            func:=Function(target, user, argument)
                      Return target.QueueStopAdvertisingGame("Advertising stopped manually.").EvalOnSuccess(Function() "Stopped advertising.")
                  End Function)

        Private Shared ReadOnly Elevate As New DelegatedTemplatedCommand(Of Bnet.Client)(
            Name:="Elevate",
            template:="-player=name",
            Description:="Elevates you or a specified player to admin in your hosted game.",
            Permissions:="games:1",
            func:=Function(target, user, argument)
                      Dim username = If(argument.TryGetOptionalNamedValue("player"),
                                        If(user Is Nothing, Nothing, user.Name.Value))
                      If username Is Nothing Then
                          Throw New ArgumentException("No player specified.")
                      End If

                      'Find hosted server, then find player's game, then elevate player
                      Return target.QueueGetUserServer(user).Select(
                          Function(server)
                              If server Is Nothing Then
                                  Throw New InvalidOperationException("You don't have a hosted game.")
                              End If

                              'Find player's game, then elevate player
                              Return server.QueueFindPlayerGame(username).Select(
                                  Function(game)
                                      If game Is Nothing Then
                                          Throw New InvalidOperationException("No matching user found.")
                                      End If

                                      'Elevate player
                                      Return game.QueueElevatePlayer(username)
                                  End Function
                              )
                                  End Function
                      ).Defuturized.Defuturized.EvalOnSuccess(Function() "'{0}' is now the admin.".Frmt(username))
                          End Function)

        Private Shared ReadOnly Connect As New DelegatedTemplatedCommand(Of Bnet.Client)(
            Name:="Connect",
            template:="Hostname",
            Description:="Connects to a battle.net server at the given hostname.",
            Permissions:="root:4",
            func:=Function(target, user, argument)
                      Return target.QueueConnect(argument.RawValue(0)).EvalOnSuccess(Function() "Established connection to {0}".Frmt(argument.RawValue(0)))
                  End Function)
    End Class
End Namespace
