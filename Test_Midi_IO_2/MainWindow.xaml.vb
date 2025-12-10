Class MainWindow

    Public WithEvents mio As New Midi_IO_2.Midi_IO_2

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        If mio.StartMidiSession() = True Then
            WriteMessageLine(mio.BuildVersion)
            WriteMessageLine(mio.ReturnMessage)
        Else
            WriteMessageLine(mio.ErrorMessage)
        End If
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        mio.StopMidiSession()
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs) Handles BtnClose.Click
        Close()
    End Sub

    Private Sub Midi_IO_List_changed() Handles mio.MidiInOutListChanged
        Dispatcher.BeginInvoke(New MidiInOutListChanged_Delegate(AddressOf MidiInOutListChanged))
    End Sub

    Public Delegate Sub MidiInOutListChanged_Delegate()
    Private Sub MidiInOutListChanged()

        LbMidiOutput.Items.Clear()
        LbMidiInput.Items.Clear()

        For Each port In mio.MidiOutputList
            LbMidiOutput.Items.Add(port.Name)
        Next

        For Each port In mio.MidiInputList
            LbMidiInput.Items.Add(port.Name)
        Next

    End Sub

End Class
