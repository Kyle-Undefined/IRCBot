Imports System.IO
Imports System.Xml
Imports System.Net
Imports System.Text
Imports System.Data
Imports System.Threading
Imports System.Net.Sockets
Imports System.Collections.ObjectModel
Imports System.Text.RegularExpressions

Imports IRC.Bot.ToolKit

Module Module1

    'TO DO :
    ' 0: ADD Default Master Question
    ' 1: ADD Nick/Auth question
    ' 2: ADD ABC GAME I(5)
    ' 4: ADD Logging
    ' 7: ADD GOOGLE SEARCH I(6)
    ' 8: ADD 8BALL GAME I(7)
    ' 9: ADD RAPE COMMAND I(8)
    '10: ADD UPTIME COMMAND I(9)
    '12: ADD ROULETTE GAME I(10)
    '14: ADD MD5 HASH COMMAND I(11)
    '15: ADD URBANDICTIONARY SEARCH I(12)
    '16: ADD BING SEARCH I(13)
    '17: ADD WIKI SEARCH I(14)
    '19: ADD QUOTE COMMAND I(15)
    '20: ADD Save setting to file and load if saved
    '21: ADD XML Files instead of Text
    '22: ADD Word censoring

    '* == Important

#Region "Private WithEvents"
    Private WithEvents _irc As New ToolKit.IRC("Rawr! (RawrBot 1.0 Beta)") 'Declares a new IRC with its CTCP Version
    Private WithEvents t As New Threading.Thread(AddressOf Run)
#End Region

#Region "Private Variables"
    Private Server As String
    Private Port As String
    Private Nick As String
    Private Pass As String
    Private Channel As String
    Private CommandCharacter As String
    Private Lock As Boolean = True
	Private Limit As Integer = 2
    Private didRemove As Integer
    Private _path As String = My.Application.Info.DirectoryPath + "\Settings\"
    Private Cmds() As String = {"Lock", "Quit", "Rawr", "Slap", "Char", "Abc", "List", "G", "8Ball", "Rape", "UpTime", "Masters", "Roulette", "MD5", "UD", "B", "Wiki", "Quote", "?Command"}
    Private Args() As String = {"On/Off", "None", "None", "None", "Desired Char", "Guessed Letter", "None", "Search Term", "Question", "Victim", "None", "None", "Victim", "String", "Search Term", "Search Term", "Search Term", "Add/Remove/None", "None"}
    Private I As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
#End Region

#Region "Public Properties"
    Public ReadOnly Property Path() As String
        Get
            Return _path
        End Get
    End Property
#End Region

#Region "Methods"

#Region "RemoveLine(s)/RandomNumber"
    Private Sub RemoveLine(ByVal fileName As String, ByVal lineToRemove As String)
        Dim str() As String = File.ReadAllLines(fileName)

        Using sw As New IO.StreamWriter(fileName)
            For Each Line As String In str
                If (Not (Line.Contains(lineToRemove))) Then
                    sw.WriteLine(Line)
                End If
            Next
        End Using
    End Sub

    Public Function RandomNumber(ByVal low As Int32, ByVal high As Int32) As Integer
        Static RandomNumGen As New System.Random
        Return RandomNumGen.Next(low, high + 1)
    End Function

    'Private Sub RemoveLines(ByVal fileName As String, ByVal linesToRemove() As String)
    '    Dim lines() As String = IO.File.ReadAllLines(fileName)
    '    Using sw As New IO.StreamWriter(fileName)
    '        For Each line As String In lines
    '            If Array.IndexOf(linesToRemove, line) Then
    '                sw.WriteLine(line)
    '            End If
    '        Next
    '    End Using
    'End Sub
#End Region

