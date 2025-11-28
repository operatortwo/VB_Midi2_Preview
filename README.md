# How to access Midi 2.0 Services in Windows using Visual Basic

### This Repository is related to  www.github.com/microsoft/midi , the upcoming integration of Midi 2.0 into Windows.  
#### Basics_Console is based on the C# example www.github.com/microsoft/MIDI/tree/main/samples/csharp-net/basics 

Currently, there is only a preview version of Midi 2 available for Windows.  
On a standard Windows installation, the applications will naturally not be able to execute any Midi2 functions. However, you can check 
if the development environment is able to build the application and the Midi2 library can be connected.  
Once Midi2 is rolled out, we can continue working from this basis.  
I have tested the Applications on a Coputer with a normal Installation and on a Computer with the Midi2-SDK and Tools installed.  
**The installation of the Midi2 SDK should only be done on a computer that is used exclusively for development.**  


## Background
At first, I was shocked when I read that it is expected that C++/WinRT is the primary way developers will use the Midi2 API and SDK.  
Do I now have to either stick with MIDI 1.0 and WinMM, or leave VB, switch to C# and learn to program Windows apps?  
The answer is: None of that, under certain conditions.

I spent many hours figuring out what is necessary to use the new technology. The main part was working on the settings of Visual Studio and selecting the right tools and packages. The goal was to find only the necessary accessories and not to add additional (superfluous) parts indiscriminately.

Based on my experience so far, the following is necessary:
-	A modern Computer with Windows 11, Build 26100.0 (24H2) or up.
-	Visual Studio 2022 (V 17.14.xx) and working NuGet Packet Manager (normally integrated)
    - The basic .Net desktop development Workload seems to be sufficient. So far, I have not needed the Workloads for C++ or UI3.
    - Windows SDK must be installed.
-	It is required to have the will to develop for the .NET platform 8.0 or up (.NET Framework is not sufficient)
 In .NET 8.0, you have the choice between Console, WinForms and WPF application.
-	The CsWinRT package, can be obtained from NuGet
-	The Microsoft.Windows.Devices.Midi2.1.0.13-preview.13.192.nupkg, must be downloaded from /Microsoft/MIDI/Releases  
   [preview 13.nupkg](https://github.com/microsoft/MIDI/releases/download/preview-13/Microsoft.Windows.Devices.Midi2.1.0.13-preview.13.192.nupkg)
-	Sometimes a little patience is needed. It is not always obvious what the error messages and warnings mean.
- It is iportant to set the project target to  .NET 8.0 - Windows - 10.0.26100.0 - minimal Version 10.0.26100.0 (or up). Lower target versions will not work. However, this is logical, as the features will only be implemented in future operating system versions and there is no way to provide older OS versions with these updates.
- To get rid of the MSB3270 warning, you need to change the setting [Project]/Properties/Compile/Options/Target CPU from 'Any CPU' to 'x64'  


Project file contains:
```
<TargetFramework>net8.0-windows10.0.26100.0</TargetFramework>

<PlatformTarget>x64</PlatformTarget>
```

## Basics_Console  

 - On a normal Computer the following apperars in the Console window
````
Hello Developer... Testing access to Midi2 Library
-----------
Microsoft.Windows.Devices.Midi2  1.0.13-preview.13.192
Failed to create MidiDesktopAppSdkInitializer
Press ESC to close the Console
````
We can see the *BuildFullVersion* Constant from the Midi2 library  
_Initialization.MidiDesktopAppSdkInitializer.Create()_ fails, since there is no Midi2 on the computer.

- On the Development Computer with Midi2-SDK and Tools installed the following apperars in the Console window

````
Hello Developer... Testing access to Midi2 Library
-----------
Microsoft.Windows.Devices.Midi2  1.0.13-preview.13.192
'Windows MIDI Service' is running
Creating MidiSession
Connecting to Sender UMP Endpoint: \\?\swd#midisrv#midiu_diag_loopback_a#{e7cce071-3c03-423f-88d3-f1045d02552b}
Connecting to Receiver UMP Endpoint: \\?\swd#midisrv#midiu_diag_loopback_b#{e7cce071-3c03-423f-88d3-f1045d02552b}
Opening endpoint connection
Creating MIDI 1.0 Channel Voice 32-bit UMP
Sending MessagePacket

 ** Wait for the message to arrive, and then press a key to cleanup. **

--> Received UMP <--

- Current Timestamp: 6020654809
- UMP Timestamp:     6020519651
- UMP Msg Type:      2
- UMP Packet Type:   1
- Message:           MIDI 1.0 Note On
- Word 0:            0x25937864

--> End of Message <--
````
First we get the Library-Version. After Initalization, we see that the Midi Service is running. We create a new Session and connectiong to the Loopback-Device (Built-in Diagnostic Software Device). After AddHandler for MessageReceive we open the Endpoints for send an receive. A Midi 1.0 Channel Voice Message is created (Note On)
and sent to the sendEndpoint.  
We receive the Message in the MessageReceivedHandler Sub.  
We can see the Timestamp difference = 135'158 Ticks /10'000 (Ticks/Millisecond) = 13.5158 Milliseconds. Word_0 contains the Midi-Message: 2 for the Msg Type, 5 is the Group we defined in the Message, then the usual 93 78 64 (Note On, Channel 3, Note 78, Velocity 64)  
After a key was pressed we RemoveHandler for MessageReceive and disconnect from send- and receive- endpoint.

The MessageReceiveHandler Sub should return as soon as possible. Calls to UI-controls should be avoided. 

## Basics_Winforms and Basics_WPF

These are the examples for the WinForms and WPF project types.  
First, an error occurred during Build: Syntax error in *WinRTEventHelpers.cs*. This temporary .cs file was created by CsWinRT but the compiler treated it like a .vb file...  
As a workaround, the C# console project _GetProjection was added to the solution and the
packages CsWinRT and Devices.Midi2 were moved there. Now we have to build XProjectionBuilder only once to create the 
Midi2.NetProjection.   
Then we can add a reference in the Winforms and WPF projects pointing to the output path of XProjectionBuilder, to be precise to /runtimes/win-x64/lib/net8.0/  
  
Furthermore, in Winforms and WPF, we cannot write directly to the user interface within the MessageReceive handler.
Instead, the application uses a *ConcurrentQueue(Of UInteger)* which is Enqueued in 'MessageReceive' and Dequeued in 'ReadMidiInput' which is regularly called by a timer.

## Basics_WPF_Direct

The following is added to the Project-file
```
<PropertyGroup>
    <CsWinRTEnabled>false</CsWinRTEnabled>
</PropertyGroup>
```

This allows a Visual Basic project to use the midi2 package without having to go through referencing.
This makes *Syntax error in WinRTEventHelpers.cs* disappear.

## Samples

Includes various tests.
