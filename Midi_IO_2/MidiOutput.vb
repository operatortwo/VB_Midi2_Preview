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

        Public Function OutLongMessage(buffer() As Byte) As Boolean
            If buffer.Length < 2 Then Return False                      ' at least 2 data bytes
            If buffer(0) <> &HF0 Then Return False
            If buffer(buffer.Length - 1) <> &HF7 Then Return False
            Dim newlen As Integer = buffer.Length - 2
            Dim buffer2 = New Byte(newlen - 1) {}

            For i = 1 To buffer.Length - 2
                buffer2(i - 1) = buffer(i)
            Next

            Dim barr(3) As Byte                 ' dword convert helper
            Dim dwcnt As Integer = 2 * Math.Ceiling(buffer2.Length / 6)
            If dwcnt < 2 Then
                dwcnt = 2       ' at least 2 dwords, allows empty and len=1 SysEx, even it makes no sense
            End If
            Dim dwordArray(dwcnt - 1) As UInteger

            If buffer2.Length <= 6 Then
                ' single UMP Mesage
                Dim msgType As Byte = 3
                ' Group
                Dim status = 0      ' Complete System Exclusive Message in one UMP
                Dim bytecount As Byte = buffer2.Length      ' number of valid bytes

                dwordArray(0) = SysEx7_FormatFirstDword(Group, status, bytecount, buffer2)
                dwordArray(1) = SysEx7_FormatSecondDword(buffer2)

                Dim ret As MidiSendMessageResults
                ret = EndpointConnection.SendMultipleMessagesWordList(MidiClock.Now, dwordArray)

            Else
                ' does not fit in 1 UMP message:
                ' SysEx start UMP message (status 1)        need 1
                ' SysEx continue UMP message (status 2)         count 0 to x
                ' SysEx end UMP message (status 3)          need 1

                Dim continueMsgCount As Integer
                If buffer2.Length > 12 Then
                    continueMsgCount = (Math.Ceiling(buffer2.Length / 6) - 2)
                End If

                ' start
                ' coninue
                ' end
            End If

            Return True
        End Function


        Private Function SysEx7_FormatFirstDword(group As Byte, status As Byte, bytecount As Byte, buffer() As Byte) As UInteger
            Dim val As UInteger

            val = 3                     ' MessageType 3
            val = val << 4
            val = val Or group
            val = val << 4
            val = val Or status
            val = val << 4
            val = val Or bytecount
            'val = val << 8

            Dim buflen As Integer = buffer.Length

            For i = 0 To 1
                val = val << 8
                If i < buflen Then
                    val = val Or buffer(i)
                End If
            Next

            Return val
        End Function


        Private Function SysEx7_FormatSecondDword(buffer() As Byte) As UInteger
            Dim val As UInteger

            Dim readlen As Integer = buffer.Length - 2

            For i = 0 To 3
                val = val << 8
                If i < readlen Then
                    val = val Or buffer(i + 2)
                End If
            Next

            Return val
        End Function


    End Class
End Class