#Region "SetUpApp"
    Private Sub SetupApp()
        SetUpDirectory()
        File.WriteAllText(Path + "Authed.txt", Nothing)
    End Sub

    Private Sub SetUpDirectory()
		'SetupSettingsXML()
        If (Not Directory.Exists(Path)) Then
            Directory.CreateDirectory(Path)
        End If
        If (Not File.Exists(Path + "CmdChar.txt")) Then
            File.AppendAllText(Path + "CmdChar.txt", Nothing)
        End If
        If (Not File.Exists(Path + "Masters.txt")) Then
            File.AppendAllText(Path + "Masters.txt", Nothing)
        End If
        If (Not File.Exists(Path + "Quotes.txt")) Then
            File.AppendAllText(Path + "Quotes.txt", Nothing)
        End If
    End Sub

    Private Sub SetupSettingsXML()
        Dim xmlDoc As XmlDocument = New XmlDocument()

        If Not File.Exists(Path + "Settings.xml") Then
            Dim xmlWriter As XmlTextWriter = New XmlTextWriter(Path + "Settings.xml", System.Text.Encoding.UTF8)

            xmlWriter.Formatting = Formatting.Indented
            xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'")
            xmlWriter.WriteStartElement("Settings")

            xmlWriter.WriteElementString("Server", "")
            xmlWriter.WriteElementString("Port", "")

            xmlWriter.WriteEndElement()
            xmlWriter.Close()

            xmlDoc.Save(Path + "Settings.xml")
        End If
    End Sub
#End Region

#Region "isAuthed/Master"
    Private Function isMaster(ByVal Nick As String) As Boolean
        Dim Master As String = File.ReadAllText(Path + "Masters.txt")
        Dim Masters() As String = Split(Master, vbCrLf)

        For I As Integer = 0 To UBound(Masters)
            If (Not (Masters(I) = "")) Then
                If (Nick = Mid(Masters(I), 1, Masters(I).IndexOf(" "))) Then
                    Return True
                End If
            End If
        Next

        Return False
    End Function

    Private Function isAuthed(ByVal Nick As String) As Boolean
        Dim Auth As String = File.ReadAllText(Path + "Authed.txt")
        Dim Auths() As String = Split(Auth, vbCrLf)

        For I As Integer = 0 To UBound(Auths)
            If (Nick = Auths(I)) Then
                Return True
                Exit For
            End If
        Next

        Return False
    End Function
#End Region

#Region "Sub Main"
    Sub Main()
        With _irc
            'Sets up the application
            SetupApp()

            ' Lets the user fill in what Server, Port, UserName, Nick, Pass, and Channel
            Console.Write("Server : ")
            Server = Console.ReadLine
            Console.Write("Port : ")
            Port = Console.ReadLine
            Console.Write("Nick : ")
            Nick = Console.ReadLine
            Console.Write("Pass(If Needed) : ")
            Pass = Console.ReadLine
            Console.Write("Channel : ")
            Channel = Console.ReadLine
            If (File.ReadAllText(Path + "CmdChar.txt") = "") Then
                Console.Write("Command Character : ")
                CommandCharacter = Console.ReadLine
                File.AppendAllText(Path + "CmdChar.txt", CommandCharacter)
            End If

            ' Does the connecting
            .Connect(Server, Port) 'Connects to the server
            .FingerInfo = "RAWR!" ' Sets the CTCP Finger info
            If (Not (Pass = "")) Then
                .Logon("OhHai", Nick)
            Else
                .Logon("OhHai", Nick, Pass)
            End If
            .ProcessEvents(35)
			.SendRawMessage("PRIVMSG NickServ :IDENTIFY " & Pass)
            .ProcessEvents(5)
            .Join(Channel)
        End With

        t.IsBackground = True
        t.Start() 'Starts a connecting thread.

        While True
            Threading.Thread.Sleep(50) 'Keeps you bot from not using a lot of memory. NOTE: this while statement is only needed for console apps.
        End While
    End Sub

    Public Sub Run()
        _irc.ProcessEvents(0) 'This call may lock up the bot so call it on a thread
    End Sub
#End Region

#Region "UserKicked"
    Private Sub _irc_UserKicked(ByVal sender As Object, ByVal chan As String, ByVal usr As User, ByVal reason As String, ByVal kicker As User) Handles _irc.IRCUserKick
        If (usr.Nick = Nick) Then
            _irc.Join(chan)
        End If

        If (isAuthed(usr.Nick)) Then
            RemoveLine(Path + "Authed.txt", usr.Nick)
        End If
    End Sub
