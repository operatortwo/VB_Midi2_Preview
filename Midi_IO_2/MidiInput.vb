Imports System.Collections.Concurrent
Imports System.Threading
Imports Microsoft.Windows.Devices.Midi2
Imports Windows.Networking.Proximity

Partial Public Class Midi_IO_2

    Public Class MidiInput
        Public Property Name As String = ""
        Public Session As MidiSession
        Public Endpoint As MidiEndpointDeviceInformation
        Public EndpointConnection As MidiEndpointConnection
        Public MessageReceiver As MessageReceiver
        Public PortDeviceID As String = ""
        Public ID As Integer
        Public Group As Byte

        'Public Event MidiInput()

        Public Function Open() As Boolean
            If Session Is Nothing Then Return False
            If Session.IsOpen = False Then Return False
            If Endpoint Is Nothing Then Return False

            '-- MidiEndpointConnection is tied to the lifetime of the session.
            '-- a session should generally not open more than one connection to a single endpoint

            'If EndpointConnection IsNot Nothing Then
            '    If Session.Connections.Values.Contains(EndpointConnection) Then
            '    End If
            'End If

            ' try connect

            If EndpointConnection Is Nothing Then
                EndpointConnection = Session.CreateEndpointConnection(Endpoint.EndpointDeviceId)
                'If EndpointConnection Is Nothing Then Return False      ' return if CreateConnection failed            
                ' Wire up the message event handler before open
                'Dim rcv As New MessageReceiver
                MessageReceiver.EndpointConnection = EndpointConnection
                'MessageReceiverList.Add(rcv)
                AddHandler EndpointConnection.MessageReceived, AddressOf MessageReceiver.MessageReceivedHandler
                EndpointConnection.Open()
            End If

            MessageReceiver.AddListener(Me)

            Return True
        End Function

        Public Function Close() As Boolean
            MessageReceiver.RemoveListener(Me)
            Return True
        End Function

    End Class

    '---------- 1 Receiver per Endpoint ----------

    Public Class MessageReceiver
        Public Endpoint As MidiEndpointDeviceInformation
        Public EndpointConnection As MidiEndpointConnection
        Public Listener As New List(Of MidiInput)
        Private Listening As Boolean

        Private InputQueue As New ConcurrentQueue(Of UInteger)

        ' at this point, no more messages are added. count can exceed this value up to +5
        Private Const InputQueueCountLimit = 4000
        ' when Limit is reached, this value is increased by 1 at every new message
        Private DiscardedMidiMessages As Integer

        Private ReadInputQueueTask As Task
        Private StopReadInputQueue As Boolean


        Public Event MsgReceived(timestamp As ULong, dword0 As UInteger, dword1 As UInteger, dword2 As UInteger, dword3 As UInteger)

        Public Sub AddListener(Input As MidiInput)
            If Input Is Nothing Then Exit Sub
            Listener.Add(Input)
            If Listening = False Then
                StartListening()
            End If
        End Sub

        Public Sub RemoveListener(Input As MidiInput)
            If Input Is Nothing Then Exit Sub
            Dim ndx As Integer
            ndx = Listener.FindIndex(Function(x) x.ID = Input.ID)
            If ndx <> -1 Then
                Listener.Remove(Input)
                If Listener.Count = 0 Then
                    StopListening()
                End If
            End If
        End Sub

        Private Sub StartListening()
            ReadInputQueueTask = Task.Run(AddressOf ReadInputQueue)
            Listening = True
            Debug.WriteLine("--Listening")
        End Sub

        Private Sub StopListening()
            StopReadInputQueue = True
            Listening = False
            Debug.WriteLine("--Listening stopped")
            Debug.WriteLine("---InputQueue.count = " & InputQueue.Count)
            Debug.WriteLine("--- readcount = " & readcount)
            InputQueue.Clear()
        End Sub


        Public Sub MessageReceivedHandler(sender As IMidiMessageReceivedEventSource, args As MidiMessageReceivedEventArgs)
            If Listening = False Then Exit Sub

            Thread.BeginCriticalRegion()

            If InputQueue.Count < InputQueueCountLimit Then

                '--- Timestamp 64-bit
                InputQueue.Enqueue(GetHighDWord(args.Timestamp))
                InputQueue.Enqueue(GetLowDWord(args.Timestamp))


                '--- up to 4* 32-bit values ---

                Dim dword0 As UInteger
                Dim dword1 As UInteger
                Dim dword2 As UInteger
                Dim dword3 As UInteger
                Dim wret As Byte                                        ' number of valid FillWords
                wret = args.FillWords(dword0, dword1, dword2, dword3)

                InputQueue.Enqueue(dword0)
                InputQueue.Enqueue(dword1)
                InputQueue.Enqueue(dword2)
                InputQueue.Enqueue(dword3)

            Else
                DiscardedMidiMessages += 1
            End If

            Thread.EndCriticalRegion()
        End Sub

        Private readcount As Long

        Private timestampHigh As UInteger
        Private timestampLow As UInteger
        Private timestamp As ULong
        Private dword0 As UInteger
        Private dword1 As UInteger
        Private dword2 As UInteger
        Private dword3 As UInteger

        Private Sub ReadInputQueue()
            StopReadInputQueue = False
            readcount = 0

            Do
                If StopReadInputQueue = True Then
                    InputQueue.Clear()
                    Return
                End If

                If InputQueue.IsEmpty = True Then
                    Thread.Sleep(1)              ' min. 1 
                ElseIf InputQueue.Count >= 6 Then         ' ID, Message, TimestampHigh, TimestampLow

                    InputQueue.TryDequeue(timestampHigh)
                    timestamp = timestampHigh
                    timestamp = timestamp << 32
                    Dim rett As Boolean
                    rett = InputQueue.TryDequeue(timestampLow)
                    timestamp = timestamp Or timestampLow

                    InputQueue.TryDequeue(dword0)
                    InputQueue.TryDequeue(dword1)
                    InputQueue.TryDequeue(dword2)
                    InputQueue.TryDequeue(dword3)

                    RaiseEvent MsgReceived(timestamp, dword0, dword1, dword2, dword3)

                    readcount += 1
                End If
            Loop

        End Sub



        Private Function GetHighDWord(value As ULong) As UInteger
            Return value >> 32
        End Function

        Private Function GetLowDWord(value As ULong) As UInteger
            Return value And &HFFFFFFFFL
        End Function
    End Class


End Class
