' When the Packages for CsWinRT and Midi2 are directly installed in the Main Project we get at every Build
' a syntax error in WinRTEventHelpers.cs. (Namespace..)
'
'   The Workaround is the C# XProjectionBuilder Console where the CsWinRT and Midi2 packages resides
'   It needs only 1 Build to create the projection Library "Microsoft.Windows.Devices.Midi2.NetProjection.dll"
'   Now we can reference this from the Main Application (add reference, browse, Ok) an then it appears in
'   /[Project]/References/Assemblys

' Requirements
'   to run: Windows 11 Minimal Build 10.0.26100.0
'   to build:  
' Visual Studio 2022 (V 17.14.19) Workload .Net Desktop Development
'                                           (WinUI and C++ Workloads are NOT required nor Windows SDK)
' Project Set To: TargetFramework: .NET 8.0, Target OS: Windows, Target + Minimal Supported Version 10.0.26100.0 (or higher)
' Microsoft.Windows.Devices.Midi2.1.0.13-preview.13.192.nupkg
' https://github.com/microsoft/MIDI/releases/download/preview-13/Microsoft.Windows.Devices.Midi2.1.0.13-preview.13.192.nupkg
' microsoft.windows.cswinrt.2.2.0.nupkg
' 
Imports System.Collections.Concurrent
Imports System.Collections.Specialized.BitVector32
Imports Microsoft.Windows.Devices.Midi2
Imports Microsoft.Windows.Devices.Midi2.Diagnostics
Imports Microsoft.Windows.Devices.Midi2.Initialization
Imports Microsoft.Windows.Devices.Midi2.Messages