#End Region

#Region "JoinUser"
    Private Sub _irc_JoinUser(ByVal sender As Object, ByVal user As User, ByVal chan As String) Handles _irc.IRCUserJoin
        If (isMaster(user.Nick)) Then
            _irc.SendNotice(user.Nick, "You have just joined the channel " + chan + " and are not Authed with me, if you would like to Auth then please /msg " + Nick + " Auth " + user.Nick + " <pass>")
        End If
    End Sub
#End Region

#Region "QuitUser"
    Private Sub _irc_QuitUser(ByVal sender As Object, ByVal user As User, ByVal reason As String) Handles _irc.IRCUserQuit
        If (isAuthed(user.Nick)) Then
            RemoveLine(Path + "Authed.txt", user.Nick)
        End If
    End Sub
#End Region

#Region "PartUser"
    Private Sub _irc_PartUser(ByVal sender As Object, ByVal user As User, ByVal chan As String, ByVal reason As String) Handles _irc.IRCUserPart
        If (isAuthed(user.Nick)) Then
            RemoveLine(Path + "Authed.txt", user.Nick)
        End If
    End Sub
#End Region

#Region "NickChange"
    Private Sub _irc_NickChange(ByVal oldnick As String, ByVal newnick As String) Handles _irc.IRCNickChange
        If (isAuthed(oldnick)) Then
            RemoveLine(Path + "Authed.txt", oldnick)
            File.AppendAllText(Path + "Authed.txt", newnick + vbCrLf)
            _irc.SendNotice(newnick, "Your new nick has Masters permission")
        End If
    End Sub
#End Region

#Region "PMRecieved"
    Private Sub _irc_PMRecieved(ByVal message As String, ByVal user As User) Handles _irc.IRCPrivateMSGRecieved
        Dim msg = message.ToLower

        If (msg.StartsWith("join")) Then
            If (isAuthed(user.Nick)) Then
                Dim chan As String = Mid(message, message.LastIndexOf(" ") + 2, message.Length)

                _irc.Join(chan)
            Else
                _irc.SendNotice(user.Nick, "You either have not Authed with me, or you do not have permission. If you are a Master and haven't Authed then please /msg " + Nick + " Auth " + user.Nick + " <pass>")
            End If
        End If

        If (msg.StartsWith("add")) Then
            If (Not (isMaster(user.Nick))) Then
                Dim userAdd As String = Mid(message, message.LastIndexOf(" ") + 2, message.Length)

                File.AppendAllText(Path + "Masters.txt", user.Nick + " " + userAdd + vbCrLf)
                File.AppendAllText(Path + "Authed.txt", user.Nick + vbCrLf)
                _irc.SendNotice(user.Nick, user.Nick + " you have been added as a Master and automatically Authed")
            Else
                _irc.SendNotice(user.Nick, "You're already added as a Master")
            End If
        End If

        If (msg.StartsWith("auth")) Then
            If (isMaster(user.Nick)) Then
                If (Not (isAuthed(user.Nick))) Then
                    Dim userAuth As String = user.Nick + " " + Mid(message, message.LastIndexOf(" ") + 2, message.Length)

                    Dim Master As String = File.ReadAllText(Path + "Masters.txt")
                    Dim Masters() As String = Split(Master, vbCrLf)

                    For I As Integer = 0 To UBound(Masters)
                        If (userAuth = Masters(I)) Then
                            File.AppendAllText(Path + "Authed.txt", user.Nick + vbCrLf)
                            _irc.SendNotice(user.Nick, "You have been Authed with me, you may now use Master commands")
                            Exit For
                        End If
                    Next
                Else
                    _irc.SendNotice(user.Nick, "You're already Authed with me")
                End If
            Else
                _irc.SendNotice(user.Nick, "You are not on the Master list")
            End If
        End If
    End Sub
#End Region

