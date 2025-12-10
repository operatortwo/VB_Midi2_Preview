Imports Microsoft.Windows.Devices.Midi2
Imports Microsoft.Windows.Devices.Midi2.Initialization
Public Class Midi_IO_2

    Private Initializer As MidiDesktopAppSdkInitializer
    Private Session As MidiSession
    Private SessionIsOpen As Boolean

    Public EndpointList As IReadOnlyDictionary(Of String, MidiEndpointDeviceInformation)

    Public MidiInputList As New List(Of MidiInput)
    Public MidiOutputList As New List(Of MidiOutput)

    Public BuildVersion As String = ""
    Public ErrorMessage As String = ""
    Public ReturnMessage As String = ""

    Public Function StartMidiSession() As Boolean
        If Session IsNot Nothing Then Return False

        BuildVersion = Microsoft.Windows.Devices.Midi2.Common.MidiNuGetBuildInformation.BuildFullVersion

        Initializer = MidiDesktopAppSdkInitializer.Create()

        '--- check for initializer, gets Nothing if Midi2 is not installed
        If Initializer Is Nothing Then
            ErrorMessage = "Failed to create MidiDesktopAppSdkInitializer"
            Return False
        End If

        '--- initialize SDK runtime
        If Initializer.InitializeSdkRuntime() = False Then
            ErrorMessage = "Failed to initialize SDK Runtime"
            Return False
        End If

        '--- check if 'Windows MIDI Service' is running, try to start if not
        If Initializer.EnsureServiceAvailable() = False Then
            ErrorMessage = "Failed to get 'Windows MIDI Service' running"
            Return False
        Else
            ReturnMessage = "'Windows MIDI Service' is running"
        End If

        Session = MidiSession.Create("Midi_IO_2 Session")
        SessionIsOpen = True

        StartEndpointWatcher()

        Return True
    End Function

    Public Sub StopMidiSession()
        StopEndpointWatcher()
        If Session IsNot Nothing Then Session.Dispose()
        SessionIsOpen = False
        Session = Nothing
        If Initializer IsNot Nothing Then Initializer.Dispose()
    End Sub



    Public Class MidiOutput
        Public Name As String = ""
        Public Endpoint As MidiEndpointDeviceInformation
        Public Group As Byte
    End Class

    Public Class MidiInput
        Public Name As String = ""
        Public Endpoint As MidiEndpointDeviceInformation
        Public Group As Byte
    End Class


End Class
