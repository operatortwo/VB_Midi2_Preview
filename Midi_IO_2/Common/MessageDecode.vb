Imports Windows.ApplicationModel

Public Module MessageDecode

    Public Class MidiMessageInfo
        '--- common ---
        Public MessageByteCount As Byte
        Public MessageBitCount As Byte
        Public MessageTypeValue As Byte
        Public MessageTypeDescription As String = ""
        '--- depending on MessageType ---
        Public Group As Byte
        Public Status As Byte
        Public Channel As Byte
        Public MessageDescription = ""
    End Class



    Function GetMidiMessageInfo(dw0 As UInteger, dw1 As UInteger, dw2 As UInteger, dw3 As UInteger) As MidiMessageInfo
        Dim nfo As New MidiMessageInfo

        nfo.MessageByteCount = GetUmpMessageSize(dw0)
        nfo.MessageBitCount = nfo.MessageByteCount * 32
        nfo.MessageTypeValue = dw0 >> 28
        nfo.MessageTypeDescription = GetMessageTypeDescription(dw0)

        Select Case nfo.MessageTypeValue
            Case MessageType.UtilityMessage
                GetMessageInfo_Type_0(nfo, dw0)
            Case MessageType.SystemRealTimeAndCommon
                GetMessageInfo_Type_1(nfo, dw0)
            Case MessageType.Midi_1_ChannelVoiceMessage
                GetMessageInfo_Type_2(nfo, dw0)
            Case MessageType.DataMessage64
                GetMessageInfo_Type_3(nfo, dw0, dw1)
            Case MessageType.Midi_2_ChannelVoiceMessage
                GetMessageInfo_Type_4(nfo, dw0, dw1)
            Case MessageType.DataMessage128
                GetMessageInfo_Type_5(nfo, dw0, dw1, dw2, dw3)
            Case MessageType.FlexData
                GetMessageInfo_Type_D(nfo, dw0, dw1, dw2, dw3)
            Case MessageType.UmpStream
                GetMessageInfo_Type_F(nfo, dw0, dw1, dw2, dw3)
            Case Else
                ' reserved MessageType
        End Select


        Return nfo
    End Function

    Private Sub GetMessageInfo_Type_0(ByRef nfo As MidiMessageInfo, dw0 As UInteger)
        'MessageType.UtilityMessage
        ' Group field is reserved
        Dim stat As UInteger
        stat = (dw0 >> 20) And &HF
        nfo.Status = stat
        If stat = 0 Then
            ' NOOP
        End If
    End Sub

    Private Sub GetMessageInfo_Type_1(ByRef nfo As MidiMessageInfo, dw0 As UInteger)
        'MessageType.SystemRealTimeAndCommon
        nfo.Group = (dw0 >> 24) And &HF
        Dim stat As UInteger
        stat = (dw0 >> 16) And &HFF
        nfo.Status = stat

        Select Case stat
            Case &HF1           ' MIDI Time Code  0nnndddd 
                nfo.MessageDescription = "Midi Time Code"
            Case &HF2           ' Song Position Pointer  0lllllll* 0mmmmmmm*
                nfo.MessageDescription = "Song Position Pointer"
            Case &HF3           ' Song Select  0sssssss 
                nfo.MessageDescription = "Song Select"
            Case &HF6           ' Tune Request  
                nfo.MessageDescription = "Tune Request"
            Case &HF8           ' Timing Clock  
                nfo.MessageDescription = "Timing Clock"
            Case &HFA           ' Start  
                nfo.MessageDescription = "Start"
            Case &HFB           ' Continue 
                nfo.MessageDescription = "Continue"
            Case &HFC           ' Stop  
                nfo.MessageDescription = "Stop"
            Case &HFE           ' Active Sensing  
                nfo.MessageDescription = "ActiveSensing"
            Case &HFF           ' Reset  
                nfo.MessageDescription = "Reset"
        End Select


        '0xF0       Reserved 
        '0xF1   MIDI Time Code  0nnndddd 
        '0xF2   Song Position Pointer  0lllllll* 0mmmmmmm*
        '0xF3   Song Select  0sssssss 
        '0xF4       Reserved  
        '0xF5       Reserved  
        '0xF6   Tune Request  
        '0xF7       Reserved  
        '0xF8   Timing Clock  
        '0xF9       Reserved  
        '0xFA   Start  
        '0xFB   Continue 
        '0xFC   Stop  
        '0xFD       Reserved  
        '0xFE   Active Sensing  
        '0xFF   Reset  
    End Sub

    Private Sub GetMessageInfo_Type_2(ByRef nfo As MidiMessageInfo, dw0 As UInteger)
        'MessageType.Midi_1_ChannelVoiceMessage
        nfo.Group = (dw0 >> 24) And &HF
    End Sub

    Private Sub GetMessageInfo_Type_3(ByRef nfo As MidiMessageInfo, dw0 As UInteger, dw1 As UInteger)
        'MessageType.DataMessage64
        nfo.Group = (dw0 >> 24) And &HF
    End Sub

    Private Sub GetMessageInfo_Type_4(ByRef nfo As MidiMessageInfo, dw0 As UInteger, dw1 As UInteger)
        'MessageType.Midi_2_ChannelVoiceMessage
        nfo.Group = (dw0 >> 24) And &HF
    End Sub

    Private Sub GetMessageInfo_Type_5(ByRef nfo As MidiMessageInfo, dw0 As UInteger, dw1 As UInteger, dw2 As UInteger, dw3 As UInteger)
        'MessageType.DataMessage128
        nfo.Group = (dw0 >> 24) And &HF
    End Sub

    Private Sub GetMessageInfo_Type_D(ByRef nfo As MidiMessageInfo, dw0 As UInteger, dw1 As UInteger, dw2 As UInteger, dw3 As UInteger)
        'MessageType.FlexData

    End Sub

    Private Sub GetMessageInfo_Type_F(ByRef nfo As MidiMessageInfo, dw0 As UInteger, dw1 As UInteger, dw2 As UInteger, dw3 As UInteger)
        'MessageType.UmpStream

    End Sub


    Function GetUmpMessageSize(dword0 As UInteger) As Byte
        Dim ndx As Integer = dword0 >> 28
        Return UmpMessageSizeTable(ndx)
    End Function

    ' 0=1, 1=1, 2=1, 3=2, 4=2, 5=4, 6=1, 7=1, 8=2, 9=2, A=2, B=3, C=3, D=4, E=4, F=4
    Private ReadOnly UmpMessageSizeTable() As Byte = {1, 1, 1, 2, 2, 4, 1, 1, 2, 2, 2, 3, 3, 4, 4, 4}

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

    Public Enum MessageType
        UtilityMessage = 0
        SystemRealTimeAndCommon = 1
        Midi_1_ChannelVoiceMessage = 2
        DataMessage64 = 3
        Midi_2_ChannelVoiceMessage = 4
        DataMessage128 = 5
        Reserved_6 = 6
        Reserved_7 = 7
        Reserved_8 = 8
        Reserved_9 = 9
        Reserved_A = &HA
        Reserved_B = &HB
        Reserved_C = &HC
        FlexData = &HD
        Reserved_E = &HE
        UmpStream = &HF
    End Enum

    Friend Const MTstr_UtilityMessage = "Utility Message"
    Friend Const MTstr_SystemRealTimeOrCommon = "System Real Time or Common"

    Friend Const MTstr_Midi_1_ChannelVoiceMessage = "MIDI 1.0 Channel Voice Message"
    Friend Const MTstr_DataMessage64 = "Data Message (incl. SysEx)"
    Friend Const MTstr_Midi_2_ChannelVoiceMessage = "MIDI 2.0 Channel Voice Message"
    Friend Const MTstr_DataMessage128 = "Data Message"
    Friend Const MTstr_FlexDataMessage = "Flex Data Message"
    Friend Const MTstr_UmpStreamMessage = "UMP Stream Message"


    Friend Const MTstr_Reserved = "Reserved"

    Function GetMessageTypeDescription(dw0 As UInteger) As String
        Dim mt As Byte      ' message type
        mt = dw0 >> 28

        Select Case mt
            Case MessageType.UtilityMessage
                Return MTstr_UtilityMessage
            Case MessageType.SystemRealTimeAndCommon
                Return MTstr_SystemRealTimeOrCommon
            Case MessageType.Midi_1_ChannelVoiceMessage
                Return MTstr_Midi_1_ChannelVoiceMessage
            Case MessageType.DataMessage64
                Return MTstr_DataMessage64
            Case MessageType.Midi_2_ChannelVoiceMessage
                Return MTstr_Midi_2_ChannelVoiceMessage
            Case MessageType.DataMessage128
                Return MTstr_DataMessage128
            Case MessageType.Reserved_6
                Return MTstr_Reserved
            Case MessageType.Reserved_7
                Return MTstr_Reserved
            Case MessageType.Reserved_8
                Return MTstr_Reserved
            Case MessageType.Reserved_9
                Return MTstr_Reserved
            Case MessageType.Reserved_A
                Return MTstr_Reserved
            Case MessageType.Reserved_B
                Return MTstr_Reserved
            Case MessageType.Reserved_C
                Return MTstr_Reserved
            Case MessageType.FlexData
                Return MTstr_FlexDataMessage
            Case MessageType.Reserved_E
                Return MTstr_Reserved
            Case MessageType.UmpStream
                Return MTstr_UmpStreamMessage
        End Select

        Return ""
    End Function


End Module