#Region "IRCMessageRecieved"
    Private Sub _irc_IRCMessageRecieved(ByVal sender As Object, ByVal message As String, ByVal user As User, ByVal chan As IRCChannel) Handles _irc.IRCMessageRecieved
        Dim Cmd As String = File.ReadAllText(Path + "CmdChar.txt")
        Dim msg = message.ToLower

        ' Lock command, locks and unlocks the bot
        If (msg = Cmd + "lock on") Then
            If (I(0) < Limit) Then
                If (isAuthed(user.Nick)) Then
                    If (Not (Lock)) Then
                        _irc.SendMessage("Bot is locked!", chan.Channel)
                        Lock = True
                        I(0) = I(0) + 1
                    Else
                        _irc.SendMessage("Bot is already locked", chan.Channel)
                        I(0) = I(0) + 1
                    End If
                Else
                    _irc.SendNotice(user.Nick, "You either have not Authed with me, or you do not have permission. If you are a Master and haven't Authed then please /msg " + Nick + " Auth " + user.Nick + " <pass>")
                End If
            Else
                Threading.Thread.Sleep(3000)
                I(0) = 0
            End If
        ElseIf (msg = Cmd + "lock off") Then
            If (I(0) < Limit) Then
                If (isAuthed(user.Nick)) Then
                    If (Lock) Then
                        _irc.SendMessage("Bot Is Unlocked!", chan.Channel)
                        Lock = False
                        I(0) = I(0) + 1
                    Else
                        _irc.SendMessage("Bot is already unlocked", chan.Channel)
                        I(0) = I(0) + 1
                    End If
                Else
                    _irc.SendNotice(user.Nick, "You either have not Authed with me, or you do not have permission. If you are a Master and haven't Authed then please /msg " + Nick + " Auth " + user.Nick + " <pass>")
                End If
            Else
                Threading.Thread.Sleep(3000)
                I(0) = 0
            End If
        ElseIf (msg = "lock?") Then
            _irc.SendNotice(user.Nick, "My current lock status is " + Lock.ToString)
        ElseIf (msg = "char?") Then
            _irc.SendNotice(user.Nick, "My current command character is " + Cmd)
        End If

        ' Quit command, makes the bot leave the server
        If (msg = Cmd + "quit") Then
            If (isAuthed(user.Nick)) Then
                _irc.SendMessage("Quit command recognized...Leaving thanks to " + user.Nick, chan.Channel)
                _irc.Disconnect("I Was Killed by " + user.Nick)
            Else
                _irc.SendNotice(user.Nick, "You either have not Authed with me, or you do not have permission. If you are a Master and haven't Authed then please /msg " + Nick + " Auth " + user.Nick + " <pass>")
            End If
        End If

        ' Part command, makes the bot leave the channel
        If (msg = Cmd + "part") Then
            If (isAuthed(user.Nick)) Then
                _irc.Part(chan.Channel, "I Was Sent Away by " + user.Nick)
            Else
                _irc.SendNotice(user.Nick, "You either have not Authed with me, or you do not have permission. If you are a Master and haven't Authed then please /msg " + Nick + " Auth " + user.Nick + " <pass>")
            End If
        End If

        ' Does the help command, explains the desired command
        Dim theCmd As String = Mid(message, 2, message.Length)
        If (msg = "?" + theCmd.ToLower) Then
            Select Case theCmd.ToLower
                Case "lock"
                    _irc.SendNotice(user.Nick, "Command : Lock | Args : On/Off | Ex. $Lock On | Anyone can lock the bot, but you must have Master privelages to unlock the bot | My current lock status is " + Lock.ToString)
                Case "quit"
                    _irc.SendNotice(user.Nick, "Command : Quit | Args : None | Ex. $Quit | You must have Master privelages to use the $Quit command")
                Case "rawr"
                    _irc.SendNotice(user.Nick, "Command : Rawr | Args : None | Ex. Rawr/rawr | This command is to check to see if the bot is locked up")
                Case "slap"
                    _irc.SendNotice(user.Nick, "Command : Slap | Args : None | Ex. $Slap | Slaps a random person from the IRC user list in the current channel")
                Case "char"
                    _irc.SendNotice(user.Nick, "Command : Char | Args : Desired Command Character | Ex. $Char * | You must have Master privelages to change the command character | My current command character is " + Cmd)
                Case "abc"
                    _irc.SendNotice(user.Nick, "Command : Abc | Args : Desired letter to guess | Ex. $Abc s | This a random letter guessing game, can you guess the correct letter?")
                Case "list"
                    _irc.SendNotice(user.Nick, "Command : List | Args : None | Ex. $List | This command lists all of the commands for the bot")
                Case "g"
                    _irc.SendNotice(user.Nick, "Command : G | Args : Search Term | Ex. $G IRC | Searches Google for the search term and returns the first result")
                Case "8ball"
                    _irc.SendNotice(user.Nick, "Command : 8Ball | Args : Question | Ex. $8Ball Is IRC cool? | Uses the 8Ball method of randomly answering your question")
                Case "rape"
                    _irc.SendNotice(user.Nick, "Command : Rape | Args : Victim | Ex. $Rape Wizzup | Rapes the victime")
                Case "uptime"
                    _irc.SendNotice(user.Nick, "Command : UpTime | Args : None | Ex. $UpTime | Displays how long the bot has been up for")
                Case "masters"
                    _irc.SendNotice(user.Nick, "Command : Masters | Args : None | Ex. $Masters | Displays who the bots Masters are")
                Case "roulette"
                    _irc.SendNotice(user.Nick, "Command : Roulette | Args : None | Ex. $Roulette | Randomly kills someone from the IRC user list in the current channel")
                Case "md5"
                    _irc.SendNotice(user.Nick, "Command : MD5 | Args : String | Ex. $MD5 Hai | MD5 hashes the requested string and prints it out")
                Case "ud"
                    _irc.SendNotice(user.Nick, "Command : UD | Args : Search Term | Ex. $UD Rage | Searches Urban Dictionary for the search term entered and returns the first result")
                Case "b"
                    _irc.SendNotice(user.Nick, "Command : B | Args : Search Term | Ex. $B IRC | Searches Bing for the search term entered and returns the first result")
                Case "wiki"
                    _irc.SendNotice(user.Nick, "Command : Wiki | Args : Search Term | Ex. $Wiki IRC | Searches Wikipedia for the search term entered and returns the first result")
                Case "quote"
                    _irc.SendNotice(user.Nick, "Command : Quote | Args : Add/Remove/None | Ex. $Quote Add <Camo`>Oh hai! | Adds/Removes a quote to the quote database, to add/remove quotes you must have Master privelages. If no arguments are passed then it displays a random quote that is stored.")
                Case "command"
                    _irc.SendNotice(user.Nick, "Command : ?Command | Args : None | Ex. ?Char | Tells the command name, the command arguements, an example on how to use the command, and if you need Master privelages to use it")
            End Select
        End If

        ' List Command, lists all of the available commands
        If (msg = Cmd + "list") Then
            Dim myCommands As StringBuilder = New StringBuilder()

            For I As Integer = 0 To UBound(Cmds)
                myCommands.Append(Cmds(I) + " : " + Args(I) + " | ")
            Next

            _irc.SendNotice(user.Nick, myCommands.Append(" /End of List").ToString)
        End If

        If (Not Lock) Then
            ' Rawr command, to make sure the bot isn't locked up
            If (msg = "rawr") Then 'Checks if the message starts with Rawr
                If (I(1) <= Limit) Then
                    _irc.SendMessage(user.Nick + ": " + "Rawr!", ToolKit.IRC.SupportedColors.Black, chan.Channel) 'Sends Rawr! back
                    I(1) = I(1) + 1
                Else
                    Threading.Thread.Sleep(3000)
                    I(1) = 0
                End If
            End If

            ' Slap command, slaps a random nick
            If (msg = Cmd + "slap") Then
                If (I(2) <= Limit) Then
                    _irc.SendAction("slaps " + chan.Users(RandomNumber(0, chan.Users.Length - 1)) + " around a bit with an over pixelated electronic trout! (Thanks to " + user.Nick + ")", chan.Channel)
                    I(2) = I(2) + 1
                Else
                    Threading.Thread.Sleep(3000)
                    I(2) = 0
                End If
            End If

            ' Masters Command, says who the bots master is)
            If (msg = Cmd + "masters") Then
                If (I(3) <= Limit) Then
                    Dim Master As String = File.ReadAllText(Path + "Masters.txt")
                    Dim Masters() As String = Split(Master, vbCrLf)
                    Dim myMasters As StringBuilder = New StringBuilder()

                    For I As Integer = 0 To UBound(Masters)
                        If (Not (Masters(I) = "")) Then
                            myMasters.Append(Mid(Masters(I), 1, Masters(I).IndexOf(" ")) + " | ")
                        End If
                    Next

                    _irc.SendMessage("My Masters are " + myMasters.ToString, chan.Channel)
                    I(3) = I(3) + 1
                Else
                    Threading.Thread.Sleep(3000)
                    I(3) = 0
                End If
            ElseIf (msg.StartsWith(Cmd + "masters add")) Then
                Dim userAdd As String = Mid(message, message.LastIndexOf(" ") + 2, message.Length)
                If (isAuthed(user.Nick)) Then
                    If (Not (isMaster(userAdd))) Then
                        _irc.SendNotice(userAdd, "Please /msg " + Nick + " Add " + userAdd + " <pass>")
                    Else
                        _irc.SendNotice(user.Nick, "User " + userAdd + " is already a Master")
                    End If
                Else
                    _irc.SendNotice(user.Nick, "You either have not Authed with me, or you do not have permission. If you are a Master and haven't Authed then please /msg " + Nick + " Auth " + user.Nick + " <pass>")
                End If
            ElseIf (msg.StartsWith(Cmd + "masters remove")) Then
                Dim userR As String = Mid(message, message.LastIndexOf(" ") + 2, message.Length)

                If (isAuthed(user.Nick)) Then
                    If (isMaster(userR)) Then
                        If (Not (userR.ToLower = "camo`office")) Then
                            RemoveLine(Path + "Masters.txt", userR)
                            RemoveLine(Path + "Authed.txt", userR)
                            _irc.SendNotice(user.Nick, userR + " has been removed from Masters and their Auth has been terminated")
                        Else
                            If (didRemove <= Limit) Then
                                didRemove = didRemove + 1
                                _irc.SendNotice(user.Nick, "Camo` can not be removed from the Masters group, try it again and you will be removed from Masters.")
                                _irc.SendRawMessage("PRIVMSG Camo` :" + user.Nick + " tried removing you from the Masters group " + didRemove.ToString + " time(s)")
                            Else
                                RemoveLine(Path + "Masters.txt", user.Nick)
                                RemoveLine(Path + "Authed.txt", user.Nick)
                                _irc.SendNotice(user.Nick, "You tried removing Camo` from the Masters three times, your Master privilages have been revoked. Good bye")
                                _irc.SendRawMessage("PRIVMSG Camo` :" + user.Nick + " tried removing you from the Masters group 3 times. Their Master privilages have been revoked.")
                                didRemove = 1
                            End If
                        End If
                    Else
                        _irc.SendNotice(user.Nick, "User " + userR + " has already been removed from the Masters")
                    End If
                Else
                    _irc.SendNotice(user.Nick, "You either have not Authed with me, or you do not have permission. If you are a Master and haven't Authed then please /msg " + Nick + " Auth " + user.Nick + " <pass>")
                End If
            End If

            ' Change Command Char
            If (msg.StartsWith(Cmd + "char")) Then
                If (I(4) <= Limit) Then
                    If (isAuthed(user.Nick)) Then
                        Dim cmdChar As String

                        cmdChar = Mid(message, message.IndexOf(" ") + 2, message.Length)
                        File.WriteAllText(Path + "CmdChar.txt", cmdChar)
                        _irc.SendMessage("My command character is now " + cmdChar, chan.Channel)

                        I(4) = I(4) + 1
                    Else
                        _irc.SendNotice(user.Nick, "You either have not Authed with me, or you do not have permission. If you are a Master and haven't Authed then please /msg " + Nick + " Auth " + user.Nick + " <pass>")
                        I(4) = I(4) + 1
                    End If
                Else : Threading.Thread.Sleep(3000)
                    I(4) = 0
                End If
            End If
        End If
    End Sub
#End Region

#End Region

#Region "Commands"

#End Region

End Module
