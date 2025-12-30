Imports System.Timers
Imports Microsoft.Windows.Devices.Midi2
Partial Public Class Midi_IO_2

    Private EndpointWatcher As MidiEndpointDeviceWatcher

    Public Event MidiInOutListChanged()
    Private EnumDelay As New Timers.Timer(500)


    Public Sub StartEndpointWatcher()

        EndpointWatcher = MidiEndpointDeviceWatcher.Create()

        AddHandler EndpointWatcher.Added, AddressOf EndpointAdded
        AddHandler EndpointWatcher.Removed, AddressOf EndpointRemoved
        AddHandler EndpointWatcher.Updated, AddressOf EndpointUpdated
        AddHandler EndpointWatcher.EnumerationCompleted, AddressOf EndpointEnumerationCompleted
        AddHandler EndpointWatcher.Stopped, AddressOf EndpointWatcherStopped
        AddHandler EnumDelay.Elapsed, AddressOf DelayedEnumerateInOut

        EnumDelay.AutoReset = False

        EndpointWatcher.Start()
    End Sub


    Public Sub StopEndpointWatcher()
        EndpointWatcher.Stop()

        RemoveHandler EndpointWatcher.Added, AddressOf EndpointAdded
        RemoveHandler EndpointWatcher.Removed, AddressOf EndpointRemoved
        RemoveHandler EndpointWatcher.Updated, AddressOf EndpointUpdated
        RemoveHandler EndpointWatcher.EnumerationCompleted, AddressOf EndpointEnumerationCompleted
        RemoveHandler EndpointWatcher.Stopped, AddressOf EndpointWatcherStopped
        RemoveHandler EnumDelay.Elapsed, AddressOf DelayedEnumerateInOut

        EndpointWatcher = Nothing
    End Sub


    Private Sub EndpointAdded(sender As MidiEndpointDeviceWatcher, args As MidiEndpointDeviceInformationAddedEventArgs)
        Debug.WriteLine("Endpoint Added")
        ' skip added while starting
        If sender.Status = Windows.Devices.Enumeration.DeviceWatcherStatus.EnumerationCompleted Then
            EndpointList = sender.EnumeratedEndpointDevices
            EnumerateInOut()
        End If
    End Sub

    Private Sub EndpointRemoved(sender As MidiEndpointDeviceWatcher, args As MidiEndpointDeviceInformationRemovedEventArgs)
        Debug.WriteLine("Endpoint Removed")
        EnumerateInOut()
    End Sub

    Private Sub EndpointUpdated(sender As MidiEndpointDeviceWatcher, args As MidiEndpointDeviceInformationUpdatedEventArgs)
        Debug.WriteLine("Endpoint Updated")
        EnumerateInOut()
    End Sub

    Private Sub EndpointEnumerationCompleted(sender As MidiEndpointDeviceWatcher, args As Object)
        Debug.WriteLine("EndpointEnumeration Completed")
        EndpointList = sender.EnumeratedEndpointDevices
        EnumerateInOut()
    End Sub


    Private Sub EndpointWatcherStopped(sender As MidiEndpointDeviceWatcher, args As Object)
        Debug.WriteLine("EndpointWatcher stopped")
        MidiOutputList.Clear()
        MidiInputList.Clear()
        RaiseEvent MidiInOutListChanged()
    End Sub

    Public Sub EnumerateInOut()
        EnumDelay.Start()
    End Sub

    Private MidiInputList2 As New List(Of MidiInput)
    Private MidiOutputList2 As New List(Of MidiOutput)

    Private Sub DelayedEnumerateInOut(sender As Object, e As ElapsedEventArgs)
        MidiInputList2.Clear()
        MidiOutputList2.Clear()

        '--- inputs
        For Each ep In EndpointList
            Dim PortDevInfoList As IReadOnlyList(Of MidiEndpointAssociatedPortDeviceInformation)
            PortDevInfoList = ep.Value.FindAllAssociatedMidi1PortsForThisEndpoint(Midi1PortFlow.MidiMessageSource, False)

            For Each port In PortDevInfoList
                Dim inp As New MidiInput
                inp.Name = port.PortName
                inp.Group = port.Group.Index
                inp.PortDeviceID = port.PortDeviceId
                inp.Endpoint = ep.Value
                MidiInputList2.Add(inp)
            Next
        Next

        ModifyInputList(MidiInputList, MidiInputList2)
        MidiInputList.Sort(Function(x, y) x.Name.CompareTo(y.Name))     ' sort by Name

        '--- outputs
        For Each ep In EndpointList
            Dim PortDevInfoList As IReadOnlyList(Of MidiEndpointAssociatedPortDeviceInformation)
            PortDevInfoList = ep.Value.FindAllAssociatedMidi1PortsForThisEndpoint(Midi1PortFlow.MidiMessageDestination, False)

            For Each port In PortDevInfoList
                Dim outp As New MidiOutput
                outp.Name = port.PortName
                outp.Group = port.Group.Index
                outp.PortDeviceID = port.PortDeviceId
                outp.Endpoint = ep.Value
                MidiOutputList2.Add(outp)
            Next
        Next

        ModifyOutputList(MidiOutputList, MidiOutputList2)
        MidiOutputList.Sort(Function(x, y) x.Name.CompareTo(y.Name))    ' sort by name

        RaiseEvent MidiInOutListChanged()
    End Sub


    Private Sub ModifyInputList(ByRef list1 As List(Of MidiInput), list2 As List(Of MidiInput))

        '--- add new Inputs (contained in List2 but not in List1) ---
        For Each inp In list2
            If list1.Exists(Function(x) x.Name = inp.Name) = False Then
                inp.Session = Session
                inp.ID = GetNewInOutID()
                list1.Add(inp)
            End If
        Next

        '--- remove unlisted Outputs (contained in List1 but not in List2 ---
        For i = list1.Count - 1 To 0 Step -1
            Dim inp As MidiInput = list1(i)
            If list2.Exists(Function(x) x.Name = inp.Name) = False Then
                list1.Remove(inp)
            End If
        Next

    End Sub


    Private Sub ModifyOutputList(ByRef list1 As List(Of MidiOutput), list2 As List(Of MidiOutput))
        '--- add new Outputs (contained in List2 but not in List1) ---
        For Each outp In list2
            If list1.Exists(Function(x) x.Name = outp.Name) = False Then
                outp.Session = Session
                outp.ID = GetNewInOutID()
                list1.Add(outp)
            End If
        Next

        '--- remove unlisted Outputs (contained in List1 but not in List2 ---
        For i = list1.Count - 1 To 0 Step -1
            Dim outp As MidiOutput = list1(i)
            If list2.Exists(Function(x) x.Name = outp.Name) = False Then
                list1.Remove(outp)
            End If
        Next
    End Sub

End Class
