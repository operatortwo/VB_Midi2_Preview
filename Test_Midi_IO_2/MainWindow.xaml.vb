Imports Microsoft.Windows.Devices.Midi2

Class MainWindow

    Public WithEvents mio As New Midi_IO_2.Midi_IO_2

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        If mio.StartMidiSession() = True Then
            WriteMessageLine(mio.BuildVersion)
            WriteMessageLine(mio.ReturnMessage)
        Else
            WriteMessageLine(mio.ErrorMessage)
        End If

        TabControl1.SelectedItem = Ti_IO_Test
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

        UpdateInputSelector()                   ' update list and try to keep selected
        UpdateOutputSelector()                  ' update list and try to keep selected

    End Sub

    Private Sub CmbOutputSelector_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CmbOutputSelector.SelectionChanged
        SelectedOutputChanged()
    End Sub

    Private Sub CmbInputSelector_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CmbInputSelector.SelectionChanged
        SelectedInputChanged()
    End Sub

    Private Sub BtnCloseSelectedOutput_Click(sender As Object, e As RoutedEventArgs) Handles BtnCloseSelectedOutput.Click
        CmbOutputSelector.SelectedItem = Nothing
    End Sub

    Private Sub BtnCloseSelectedInput_Click(sender As Object, e As RoutedEventArgs) Handles BtnCloseSelectedInput.Click
        CmbInputSelector.SelectedItem = Nothing
    End Sub

    Private Sub BtnTest_Click(sender As Object, e As RoutedEventArgs) Handles BtnTest.Click
        mio.DiagTest()
    End Sub

    Private Sub BtnClearTbInputData_Click(sender As Object, e As RoutedEventArgs) Handles BtnClearTbInputData.Click
        TbInputData.Clear()
    End Sub

    Private Sub BtnClearTbOutputData_Click(sender As Object, e As RoutedEventArgs) Handles BtnClearTbOutputData.Click
        TbOutputData.Clear()
    End Sub

    Private Sub BtnSysExTest_Click(sender As Object, e As RoutedEventArgs) Handles BtnSysExTest.Click
        SendSysExTestData()
    End Sub

    Private Sub CbShowSysEx7_Click(sender As Object, e As RoutedEventArgs) Handles CbFilterSysEx7raw.Click
        FilterSysEx7raw = CbFilterSysEx7raw.IsChecked
    End Sub
End Class
