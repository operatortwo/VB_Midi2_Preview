Imports System.Runtime.InteropServices.JavaScript
Imports Microsoft.Windows.Devices.Midi2

Partial Public Class Midi_IO_2

    Public Class MidiOutput
        Public Property Name As String = ""
        Public Session As MidiSession
        Public Endpoint As MidiEndpointDeviceInformation
        Public EndpointConnection As MidiEndpointConnection
        Public PortDeviceID As String = ""
        Public ID As Integer
        Public Group As Byte

        Public Function Open() As Boolean
            If Session Is Nothing Then Return False
            If Session.IsOpen = False Then Return False
            If Endpoint Is Nothing Then Return False
            If EndpointConnection IsNot Nothing Then
                ' return if the Endpoint is already connected
                If Session.Connections.Values.Contains(EndpointConnection) Then Return True
            End If
            ' try connect
            EndpointConnection = Session.CreateEndpointConnection(Endpoint.EndpointDeviceId)
            If EndpointConnection Is Nothing Then Return False      ' return if CreateConnection failed
            'Wire up the message event handler before open
            EndpointConnection.Open()
            Return True
        End Function
        ''' <summary>
        ''' Send Midi 1 ChannelVoiceMessage (&h80,90,A0,B0,C0,D0,E0) 
        ''' (NoteOn, NoteOff, PolyPressure, ControlChange, ProgramChange, ChannelPressure, PitchBend)
        ''' </summary>
        ''' <param name="StatusChannel">Status in higher 4 bits, Channel in lower 4 bits</param>
        ''' <param name="data1">first Midi data byte</param>
        ''' <param name="data2">second Midi data byte or 0</param>
        Public Sub OutShortMessage(StatusChannel As Byte, data1 As Byte, data2 As Byte)
            Midi_1_ChannelVoiceMessageOut(StatusChannel, data1, data2)
        End Sub

        ''' <summary>
        ''' Send Midi 1 ChannelVoiceMessage (&h80,90,A0,B0,C0,D0,E0) 
        ''' (NoteOn, NoteOff, PolyPressure, ControlChange, ProgramChange, ChannelPressure, PitchBend)
        ''' </summary>
        ''' <param name="status">Status in higher 4 bits or one item of the 
        ''' Midi1ChannelVoiceMessageStatus enumeration</param>
        ''' <param name="channel">4 bit channel number, 0 based</param>
        ''' <param name="data1">first Midi data byte</param>
        ''' <param name="data2">second Midi data byte or 0</param>
        Public Sub OutShortMessage(status As Byte, channel As Byte, data1 As Byte, data2 As Byte)
            Dim statch As Byte
            Dim stat As Byte
            If status < &H80 Then
                stat = status << 4
            Else
                stat = status And &HF0
            End If
            Dim cha As Byte = channel And &HF
            statch = stat Or cha
            Midi_1_ChannelVoiceMessageOut(statch, data1, data2)
        End Sub

        Private Sub Midi_1_ChannelVoiceMessageOut(StatusChannel As Byte, data1 As Byte, data2 As Byte)
            Dim mdword As UInteger
            Dim grp As Byte = Group And &HF
            mdword = 2                          ' MessageType 2     4 bits
            mdword = mdword << 4
            mdword = mdword Or Group            ' group             4 bits
            mdword = mdword << 8
            mdword = mdword Or StatusChannel    ' statusChannel     8 bits
            mdword = mdword << 8
            mdword = mdword Or data1            ' data1             8 bits
            mdword = mdword << 8
            mdword = mdword Or data2            ' data2             8 bits
            EndpointConnection.SendSingleMessageWords(MidiClock.Now, mdword)


        End Sub

        Private WordList As New List(Of UInteger)

        Public Function OutLongMessage(buffer() As Byte) As Boolean
            ' max. Words is unknown, cannot find
            ' (EndpointConnection) GetSupportedMaxMidiWordsPerTransmission

            If buffer Is Nothing Then Return False
            If buffer.Length < 2 Then Return False                      ' at least 2 data bytes
            If buffer(0) <> &HF0 Then Return False
            If buffer(buffer.Length - 1) <> &HF7 Then Return False

            If WordList.Count > 0 Then WordList.Clear()

            Dim msgcnt As Integer = Math.Ceiling(buffer.Length - 2 / 6)
            If msgcnt < 1 Then
                msgcnt = 1       ' at least 1 msg, allows empty and len=1 SysEx, even it makes no sense
            End If

            Dim bytecount As Integer = buffer.Length - 2

            Dim srcndx As Integer = 1

            Dim ret As Byte
            Do
                ret = WriteWordlist(buffer, srcndx)
                If ret = 0 Then Exit Do
                If ret = 3 Then Exit Do
            Loop


            Dim res As MidiSendMessageResults
            res = EndpointConnection.SendMultipleMessagesWordList(MidiClock.Now, WordList)

            Return True
        End Function


        Private Function WriteWordlist(buffer() As Byte, ByRef ndx As Integer) As Byte
            Dim maxindex As Integer = buffer.Length - 2     ' len-1 -F7
            Dim status As Byte

            If maxindex <= 6 Then
                status = 0                          ' Single UMP Message
            ElseIf ndx = 1 Then
                status = 1                          ' SysEx Start UMP Message
            ElseIf (ndx + 6) < maxindex Then
                status = 2                          ' SysEx Continue UMP Message
            Else
                status = 3                          ' SysEx End Ump Message
            End If

            Dim numbytes As Integer = maxindex - ndx + 1
            If numbytes > 6 Then
                numbytes = 6
            End If

            Dim dw0 As UInteger
            dw0 = 3                     ' MessageType 3
            dw0 = dw0 << 4
            dw0 = dw0 Or Group
            dw0 = dw0 << 4
            dw0 = dw0 Or status
            dw0 = dw0 << 4
            dw0 = dw0 Or numbytes

            Dim val As Byte

            For i = 1 To 2
                dw0 = dw0 << 8
                val = 0
                If ndx <= maxindex Then
                    val = buffer(ndx)
                    ndx += 1
                End If
                dw0 = dw0 Or val
            Next

            Dim dw1 As UInteger
            For i = 1 To 4
                dw1 = dw1 << 8
                val = 0
                If ndx <= maxindex Then
                    val = buffer(ndx)
                    ndx += 1
                End If
                dw1 = dw1 Or val
            Next

            WordList.Add(dw0)
            WordList.Add(dw1)

            Return status
        End Function



    End Class
End Class
