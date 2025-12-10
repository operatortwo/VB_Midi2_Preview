Imports Microsoft.Windows.Devices.Midi2
Imports Microsoft.Windows.Devices.Midi2.Initialization
Imports Microsoft.Windows.Devices.Midi2.Messages
Class MainWindow

    Private Initializer As MidiDesktopAppSdkInitializer
    Private Session As MidiSession

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        StartSession()
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        If Initializer IsNot Nothing Then Initializer.Dispose()
        If Session IsNot Nothing Then Session.Dispose()
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs) Handles BtnClose.Click
        Close()
    End Sub

    Private Function StartSession() As Boolean
        Dim BuildVersion As String
        BuildVersion = Microsoft.Windows.Devices.Midi2.Common.MidiNuGetBuildInformation.BuildFullVersion
        WriteMessageLine("Microsoft.Windows.Devices.Midi2  " & BuildVersion)

        Initializer = MidiDesktopAppSdkInitializer.Create()

        '--- check for initializer, gets Nothing if Midi2 is not installed
        If Initializer Is Nothing Then
            WriteMessageLine("Failed to create MidiDesktopAppSdkInitializer")
            Return False
        End If

        '--- initialize SDK runtime
        If Initializer.InitializeSdkRuntime() = False Then

            WriteMessageLine("Failed to initialize SDK Runtime")
            Return False
        End If

        '--- check if 'Windows MIDI Service' is running, try to start if not
        If Initializer.EnsureServiceAvailable() = False Then
            WriteMessageLine("Failed to get 'Windows MIDI Service' running")
            Return False
        Else
            WriteMessageLine("'Windows MIDI Service' is running")
        End If


        Session = MidiSession.Create("API Sample Session")

        Return True
    End Function

    Private Sub WriteMessage(str As String)
        If str Is Nothing Then Exit Sub
        TbMessage.AppendText(str)
    End Sub

    Private Sub WriteMessageLine(str As String)
        If str Is Nothing Then Exit Sub
        TbMessage.AppendText(str)
        TbMessage.AppendText(vbCrLf)
    End Sub

    Private Sub BtnEnumerate_Click(sender As Object, e As RoutedEventArgs) Handles BtnEnumerate.Click

        Dim eplist As IReadOnlyList(Of MidiEndpointDeviceInformation)
        'eplist = MidiEndpointDeviceInformation.FindAll()


        '--- include Loopback
        'eplist = MidiEndpointDeviceInformation.FindAll(
        '    MidiEndpointDeviceInformationSortOrder.Name,
        '    MidiEndpointDeviceInformationFilters.StandardNativeMidi1ByteFormat Or
        '    MidiEndpointDeviceInformationFilters.StandardNativeUniversalMidiPacketFormat Or
        '    MidiEndpointDeviceInformationFilters.DiagnosticLoopback Or
        '    MidiEndpointDeviceInformationFilters.VirtualDeviceResponder)

        '--- True Midi-Devices
        'eplist = MidiEndpointDeviceInformation.FindAll(
        '    MidiEndpointDeviceInformationSortOrder.Name,
        'MidiEndpointDeviceInformationFilters.AllStandardEndpoints)

        eplist = MidiEndpointDeviceInformation.FindAll(
            MidiEndpointDeviceInformationSortOrder.Name,
            MidiEndpointDeviceInformationFilters.AllStandardEndpoints)



        WriteMessageLine("--> enumerate endpoints - " & eplist.Count & " Endpoints returned")

        If eplist IsNot Nothing Then
            For Each endpoint In eplist
                WriteMessageLine("")
                WriteMessageLine("Endpoint Name: " & endpoint.Name)
                WriteMessageLine("Endpoint ID: " & endpoint.EndpointDeviceId)

                Dim info1 = endpoint.GetDeclaredEndpointInfo()              ' protocol 1/2, jitter y/n
                Dim info2 = endpoint.GetDeclaredDeviceIdentity()
                Dim info3 = endpoint.GetDeclaredStreamConfiguration()
                Dim info4 = endpoint.GetDeclaredFunctionBlocks()
                Dim info5 = endpoint.GetGroupTerminalBlocks()
                Dim info6 = endpoint.GetUserSuppliedInfo()
                Dim info7 = endpoint.GetTransportSuppliedInfo()
                Dim info8 = endpoint.GetParentDeviceInformation()
                Dim info9 = endpoint.GetContainerDeviceInformation()        ' properties DeviceName

                WriteMessageLine("DeviceInformation - Name: " & info9.Name)
                'nfo.Properties

                Dim parent = endpoint.GetParentDeviceInformation

                If parent IsNot Nothing Then
                    WriteMessageLine("- Parent:  " & parent.Id)
                Else
                    WriteMessageLine("- Parent:  Unknown")
                End If

                If endpoint.EndpointPurpose = MidiEndpointDevicePurpose.NormalMessageEndpoint Then
                    WriteMessageLine("- Purpose: Application MIDI Traffic")
                ElseIf endpoint.EndpointPurpose = MidiEndpointDevicePurpose.VirtualDeviceResponder Then
                    WriteMessageLine("- Purpose: Virtual Device Responder")
                ElseIf endpoint.EndpointPurpose = MidiEndpointDevicePurpose.DiagnosticLoopback Then
                    WriteMessageLine("- Purpose: Diagnostics use")
                ElseIf endpoint.EndpointPurpose = MidiEndpointDevicePurpose.DiagnosticPing Then
                    WriteMessageLine("- Purpose: Internal Diagnostics Ping")
                Else
                    WriteMessageLine("- Purpose: Unknown")
                End If

                'info gathered through endpoint discovery
                Dim declaredEndpointInfo = endpoint.GetDeclaredEndpointInfo
                WriteMessageLine("Endpoint Metadata")
                WriteMessageLine("- Product Instance Id:    " & declaredEndpointInfo.ProductInstanceId)
                WriteMessageLine("- Endpoint-supplied Name: " & declaredEndpointInfo.Name)

                'Device Identity
                'Dim declaredDeviceIdentity = endpoint.GetDeclaredDeviceIdentity
                'WriteMessageLine("Device Identity")
                'WriteMessageLine("- System Exclusive Id:    " &
                '    declaredDeviceIdentity.SystemExclusiveIdByte1 & "  " &
                '    declaredDeviceIdentity.SystemExclusiveIdByte2 & "  " &
                '    declaredDeviceIdentity.SystemExclusiveIdByte3)
                ' ... all bytes = 0

                WriteMessageLine("AssociatedMidi1PortsForThisEndpoint")
                Dim srcp = endpoint.FindAllAssociatedMidi1PortsForThisEndpoint(Midi1PortFlow.MidiMessageSource)
                Dim dstp = endpoint.FindAllAssociatedMidi1PortsForThisEndpoint(Midi1PortFlow.MidiMessageDestination)


                For Each prt In srcp
                    WriteMessageLine("--> source  " & prt.PortName & "  Num: " & prt.PortNumber & "  GrpVal: " & prt.Group.DisplayValue & "  GrpNdx: " & prt.Group.Index)
                Next

                For Each prt In dstp
                    WriteMessageLine("--> destination  " & prt.PortName & "  Num: " & prt.PortNumber & "  GrpVal: " & prt.Group.DisplayValue & "  GrpNdx: " & prt.Group.Index)
                Next

                'Dim devinfo = endpoint.GetContainerDeviceInformation()

                'Dim prop = devinfo.Properties

                Dim ntable = endpoint.GetNameTable()

                For Each item In ntable

                Next


            Next
        End If

        TbMessage.ScrollToEnd()

    End Sub

    Private Sub BtnSendNote_Click(sender As Object, e As RoutedEventArgs) Handles BtnSendNote.Click

        Dim eplist As IReadOnlyList(Of MidiEndpointDeviceInformation)

        eplist = MidiEndpointDeviceInformation.FindAll(
           MidiEndpointDeviceInformationSortOrder.Name,
       MidiEndpointDeviceInformationFilters.StandardNativeMidi1ByteFormat)

        If eplist IsNot Nothing Then

            Dim nof = eplist(0).GetContainerDeviceInformation
            Dim name As String = nof.Name
            Dim deviceID As String = nof.Id

            'Dim sendConn = Session.CreateEndpointConnection(deviceID)
            Dim sendConn = Session.CreateEndpointConnection(eplist(0).EndpointDeviceId)

            If sendConn.Open() = False Then
                Console.WriteLine("Could not open send endpoint")
                Exit Sub
            End If


            Dim ump32 As MidiMessage32 = MidiMessageBuilder.BuildMidi1ChannelVoiceMessage(
                        MidiClock.Now,                                          ' current time
                        New MidiGroup(0),                                       ' Group 5
                        Midi1ChannelVoiceMessageStatus.NoteOn,                  ' NoteOn (9)
                        New MidiChannel(0),                                     ' channel 3
                        64,                                                    ' note 120  (&h78)
                        100)                                                    ' velocity 100  (&h64)

            Console.WriteLine("Sending MessagePacket")
            ' C# sendEndpoint.SendSingleMessagePacket((IMidiUniversalPacket)ump32);
            sendConn.SendSingleMessagePacket(ump32)
            ' // could also use the SendWords methods, etc.


        End If


    End Sub

End Class
