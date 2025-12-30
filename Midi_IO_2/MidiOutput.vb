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


    End Class
End Class
