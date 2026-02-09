# DSC Powerseries Neo Integration

 _Integrate with DSC Powerseries Neo alarm panel over your local network_

## Hardware

My setup consists of the following hardware:
- HS2032 - DSC Powerseries Neo alarm panel
- TL280RE - Ethernet communication module.

The HS2032 panel was installed by my builder and seems to be a common consumer/builder grade component.

The TL280RE was purchased and installed by myself for the sole purpose of experimenting.  I believe this is also what would be installed if you have a monitoring service like Alarm.com, so I think this is also a fairly common component.  The TL280 comes in a variety of flavors including cellular enabled models indicated by the suffix letters of the model number.  Mine is an RE model, which indicates serial and ethernet interfaces.  I chose this configuration as I thought it would provide the most surface area to experiment with.  So far, I am only using the ethernet interface, so you don't already have a communicator card, and want to duplicate my results, you could probably save a few bucks by purchasing the TL280E which is the ethernet only model.  For reference, these cards retail in the $200ish USD range.


## Integration Setup Guide

### Prerequisites

- **.NET 8 SDK** installed on your machine ([download](https://dot.net/download))
- The **installer code** for your DSC panel (default is 5555 or 5555 or 1234 or CAFE, but may have been changed by your installer)
- **DLS5** software (optional, but makes configuration easier) — available from [DSC's documentation site](https://docs.johnsoncontrols.com/dsc/search/all?content-lang=en-US)
- Your computer and the TL280 must be on the **same local network** (or reachable via routing/VPN)

### Step 1: Build and Run the Server

```bash
cd src
dotnet restore "DSC TLink.sln"
dotnet build "DSC TLink.sln"
dotnet run --project Demo/Demo.csproj -- <integrationId10char> <32charhexencryptionKeyfrom*8[installercode][851][701]>
```

The server will start listening on TCP port 3072 (Integration Notification port). Press Ctrl+C to stop.

### Step 2: Configure the TL280 for Integration

The TL280 needs to be configured to connect to your server. This can be done either via DLS5 software or directly on the panel keypad.

#### Option A: Via DLS5 Software

Navigate to **Integration Options** > **Session 1 Integration Opt** and configure the following:

![DLS5 Integration Session Configuration](docs/images/dls5_integration_session.png)

| Section | Setting | Value |
|---------|---------|-------|
| `[851][450]` | Type 1 Integration Access Code | Your access code (e.g. `12345678`) |
| `[851][701]` | Type 2 Integration Access Code | Your 32-char hex key |
| `[851][452]-4` | Integration Encryption Type | `Type 2` |
| `[851][452]` | Interactive Configuration | `Integration Over Ethernet` |
| `[851][453]-3` | Real-Time Notification Enabled | `Yes` |
| `[851][453]-4` | Notification Port Selection | `Notification Port` |
| `[851][455]` | Integration Server IP | IP address of your server |
| `[851][456]` | Integration Notification Port | `3072` (default, matches the demo app) |

#### Option B: Via Panel Keypad

Enter `[*][8][installer code][851]` on the keypad to access communicator configuration, then program each section:

1. `[450]` — Set the Type 1 Integration Access Code
2. `[701]` — Set the Type 2 Integration Access Code (32-character hex key)
3. `[452]` — Set Integration Encryption Type to Type 2 (toggle option 4)
4. `[452]` — Set Interactive Configuration to Integration Over Ethernet
5. `[453]` — Enable Real-Time Notification (toggle option 3), set Notification Port Selection to Notification Port (toggle option 4)
6. `[455]` — Enter the IP address of the machine running the server
7. `[456]` — Set to `0C00` (hex for port 3072)

### Step 3: Update the Server Configuration

Edit `src/TLink/ITv2/ITv2Server.TLinkServerConnection.cs` and update the following values to match your panel configuration:

- **`integrationId`** — Your integration account ID
- **Type 2 encryption key** — The 32-character hex key matching `[851][701]` on your panel

### Step 4: Verify the Connection

After starting the server and configuring the TL280, the panel should connect and you will see the ITv2 handshake in the console logs. Once connected, incoming notifications (zone status, arming/disarming events, trouble conditions, etc.) will be printed to the console with their decoded command type and data.

## What is known so far (Jan-2024)

I have seen that it is possible to communicate with the alarm panel over the local network through the TL280 and see the status of the various zones as well as programming and configuration using the DLS5 software tool.  I believe that control of the arming and disarming is also possible, but I havent actually seen that yet. Seeing this was enough confirmation to make the investment of figuring out how to do this myself.

The communication begins by opening a TCP connection with the TL280 on port 3062.  The TL280 will immedietly send a packet of 56 bytes that eventually can be parsed into information about the alarm panel such as device ID, software revision, and several other fields that may or may not be interesting.  From this point on, all communications are encrypted using an AES ECB block cipher.  The key for the cipher is a mashup of data that is sent in the initial data packet from the TL280.  So far, the project can connect and parse this data.  I have verified that I can generate the correct key and successfuly decode the AES cipher.

## Update (Aug-2024)
I have expanded the functionality the the TLink Client as well as made progress in figuring out more pieces of the communication.  At this time, it appears that what I am working with is DLS specific and I may need to pivot to the ITV2 interface.
I also have added a [References](References.md) section that has some useful manufacturer documentation.
