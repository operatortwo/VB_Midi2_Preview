Imports Microsoft.Windows.Devices.Midi2
Imports Microsoft.Windows.Devices.Midi2.Diagnostics
Imports Microsoft.Windows.Devices.Midi2.Messages


' At every new Build or ReBuid, we get a syntax error in WinRTEventHelpers.cs. (Namespace..)
' just correct it to VB-Syntax, the build again --> works (it will happen again at every ReBuild)
'       OR use Referencing, just as in the WPF Example

'----
'Imports System
'Namespace WinRT
'End Namespace
'----

' Requirements
'   to run: Windows 11 Minimal Build 10.0.26100.0 (24H2)
'   to build:  
' Visual Studio 2022 (V 17.14.19) Workload .Net Desktop Development
'                                           (WinUI and C++ Workloads are NOT required nor Windows SDK)
' Project Set To: TargetFramework: .NET 8.0, Target OS: Windows, Target + Minimal Supported Version 10.0.26100.0 (or higher)
' Microsoft.Windows.Devices.Midi2.1.0.13-preview.13.192.nupkg
' https://github.com/microsoft/MIDI/releases/download/preview-13/Microsoft.Windows.Devices.Midi2.1.0.13-preview.13.192.nupkg
' microsoft.windows.cswinrt.2.2.0.nupkg
' 


