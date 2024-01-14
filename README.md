# DSC Powerseries Neo Integration

 _Integrate with DSC Powerseries Neo alarm panel over your local network_

## Goal

This project is an attempt at communicating with a DSC Neo series alarm panel through a DSC TL280(R)E communicator.  Ultimately, if I get the communication figured out, I want to use this information to create an integration for Home Assistant.

I would like to find collaborators that are either familier with Home Assistant integrations or other DSC integrations.  I know DSC has been around a long time, and I have seen several projects to communicate with older DSC hardware in various ways.  I suspect that once I get past the encryption the underlying protocol might be similar to something that is already exists.  If that proves to be true, then working with someone that is familier with 'how DSC does things' would be helpful.

## Hardware

My setup consists of the following hardware:
- HS2032 - DSC Powerseries Neo alarm panel
- TL280RE - Ethernet communication module.

The HS2032 panel was installed by my builder and seems to be a common consumer/builder grade component.

The TL280RE was purchased and installed by myself for the sole purpose of experimenting.  I believe this is also what would be installed if you have a monitoring service like Alarm.com, so I think this is also a fairly common component.  The TL280 comes in a variety of flavors including cellular enabled models indicated by the suffix letters of the model number.  Mine is an RE model, which indicates serial and ethernet interfaces.  I chose this configuration as I thought it would provide the most surface area to experiment with.  So far, I am only using the ethernet interface, so you don't already have a communicator card, and want to duplicate my results, you could probably save a few bucks by purchasing the TL280E which is the ethernet only model.  For reference, these cards retail in the $200ish USD range.

## What is known so far (Jan-2024)

I have seen that it is possible to communicate with the alarm panel over the local network through the TL280 and see the status of the various zones as well as programming and configuration using the DLS5 software tool.  I believe that control of the arming and disarming is also possible, but I havent actually seen that yet. Seeing this was enough confirmation to make the investment of figuring out how to do this myself.

The communication begins by opening a TCP connection with the TL280 on port 3062.  The TL280 will immedietly send a packet of 56 bytes that eventually can be parsed into information about the alarm panel such as device ID, software revision, and several other fields that may or may not be interesting.  From this point on, all communications are encrypted using an AES ECB block cipher.  The key for the cipher is a mashup of data that is sent in the initial data packet from the TL280.  So far, the project can connect and parse this data.  I am currently working on verifying I have the correct key and successfully decoding the AES wrapper.
