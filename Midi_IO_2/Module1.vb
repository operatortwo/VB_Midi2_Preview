Imports System.Collections.Concurrent
Imports System.Threading
Imports Microsoft.Windows.Devices.Midi2


Module Module1
    Friend Instance As Midi_IO_2

    Public MessageReceiverList As New List(Of MessageReceiver)


    Public Event ShortMsgReceived()

    Public Class MessageReceiver
        Public Endpoint As MidiEndpointDeviceInformation
        Public EndpointConnection As MidiEndpointConnection
        Public Listener As New List(Of Midi_IO_2.MidiInput)


        Public Sub MessageReceivedHandler(sender As IMidiMessageReceivedEventSource, args As MidiMessageReceivedEventArgs)
            '    If InputQueue.Count < InputQueueCountLimit Then

            '        Dim dword0 As UInteger
            '        Dim dword1 As UInteger
            '        Dim dword2 As UInteger
            '        Dim dword3 As UInteger
            '        Dim wret As Byte
            '        wret = args.FillWords(dword0, dword1, dword2, dword3)

            '        If wret > 0 Then
            '            If wret >= 1 Then
            '                InputQueue.Enqueue(dword0)
            '                If wret >= 2 Then
            '                    InputQueue.Enqueue(dword1)
            '                    If wret >= 3 Then
            '                        InputQueue.Enqueue(dword2)
            '                        If wret >= 4 Then
            '                            InputQueue.Enqueue(dword3)
            '                        End If
            '                    End If
            '                End If
            '            End If
            '        End If

            '        '--- Timestamp 64-bit
            '        InputQueue.Enqueue(GetHighDWord(args.Timestamp))
            '        InputQueue.Enqueue(GetLowDWord(args.Timestamp))

            '    Else
            '        DiscardedMidiMessages += 1
            '    End If
        End Sub


    End Class

    Private InputQueue As New ConcurrentQueue(Of UInteger)
    ' at this point, no more messages are added. count can exceed this value up to +5
    Private Const InputQueueCountLimit = 4000
    ' when Limit is reached, this value is increased by 1 at every new message
    Private DiscardedMidiMessages As Integer


    Public ReadThread As New Thread(AddressOf ReadInput)


    Private readcount As Long

    Private Sub ReadInput()
        Do
            readcount += 1
            If InputQueue.IsEmpty = True Then
                Thread.Sleep(20)
            ElseIf InputQueue.Count > 3 Then         ' ID, Message, TimestampHigh, TimestampLow

                RaiseEvent ShortMsgReceived()

            End If
        Loop
    End Sub


    Private Function GetHighDWord(value As ULong) As UInteger
        Return value >> 32
    End Function

    Private Function GetLowDWord(value As ULong) As UInteger
        Return value And &HFFFFFFFFL
    End Function

End Module
