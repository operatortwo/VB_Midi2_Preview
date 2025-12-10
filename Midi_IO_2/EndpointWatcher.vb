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


    Private Sub DelayedEnumerateInOut(sender As Object, e As ElapsedEventArgs)
        MidiInputList.Clear()
        MidiOutputList.Clear()

        '--- inputs
        For Each ep In EndpointList
            Dim PortDevInfoList As IReadOnlyList(Of MidiEndpointAssociatedPortDeviceInformation)
            PortDevInfoList = ep.Value.FindAllAssociatedMidi1PortsForThisEndpoint(Midi1PortFlow.MidiMessageSource, False)

            For Each port In PortDevInfoList
                Dim inp As New MidiInput
                inp.Name = port.PortName
                inp.Group = port.Group.Index
                inp.Endpoint = ep.Value
                MidiInputList.Add(inp)
            Next
        Next

        MidiInputList.Sort(Function(x, y) x.Name.CompareTo(y.Name))

        '--- outputs
        For Each ep In EndpointList
            Dim PortDevInfoList As IReadOnlyList(Of MidiEndpointAssociatedPortDeviceInformation)
            PortDevInfoList = ep.Value.FindAllAssociatedMidi1PortsForThisEndpoint(Midi1PortFlow.MidiMessageDestination, False)

            For Each port In PortDevInfoList
                Dim outp As New MidiOutput
                outp.Name = port.PortName
                outp.Group = port.Group.Index
                outp.Endpoint = ep.Value
                MidiOutputList.Add(outp)
            Next
        Next

        MidiOutputList.Sort(Function(x, y) x.Name.CompareTo(y.Name))

        RaiseEvent MidiInOutListChanged()
    End Sub


End Class
