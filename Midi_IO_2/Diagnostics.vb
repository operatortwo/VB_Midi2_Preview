Imports Microsoft.Windows.Devices.Midi2
Partial Public Class Midi_IO_2

    Friend Shared TimestampFrequency As ULong = MidiClock.TimestampFrequency       '  ticks per second

    Public Sub DiagTest()
        Dim tmset As MidiSystemTimerSettings = MidiClock.GetCurrentSystemTimerInfo()

        Debug.WriteLine("CurrentIntervalTicks = " & tmset.CurrentIntervalTicks)
        Debug.WriteLine("MaximumIntervalTicks = " & tmset.MaximumIntervalTicks)
        Debug.WriteLine("MinimumIntervalTicks = " & tmset.MinimumIntervalTicks)

        Debug.WriteLine("TimestampFrequency = " & MidiClock.TimestampFrequency)


        Dim m As Midi_IO_2 = Instance

    End Sub


    ''' <summary>
    ''' Converts the specified timestamp ticks to milliseconds
    ''' </summary>
    ''' <param name="ticks">Midi Timestamp Ticks</param>
    ''' <returns>Number of Millisecons rounded to 1 decimal place</returns>
    Public Shared Function TicksToMilliseconds(ticks As ULong) As Double
        Dim result As Double
        result = ticks / (TimestampFrequency / 1000)
        Return Math.Round(result, 1)
    End Function

    ''' <summary>
    ''' Calculates the difference between the two timestamp ticks 
    ''' and returns the result in milliseconds.
    ''' </summary>
    ''' <param name="ticks1">Midi Timestamp Ticks</param>
    ''' <param name="ticks2">Midi Timestamp Ticks</param>
    ''' <returns>Number of Millisecons rounded to 1 decimal place</returns>
    Public Shared Function TickDiffToMilliseconds(ticks1 As ULong, ticks2 As ULong) As Double
        ' allows negative results
        Dim t1 As Double = ticks1
        Dim t2 As Double = ticks2
        Dim diff As Double = t1 - t2
        Return Math.Round(diff / (TimestampFrequency / 1000), 1)
    End Function


    Public Class MidiMessageInfo
        Public MessageByteCount As Byte
        Public MessageBitCount As Byte
        Public MessageTypeValue As Byte
        Public MessageTypeDescription As String = ""
    End Class


    Public Shared Function GetMidiMessageInfo(dw0 As UInteger, dw1 As UInteger, dw2 As UInteger, dw3 As UInteger) As MidiMessageInfo
        Dim nfo As New MidiMessageInfo

        nfo.MessageByteCount = GetUmpMessageSize(dw0)
        nfo.MessageBitCount = nfo.MessageByteCount * 32
        nfo.MessageTypeValue = dw0 >> 28
        nfo.MessageTypeDescription = GetMessageTypeDescription(dw0)

        Return nfo
    End Function



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

    Private Shared Function GetUmpMessageSize(dword0 As UInteger) As Byte
        Dim ndx As Integer = dword0 >> 28
        Return UmpMessageSizeTable(ndx)
    End Function

    ' 0=1, 1=1, 2=1, 3=2, 4=2, 5=4, 6=1, 7=1, 8=2, 9=2, A=2, B=3, C=3, D=4, E=4, F=4
    Private Shared ReadOnly UmpMessageSizeTable() As Byte = {1, 1, 1, 2, 2, 4, 1, 1, 2, 2, 2, 3, 3, 4, 4, 4}

    Private Shared Function GetMessageTypeDescription(dw0 As UInteger) As String
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

End Class