Public Class Form1


    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        WriteMessageLine("Hello Developer... Testing access to Midi2 Library")
        WriteMessageLine("-----------")
        If Function1() = 0 Then
            WriteMessageLine("Creating MIDI 1.0 Channel Voice 32-bit UMP")
            WriteMessageLine(" ** Waiting for the message to arrive **")
            WriteMessageLine("---")
            SendTestMessage()
            BtnSendMsg.Enabled = True
        End If
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        StopInputQueue()
        If Initializer IsNot Nothing Then Initializer.Dispose()
        If Session IsNot Nothing Then Session.Dispose()

        'Session.DisconnectEndpointConnection(sendEndpoint.ConnectionId)
        'Session.DisconnectEndpointConnection(receiveEndpoint.ConnectionId)
    End Sub

    Private Initializer As MidiDesktopAppSdkInitializer
    Private Session As MidiSession
    Private SendEndpoint As MidiEndpointConnection
    Private ReceiveEndpoint As MidiEndpointConnection


    Function Function1() As Boolean

        Dim BuildVersion As String
        BuildVersion = Microsoft.Windows.Devices.Midi2.Common.MidiNuGetBuildInformation.BuildFullVersion
        WriteMessageLine("Microsoft.Windows.Devices.Midi2  " & BuildVersion)

        Initializer = MidiDesktopAppSdkInitializer.Create()

        '--- check for initializer, gets Nothing if Midi2 is not installed
        If Initializer Is Nothing Then
            WriteMessageLine("Failed to create MidiDesktopAppSdkInitializer")
            Return 1
        End If

        '--- initialize SDK runtime
        If Initializer.InitializeSdkRuntime() = False Then

            WriteMessageLine("Failed to initialize SDK Runtime")
            Return 1
        End If

        '--- check if 'Windows MIDI Service' is running, try to start if not
        If Initializer.EnsureServiceAvailable() = False Then
            WriteMessageLine("Failed to get 'Windows MIDI Service' running")
            Return 1
        Else
            WriteMessageLine("'Windows MIDI Service' is running")
        End If

        '--- start UI Input queue ---

        WriteMessageLine("Starting Application's Input Queue")
        StartInputQueue()

        '--- Create Midi2 Session
        WriteMessageLine("Creating MidiSession")

        '-- session implements IDisposable
        Session = MidiSession.Create("API Sample Session")

        Dim endpointAId = MidiDiagnostics.DiagnosticsLoopbackAEndpointDeviceId
        Dim endpointBId = MidiDiagnostics.DiagnosticsLoopbackBEndpointDeviceId

        WriteMessageLine("Connecting to Sender UMP Endpoint: " & endpointAId)
        WriteMessageLine("Connecting to Receiver UMP Endpoint: " & endpointBId)

        SendEndpoint = Session.CreateEndpointConnection(endpointAId)
        ReceiveEndpoint = Session.CreateEndpointConnection(endpointBId)

        AddHandler ReceiveEndpoint.MessageReceived, AddressOf MessageReceivedHandler
        '    receiveEndpoint.MessageReceived += MessageReceivedHandler();

        WriteMessageLine("Opening endpoint connection")

        '--- Original comment from C# Example ---
        '// once you have wired up all your event handlers, added any filters/listeners, etc.
        '// You can open the connection. Doing this will query the cache for the in-protocol 
        '// endpoint information And function blocks. If Not there, it will send out the requests
        '// which will come back asynchronously with responses.

        If ReceiveEndpoint.Open() = False Then
            WriteMessageLine("Could not open receive endpoint")
            Return 1
        End If

        If SendEndpoint.Open() = False Then
            WriteMessageLine("Could not open send endpoint")
            Return 1
        End If

        Return 0
    End Function


    Private Sub SendTestMessage()
        If SendEndpoint IsNot Nothing Then
            Dim ump32 As MidiMessage32 = MidiMessageBuilder.BuildMidi1ChannelVoiceMessage(
                       MidiClock.Now,                                          ' current time
                       New MidiGroup(5),                                       ' Group 5
                       Midi1ChannelVoiceMessageStatus.NoteOn,                  ' NoteOn (9)
                       New MidiChannel(3),                                     ' channel 3
                       120,                                                    ' note 120  (&h78)
                       100)                                                    ' velocity 100  (&h64)

            'WriteMessageLine("Sending MessagePacket")
            ' C# sendEndpoint.SendSingleMessagePacket((IMidiUniversalPacket)ump32);
            SendEndpoint.SendSingleMessagePacket(ump32)
        End If
    End Sub


    '--- Input Queue ---

    Private Shared InputTimer As New Timers.Timer(50)        ' 50 ms Timer (= 20 times / second)

    Private InputQueue As New ConcurrentQueue(Of UInteger)
    ' at this point, no more messages are added. count can exceed this value up to +5
    Private Const InputQueueCountLimit = 4000
    ' when Limit is reached, this value is increased by 1 at every new message
    Private DiscardedMidiMessages As Integer


    Private Sub StartInputQueue()
        If InputTimer.Enabled = False Then
            AddHandler InputTimer.Elapsed, AddressOf InputTimer_Elapsed
            InputTimer.Start()
        End If
    End Sub

    Private Sub StopInputQueue()
        If InputTimer.Enabled = True Then
            InputTimer.Stop()
            RemoveHandler InputTimer.Elapsed, AddressOf InputTimer_Elapsed
        End If
    End Sub

    Private Sub InputTimer_Elapsed(sender As Object, e As EventArgs)
        TbMessage.Invoke(New ReadMidiInput_Delegate(AddressOf ReadMidiInput))
    End Sub

    Public Delegate Sub ReadMidiInput_Delegate()
    Private Sub ReadMidiInput()

        While InputQueue.IsEmpty = False

            Dim dword0 As UInteger
            Dim dword1 As UInteger
            Dim dword2 As UInteger
            Dim dword3 As UInteger

            Dim TimestampHigh As UInteger
            Dim timestampLow As UInteger
            Dim Timestamp As ULong

            Dim msgsize As Byte

            InputQueue.TryPeek(dword0)
            msgsize = GetUmpMessageSize(dword0)

            If InputQueue.Count >= msgsize + 2 Then

                InputQueue.TryDequeue(dword0)
                If msgsize >= 2 Then
                    InputQueue.TryDequeue(dword1)
                    If msgsize >= 3 Then
                        InputQueue.TryDequeue(dword2)
                        If msgsize >= 4 Then
                            InputQueue.TryDequeue(dword3)
                        End If
                    End If
                End If

                InputQueue.TryDequeue(TimestampHigh)
                InputQueue.TryDequeue(timestampLow)

                Timestamp = TimestampHigh
                Timestamp = Timestamp << 32
                Timestamp = Timestamp Or timestampLow

                DecodeMidi2Message(dword0, dword1, dword2, dword3, Timestamp)
            Else
                ' not enough data
                WriteMessageLine("Not enough Data")
                Exit Sub
            End If

        End While

    End Sub


    Private Sub DecodeMidi2Message(dw0 As UInteger, dw1 As UInteger, dw2 As UInteger, dw3 As UInteger, timestamp As ULong)

        '--- raw ---

        Dim rawstr As String = "raw: " & dw0.ToString("x") & "  " &
                                dw1.ToString("x") & "  " &
                                dw2.ToString("x") & "  " &
                                dw3.ToString("x") & "  " &
                                timestamp.ToString("x")
        WriteMessageLine(rawstr)

        '---

        Dim msgsize As Byte
        msgsize = GetUmpMessageSize(dw0)
        Dim msgTypeValue As Byte = dw0 >> 28

        WriteMessageLine("Message size = " & msgsize * 32 & " bits")
        WriteMessageLine("Message type = " & "[" & msgTypeValue.ToString("x") & "]  " &
                                                            GetMessageTypeDescription(dw0))
        WriteMessageLine("Timestamp = " & timestamp.ToString("N0"))
        '---
        WriteMessageLine("---")     ' end mark
        TbMessage.ScrollToCaret()   ' make last line visible
    End Sub


    '--- receiveEndpoint Message IN ---
    Private Sub MessageReceivedHandler(sender As IMidiMessageReceivedEventSource, args As MidiMessageReceivedEventArgs)

        If InputQueue.Count < InputQueueCountLimit Then

            Dim dword0 As UInteger
            Dim dword1 As UInteger
            Dim dword2 As UInteger
            Dim dword3 As UInteger
            Dim wret As Byte
            wret = args.FillWords(dword0, dword1, dword2, dword3)

            If wret > 0 Then
                If wret >= 1 Then
                    InputQueue.Enqueue(dword0)
                    If wret >= 2 Then
                        InputQueue.Enqueue(dword1)
                        If wret >= 3 Then
                            InputQueue.Enqueue(dword2)
                            If wret >= 4 Then
                                InputQueue.Enqueue(dword3)
                            End If
                        End If
                    End If
                End If
            End If

            '--- Timestamp 64-bit
            InputQueue.Enqueue(GetHighDWord(args.Timestamp))
            InputQueue.Enqueue(GetLowDWord(args.Timestamp))

        Else
            DiscardedMidiMessages += 1
        End If

    End Sub


    ' MessageType (MT)      
    '                       1x 2x 3x 4x 32-bit  
    '                       MidiMessage32, MidiMessage64, MidiMessage96, MidiMessage128
    '
    'MT     UMP Size       Description
    '---------------------------------------------------------------------------------
    '0x0    32 bits         Utility Messages
    '0x1    32 bits         System Real Time And System Common Messages (except System Exclusive)
    '0x2    32 bits         MIDI 1.0 Channel Voice Messages
    '0x3    64 bits         Data Messages(including System Exclusive)
    '0x4    64 bits         MIDI 2.0 Channel Voice Messages
    '0x5   128 bits         Data Messages
    '0x6    32 bits         Reserved for future definition by MMA/AMEI
    '0x7    32 bits         "
    '0x8    64 bits         "
    '0x9    64 bits         "
    '0xA    64 bits         "
    '0xB    96 bits         "
    '0xC    96 bits         "
    '0xD   128 bits         Flex Data Messages
    '0xE   128 bits         Reserved for future definition by MMA/AMEI
    '0xF   128 bits         UMP Stream Messages

    Private Function GetUmpMessageSize(dword0 As UInteger) As Byte
        Dim ndx As Integer = dword0 >> 28
        Return UmpMessageSizeTable(ndx)
    End Function

    ' 0=1, 1=1, 2=1, 3=2, 4=2, 5=4, 6=1, 7=1, 8=2, 9=2, A=2, B=3, C=3, D=4, E=4, F=4
    Private ReadOnly UmpMessageSizeTable() As Byte = {1, 1, 1, 2, 2, 4, 1, 1, 2, 2, 2, 3, 3, 4, 4, 4}

    Private Function GetMessageTypeDescription(dw0 As UInteger) As String
        Dim mt As Byte      ' message type
        mt = dw0 >> 28

        Select Case mt
            Case 0
                Return ("Utility Message")
            Case 1
                Return ("System Real Time or Common")
            Case 2
                Return ("MIDI 1.0 Channel Voice Message")
            Case 3
                Return ("Data Message (incl. SysEx")
            Case 4
                Return ("MIDI 2.0 Channel Voice Message")
            Case 5
                Return ("Data Message")
            Case 6
                Return ("Reserved")
            Case 7
                Return ("Reserved")
            Case 8
                Return ("Reserved")
            Case 9
                Return ("Reserved")
            Case &HA
                Return ("Reserved")
            Case &HB
                Return ("Reserved")
            Case &HC
                Return ("Reserved")
            Case &HD
                Return ("Flex Data Message")
            Case &HE
                Return ("Reserved")
            Case &HF
                Return ("UMP Stream Message")
        End Select

        Return ""
    End Function

    Private Function GetHighDWord(value As ULong) As UInteger
        Return value >> 32
    End Function

    Private Function GetLowDWord(value As ULong) As UInteger
        Return value And &HFFFFFFFFL
    End Function

    Private Sub WriteMessage(str As String)
        If str Is Nothing Then Exit Sub
        TbMessage.AppendText(str)
    End Sub

    Private Sub WriteMessageLine(str As String)
        If str Is Nothing Then Exit Sub
        TbMessage.AppendText(str)
        TbMessage.AppendText(vbCrLf)
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As EventArgs) Handles BtnClose.Click
        Close()
    End Sub

    Private Sub BtnSendMsg_Click(sender As Object, e As EventArgs) Handles BtnSendMsg.Click
        SendTestMessage()
    End Sub
End Class