Module Program
    Sub Main(args As String())
        Console.WriteLine("Hello Developer... Testing access to Midi2 Library")
        Console.WriteLine("-----------")
        Function1()
        'Console.WriteLine("-----------")
        '--- wait for user to terminate ---
        Console.WriteLine("Press ESC to close the Console")
        Do
            While Not Console.KeyAvailable
            End While
        Loop While Console.ReadKey(True).Key <> ConsoleKey.Escape
    End Sub

    Function Function1() As Boolean

        Dim BuildVersion As String
        BuildVersion = Microsoft.Windows.Devices.Midi2.Common.MidiNuGetBuildInformation.BuildFullVersion
        Console.WriteLine("Microsoft.Windows.Devices.Midi2  " & BuildVersion)

        Using initializer As Object =
            Microsoft.Windows.Devices.Midi2.Initialization.MidiDesktopAppSdkInitializer.Create()

            '--- check for initializer, gets Nothing if Midi2 is not installed
            If initializer Is Nothing Then
                Console.WriteLine("Failed to create MidiDesktopAppSdkInitializer")
                Return 1
            End If

            '--- initialize SDK runtime
            If initializer.InitializeSdkRuntime() = False Then

                Console.WriteLine("Failed to initialize SDK Runtime")
                Return 1
            End If

            '--- check if 'Windows MIDI Service' is running, try to start if not
            If initializer.EnsureServiceAvailable() = False Then
                Console.WriteLine("Failed to get 'Windows MIDI Service' running")
                Return 1
            Else
                Console.WriteLine("'Windows MIDI Service' is running")
            End If

            '--- Create Midi2 Session
            Console.WriteLine("Creating MidiSession")

            '-- session implements IDisposable
            Using session As MidiSession = MidiSession.Create("API Sample Session")

                Dim endpointAId = MidiDiagnostics.DiagnosticsLoopbackAEndpointDeviceId
                Dim endpointBId = MidiDiagnostics.DiagnosticsLoopbackBEndpointDeviceId

                Console.WriteLine("Connecting to Sender UMP Endpoint: " & endpointAId)
                Console.WriteLine("Connecting to Receiver UMP Endpoint: " & endpointBId)

                Dim sendEndpoint = session.CreateEndpointConnection(endpointAId)
                Dim receiveEndpoint = session.CreateEndpointConnection(endpointBId)

                AddHandler receiveEndpoint.MessageReceived, AddressOf MessageReceivedHandler
                '    receiveEndpoint.MessageReceived += MessageReceivedHandler();

                Console.WriteLine("Opening endpoint connection")

                '--- Original comment from C# Example ---
                '// once you have wired up all your event handlers, added any filters/listeners, etc.
                '// You can open the connection. Doing this will query the cache for the in-protocol 
                '// endpoint information And function blocks. If Not there, it will send out the requests
                '// which will come back asynchronously with responses.

                If receiveEndpoint.Open() = False Then
                    Console.WriteLine("Could not open receive endpoint")
                    Return 1
                End If

                If sendEndpoint.Open() = False Then
                    Console.WriteLine("Could not open send endpoint")
                    Return 1
                End If

                Console.WriteLine("Creating MIDI 1.0 Channel Voice 32-bit UMP")

                '    var ump32 = MidiMessageBuilder.BuildMidi1ChannelVoiceMessage(
                '        MidiClock.Now, // use current timestamp
                '        New MidiGroup(5),      // group 5
                '        Midi1ChannelVoiceMessageStatus.NoteOn,  // 9
                '        New MidiChannel(3),      // channel 3
                '        120,    // note 120 - hex 0x78
                '        100);   // velocity 100 hex 0x64

                Dim ump32 As MidiMessage32 = MidiMessageBuilder.BuildMidi1ChannelVoiceMessage(
                        MidiClock.Now,                                          ' current time
                        New MidiGroup(5),                                       ' Group 5
                        Midi1ChannelVoiceMessageStatus.NoteOn,                  ' NoteOn (9)
                        New MidiChannel(3),                                     ' channel 3
                        120,                                                    ' note 120  (&h78)
                        100)                                                    ' velocity 100  (&h64)

                Console.WriteLine("Sending MessagePacket")
                ' C# sendEndpoint.SendSingleMessagePacket((IMidiUniversalPacket)ump32);
                sendEndpoint.SendSingleMessagePacket(ump32)
                ' // could also use the SendWords methods, etc.

                Console.WriteLine()
                Console.WriteLine(" ** Wait for the message to arrive, and then press a key to cleanup. ** ")
                'Console.ReadLine()

                'Do
                While Not Console.KeyAvailable
                End While
                'Loop While Console.ReadKey(True).Key <> ConsoleKey.Escape

                '// you should unregister the event handler as well
                'receiveEndpoint.MessageReceived -= MessageReceivedHandler();
                RemoveHandler receiveEndpoint.MessageReceived, AddressOf MessageReceivedHandler

                '    // Not strictly necessary if the session Is going out of scope Or Is in a using block
                session.DisconnectEndpointConnection(sendEndpoint.ConnectionId)
                session.DisconnectEndpointConnection(receiveEndpoint.ConnectionId)

            End Using
        End Using

        Return 0
    End Function

    Private Sub MessageReceivedHandler(sender As IMidiMessageReceivedEventSource, args As MidiMessageReceivedEventArgs)

        '// c# allows local functions. This Is nicer than anonymous because we can unregister it by name
        ' Void MessageReceivedHandler(IMidiMessageReceivedEventSource sender, MidiMessageReceivedEventArgs args)

        '!!! ---
        '!!! When processing the MessageReceived event, do so quickly. This event is synchronous.
        '!!! If you need to do long-running processing of incoming messages, add them to your own
        '!!! incoming queue structure and have them processed by another application thread.
        '!!! ---

        ' Console.WriteLine works in Console, Debug.WriteLine works in other Application
        ' Try writing to UI never returns

        Dim ump = args.GetMessagePacket()

        Console.WriteLine()
        Console.WriteLine("--> Received UMP <--")
        Console.WriteLine()

        Console.WriteLine("- Current Timestamp: " & MidiClock.Now)
        Console.WriteLine("- UMP Timestamp:     " & ump.Timestamp)
        Console.WriteLine("- UMP Msg Type:      " & ump.MessageType)
        Console.WriteLine("- UMP Packet Type:   " & ump.PacketType)
        Console.WriteLine("- Message:           " & MidiMessageHelper.GetMessageDisplayNameFromFirstWord(args.PeekFirstWord()))


        Dim tp = ump.GetType

        If ump.GetType Is GetType(MidiMessage32) Then

            Dim ump32 = TryCast(ump, MidiMessage32)

            If ump32 IsNot Nothing Then
                Console.WriteLine("- Word 0:            0x{0:X}", ump32.Word0)
                '  MsgType 2, Group 5, 937864
            End If
        End If

        Console.WriteLine()
        Console.WriteLine("--> End of Message <--")

    End Sub


End Module
