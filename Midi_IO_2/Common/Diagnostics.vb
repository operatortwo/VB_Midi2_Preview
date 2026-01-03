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


End Class
