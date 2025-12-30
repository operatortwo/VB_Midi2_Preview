Imports Microsoft.Windows.Devices.Midi2
Imports Windows.Networking.Proximity

Partial Public Class Midi_IO_2

    Public Class MidiInput
        Public Property Name As String = ""
        Public Session As MidiSession
        Public Endpoint As MidiEndpointDeviceInformation
        Public EndpointConnection As MidiEndpointConnection
        Public PortDeviceID As String = ""
        Public ID As Integer
        Public Group As Byte

        Public Event MidiInput()

        Public Function Open() As Boolean
            If Session Is Nothing Then Return False
            If Session.IsOpen = False Then Return False
            If Endpoint Is Nothing Then Return False

            '-- MidiEndpointConnection is tied to the lifetime of the session.
            '-- a session should generally not open more than one connection to a single endpoint

            If EndpointConnection IsNot Nothing Then
                ' return if the Endpoint is already connected
                If Session.Connections.Values.Contains(EndpointConnection) Then
                    'xxx add listener
                    Return True
                End If
            End If
            ' try connect
            EndpointConnection = Session.CreateEndpointConnection(Endpoint.EndpointDeviceId)
            If EndpointConnection Is Nothing Then Return False      ' return if CreateConnection failed            
            ' Wire up the message event handler before open
            Dim rcv As New MessageReceiver

            rcv.EndpointConnection = EndpointConnection
            MessageReceiverList.Add(rcv)
            AddHandler EndpointConnection.MessageReceived, AddressOf rcv.MessageReceivedHandler
            EndpointConnection.Open()
            Return True
        End Function

        Public Function Close() As Boolean
            'xxx remove listener
            Beep()



            Return True
        End Function

        Private Sub MessageReceivedHandler(sender As IMidiMessageReceivedEventSource, args As MidiMessageReceivedEventArgs)

            'If InputQueue.Count < InputQueueCountLimit Then

            '    Dim dword0 As UInteger
            '    Dim dword1 As UInteger
            '    Dim dword2 As UInteger
            '    Dim dword3 As UInteger
            '    Dim wret As Byte
            '    wret = args.FillWords(dword0, dword1, dword2, dword3)

            '    If wret > 0 Then
            '        If wret >= 1 Then
            '            InputQueue.Enqueue(dword0)
            '            If wret >= 2 Then
            '                InputQueue.Enqueue(dword1)
            '                If wret >= 3 Then
            '                    InputQueue.Enqueue(dword2)
            '                    If wret >= 4 Then
            '                        InputQueue.Enqueue(dword3)
            '                    End If
            '                End If
            '            End If
            '        End If
            '    End If

            '    '--- Timestamp 64-bit
            '    InputQueue.Enqueue(GetHighDWord(args.Timestamp))
            '    InputQueue.Enqueue(GetLowDWord(args.Timestamp))

            'Else
            '    DiscardedMidiMessages += 1
            'End If

        End Sub



    End Class




End Class
