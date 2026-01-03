Imports Microsoft.Windows.Devices.Midi2
Imports Microsoft.Windows.Devices.Midi2.Messages
Imports Midi_IO_2.Midi_IO_2
Imports Midi_IO_2.MessageDecode
Partial Public Class MainWindow

    Private MidiOut1 As MidiOutput
    Private WithEvents MidiIn1 As MidiInput
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
            MidiOut1 = sel
        Else
            MidiOut1 = Nothing
        End If

        'e.AddedItems
        'e.RemovedItems
    End Sub

    Private Sub SelectedInputChanged()
        If MidiIn1 IsNot Nothing Then
            MidiIn1.Close()
            MidiIn1 = Nothing
        End If

        Dim sel As MidiInput
        sel = TryCast(CmbInputSelector.SelectedItem, MidiInput)
        If sel IsNot Nothing Then
            sel.SetFilterTimingClock(CbFilterTimingClock.IsChecked)
            sel.SetFilterActiveSensing(CbFilterActiveSensing.IsChecked)
            sel.Open()
            MidiIn1 = sel
            Receiver = MidiIn1.MessageReceiver
        Else
            MidiIn1 = Nothing
        End If
    End Sub

    Private Sub CbFilterTimingClock_Click(sender As Object, e As RoutedEventArgs) Handles CbFilterTimingClock.Click
        If MidiIn1 IsNot Nothing Then
            MidiIn1.SetFilterTimingClock(CbFilterTimingClock.IsChecked)
        End If
    End Sub

    Private Sub CbFilterActiveSensing_Click(sender As Object, e As RoutedEventArgs) Handles CbFilterActiveSensing.Click
        If MidiIn1 IsNot Nothing Then
            MidiIn1.SetFilterActiveSensing(CbFilterActiveSensing.IsChecked)
        End If
    End Sub

#Region "Send Messages"

    Private Sub BtnSendNote_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles BtnSendNote.PreviewMouseLeftButtonDown
        If MidiOut1 IsNot Nothing Then
            Dim ump32 As MidiMessage32 = MidiMessageBuilder.BuildMidi1ChannelVoiceMessage(
                       MidiClock.Now,                                          ' current time
                       New MidiGroup(MidiOut1.Group),                                       ' Group 5
                       Midi1ChannelVoiceMessageStatus.NoteOn,                  ' NoteOn (9)
                       New MidiChannel(0),                                     ' channel 3
                       64,                                                    ' note 120  (&h78)
                       100)

            'MidiOut1.EndpointConnection.SendSingleMessagePacket(ump32)
            'MidiOut1.OutShortMessage(&H90, 64, 100)

            MidiOut1.OutShortMessage(Midi1ChannelVoiceMessageStatus.NoteOn, 0, 64, 100)
        Else
            WriteOutputMsg("Select an Output to send messages")
        End If
    End Sub

    Private Sub BtnSendNote_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles BtnSendNote.PreviewMouseLeftButtonUp
        If MidiOut1 IsNot Nothing Then
            Dim ump32 As MidiMessage32 = MidiMessageBuilder.BuildMidi1ChannelVoiceMessage(
                       MidiClock.Now,                                          ' current time
                       New MidiGroup(MidiOut1.Group),                                       ' Group 5
                       Midi1ChannelVoiceMessageStatus.NoteOn,                  ' NoteOn (9)
                       New MidiChannel(0),                                     ' channel 3
                       64,                                                    ' note 120  (&h78)
                       0)

            'MidiOut1.EndpointConnection.SendSingleMessagePacket(ump32)
            'MidiOut1.OutShortMessage(&H90, 64, 0)
            MidiOut1.OutShortMessage(Midi1ChannelVoiceMessageStatus.NoteOff, 0, 64, 0)
        End If
    End Sub

    Private Sub Btn_ID_Request_Click(sender As Object, e As RoutedEventArgs) Handles Btn_ID_Request.Click
        If MidiOut1 IsNot Nothing Then
            Dim identity_request() As Byte = {&HF0, &H7E, 0, 6, 1, &HF7}
            WriteOutputHex(identity_request)
            Dim ret = MidiOut1.OutLongMessage(identity_request)
        Else
            WriteOutputMsg("Select an Output to send messages")
        End If
    End Sub

    Private Sub Btn_GM_On_Click(sender As Object, e As RoutedEventArgs) Handles Btn_GM_On.Click
        If MidiOut1 IsNot Nothing Then
            Dim gm_on() As Byte = {&HF0, &H7E, &H7F, 9, 1, &HF7}
            WriteOutputHex(gm_on)
            Dim ret = MidiOut1.OutLongMessage(gm_on)
        Else
            WriteOutputMsg("Select an Output to send messages")
        End If
    End Sub

    Private Sub SendSysExTestData()
        If MidiOut1 IsNot Nothing Then
            Dim buffer() As Byte
            Dim cnt As Integer = SldSysExNumValues.Value
            buffer = GenerateSysExTestData(cnt)
            WriteOutputHex(buffer)
            Dim ret = MidiOut1.OutLongMessage(buffer)
        Else
            WriteOutputMsg("Select an Output to send messages")
        End If

    End Sub

#Region "Generate SysEx test data"

    Private Function GenerateSysExTestData(bytecount As Integer) As Byte()
        Dim bcount As Integer
        bcount = bytecount + 2                  ' F0 ... F7
        Dim buffer(bcount - 1) As Byte

        Dim ndx As Integer

        buffer(ndx) = &HF0
        buffer(bcount - 1) = &HF7

        Dim val As Byte = 1

        For i = 1 To bcount - 2

            buffer(i) = val
            val += 1
            If val >= 128 Then
                val = 1
            End If

        Next

        Return buffer
    End Function

#End Region

#End Region

#Region "Input"

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
                    vbCrLf & vbTab & nfo.MessageTypeDescription & "  " &
                    "Group: " & nfo.Group

        Dispatcher.BeginInvoke(New WriteInputMsg_Delegate(AddressOf WriteInputMsg), str)

        icount += 1
    End Sub



    Private Delegate Sub WriteInputMsg_Delegate(str As String)

    Private Sub WriteInputMsg(str As String)
        If str Is Nothing Then Exit Sub

        If TbInputData.LineCount >= 1500 Then
            'TbInputData.Clear()
            'Debug.WriteLine("Lcount = " & TbInputData.LineCount)
            Dim ndx As Integer
            Dim lcount As Integer
            Dim fchar As Char = vbLf
            For i = 0 To TbInputData.Text.Length - 1
                ndx = TbInputData.Text.IndexOf(fchar, ndx)
                lcount += 1
                ndx += 1
                If lcount >= 500 Then Exit For
            Next
            TbInputData.Text = TbInputData.Text.Remove(0, ndx)
            'Debug.WriteLine("new Lcount = " & TbInputData.LineCount)
        End If

        TbInputData.AppendText(str)
        TbInputData.AppendText(vbCrLf)
        TbInputData.ScrollToEnd()
    End Sub

#End Region

    Private Sub WriteOutputHex(buffer() As Byte)
        Dim str As String
        str = BitConverter.ToString(buffer)
        str = str.Replace("-", " ")
        WriteOutputMsg(str)
    End Sub

    Private Sub WriteOutputMsg(str As String)
        If str Is Nothing Then Exit Sub
        TbOutputData.AppendText(str)
        TbOutputData.AppendText(vbCrLf)
        TbOutputData.ScrollToEnd()
    End Sub




End Class
