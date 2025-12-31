Imports Microsoft.Windows.Devices.Midi2
Imports Microsoft.Windows.Devices.Midi2.Messages
Imports Midi_IO_2.Midi_IO_2
Partial Public Class MainWindow

    Private SelectedOutput As MidiOutput
    Private WithEvents SelectedInput As MidiInput
    Private WithEvents Receiver As MessageReceiver

    Private Sub Ti_IO_Test_Loaded(sender As Object, e As RoutedEventArgs) Handles Ti_IO_Test.Loaded
        UpdateInputSelector()
        UpdateOutputSelector()
    End Sub

    Private Sub UpdateInputSelector()
        Dim sel = CmbInputSelector.SelectedValue

        CmbInputSelector.ItemsSource = Nothing
        CmbInputSelector.ItemsSource = mio.MidiInputList
        CmbInputSelector.DisplayMemberPath = "Name"
        CmbInputSelector.SelectedValuePath = "Name"

        If sel IsNot Nothing Then
            CmbInputSelector.SelectedValue = sel
        End If

    End Sub

    Private Sub UpdateOutputSelector()
        Dim sel = CmbOutputSelector.SelectedValue

        CmbOutputSelector.ItemsSource = Nothing
        CmbOutputSelector.ItemsSource = mio.MidiOutputList
        CmbOutputSelector.DisplayMemberPath = "Name"
        CmbOutputSelector.SelectedValuePath = "Name"

        If sel IsNot Nothing Then
            CmbOutputSelector.SelectedValue = sel
        End If

    End Sub

    Private Sub SelectedOutputChanged()
        Dim sel As MidiOutput
        sel = TryCast(CmbOutputSelector.SelectedItem, MidiOutput)
        If sel IsNot Nothing Then
            sel.Open()
            SelectedOutput = sel
        Else
            SelectedOutput = Nothing
        End If

        'e.AddedItems
        'e.RemovedItems
    End Sub

    Private Sub SelectedInputChanged()
        If SelectedInput IsNot Nothing Then
            SelectedInput.Close()
            SelectedInput = Nothing
        End If

        Dim sel As MidiInput
        sel = TryCast(CmbInputSelector.SelectedItem, MidiInput)
        If sel IsNot Nothing Then
            sel.Open()
            SelectedInput = sel
            Receiver = SelectedInput.MessageReceiver
        Else
            SelectedInput = Nothing
        End If
    End Sub

    Private Sub BtnSendNote_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles BtnSendNote.PreviewMouseLeftButtonDown
        If SelectedOutput IsNot Nothing Then
            Dim ump32 As MidiMessage32 = MidiMessageBuilder.BuildMidi1ChannelVoiceMessage(
                       MidiClock.Now,                                          ' current time
                       New MidiGroup(SelectedOutput.Group),                                       ' Group 5
                       Midi1ChannelVoiceMessageStatus.NoteOn,                  ' NoteOn (9)
                       New MidiChannel(0),                                     ' channel 3
                       64,                                                    ' note 120  (&h78)
                       100)

            'SelectedOutput.EndpointConnection.SendSingleMessagePacket(ump32)
            'SelectedOutput.OutShortMessage(&H90, 64, 100)

            SelectedOutput.OutShortMessage(Midi1ChannelVoiceMessageStatus.NoteOn, 0, 64, 100)
        End If
    End Sub

    Private Sub BtnSendNote_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles BtnSendNote.PreviewMouseLeftButtonUp
        If SelectedOutput IsNot Nothing Then
            Dim ump32 As MidiMessage32 = MidiMessageBuilder.BuildMidi1ChannelVoiceMessage(
                       MidiClock.Now,                                          ' current time
                       New MidiGroup(SelectedOutput.Group),                                       ' Group 5
                       Midi1ChannelVoiceMessageStatus.NoteOn,                  ' NoteOn (9)
                       New MidiChannel(0),                                     ' channel 3
                       64,                                                    ' note 120  (&h78)
                       0)

            'SelectedOutput.EndpointConnection.SendSingleMessagePacket(ump32)
            'SelectedOutput.OutShortMessage(&H90, 64, 0)
            SelectedOutput.OutShortMessage(Midi1ChannelVoiceMessageStatus.NoteOff, 0, 64, 0)
        End If
    End Sub



    Private icount As Long

    Private Sub MsgInput(timestamp As ULong, dw0 As UInteger, dw1 As UInteger, dw2 As UInteger, dw3 As UInteger) Handles Receiver.MsgReceived

        Dim str As String
        Dim tickdiff As String

        tickdiff = TickDiffToMilliseconds(MidiClock.Now, timestamp)

        Dim nfo As MidiMessageInfo
        nfo = GetMidiMessageInfo(dw0, dw1, dw2, dw3)

        'str = tickdiff & vbTab & "  " & " " & nfo.MessageBitCount & " bits, " & "MT: " &
        '            nfo.MessageTypeValue & "  " & nfo.MessageTypeDescription

        str = icount & "  " & nfo.MessageBitCount & " bits, " & "MT: " &
                    nfo.MessageTypeValue & "  " &
                    timestamp.ToString("x") & "  " &
                    dw0.ToString("x") & "  " &
                    dw1.ToString("x") & "  " &
                    dw2.ToString("x") & "  " &
                    dw3.ToString("x") & "  " &
                    vbCrLf & vbTab & nfo.MessageTypeDescription

        Dispatcher.BeginInvoke(New WriteInputMsg_Delegate(AddressOf WriteInputMsg), str)

        icount += 1
    End Sub



    Private Delegate Sub WriteInputMsg_Delegate(str As String)

    Private Sub WriteInputMsg(str As String)
        If str Is Nothing Then Exit Sub

        If TbInputData.LineCount > 1000 Then
            TbInputData.Clear()
        End If

        TbInputData.AppendText(str)
        TbInputData.AppendText(vbCrLf)
        TbInputData.ScrollToEnd()


    End Sub




End Class
