Imports Microsoft.Windows.Devices.Midi2
Imports Microsoft.Windows.Devices.Midi2.Initialization
Public Class Midi_IO_2

    Private Initializer As MidiDesktopAppSdkInitializer
    Private Session As MidiSession
    Private SessionIsOpen As Boolean

    Public EndpointList As IReadOnlyDictionary(Of String, MidiEndpointDeviceInformation)

    Public ReadOnly MidiInputList As New List(Of MidiInput)
    Public ReadOnly MidiOutputList As New List(Of MidiOutput)

    Public BuildVersion As String = ""
    Public ErrorMessage As String = ""
    Public ReturnMessage As String = ""


#Region "Session"
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

#End Region

#Region "Input Output"
    Public Class MidiOutput
        Public Property Name As String = ""
        Public Session As MidiSession
        Public Endpoint As MidiEndpointDeviceInformation
        Public EndpointConnection As MidiEndpointConnection
        Public PortDeviceID As String = ""
        Public ID As Integer
        Public Group As Byte

        Public Function Open() As Boolean
            If Session Is Nothing Then Return False
            If Session.IsOpen = False Then Return False
            If Endpoint Is Nothing Then Return False
            If EndpointConnection IsNot Nothing Then
                If Session.Connections.Values.Contains(EndpointConnection) Then Return True ' already open
            End If
            ' try connect
            EndpointConnection = Session.CreateEndpointConnection(Endpoint.EndpointDeviceId)
            If EndpointConnection Is Nothing Then Return False
            'Wire up the message event handler before open
            EndpointConnection.Open()
            Return True
        End Function


    End Class

    Public Class MidiInput
        Public Property Name As String = ""
        Public Session As MidiSession
        Public Endpoint As MidiEndpointDeviceInformation
        Public EndpointConnection As MidiEndpointConnection
        Public PortDeviceID As String = ""
        Public ID As Integer
        Public Group As Byte


        Public Function Open() As Boolean
            If Session Is Nothing Then Return False
            If Session.IsOpen = False Then Return False
            If Endpoint Is Nothing Then Return False
            If EndpointConnection IsNot Nothing Then
                If Session.Connections.Values.Contains(EndpointConnection) Then Return True ' already open
            End If
            ' try connect
            EndpointConnection = Session.CreateEndpointConnection(Endpoint.EndpointDeviceId)
            If EndpointConnection Is Nothing Then Return False
            'xxxx
            'Wire up the message event handler before open
            EndpointConnection.Open()
            Return True
        End Function

    End Class

    Private InOutBaseValue As Integer = 1
    Private InOutRandom As New Random

    Private Function GetNewInOutID() As Integer
        Dim RndVal As Integer
        Dim NewID As Integer
        RndVal = InOutRandom.Next(31)
        NewID = InOutBaseValue << 5
        NewID = NewID Or RndVal
        InOutBaseValue += 1
        Return NewID
    End Function

#End Region

End Class
