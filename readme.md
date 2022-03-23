# Slipe Server
[![Build](https://dev.azure.com/bobvanhooff/slipe/_apis/build/status/3?branchName=master)](https://dev.azure.com/BobvanHooff/Slipe/_build?definitionId=3)
[![Version](https://img.shields.io/nuget/v/SlipeServer.Server)](https://www.nuget.org/packages/SlipeServer.Server)
[![Downloads](https://img.shields.io/nuget/dt/SlipeServer.Server)](https://www.nuget.org/packages/SlipeServer.Server)
[![Discord](https://img.shields.io/discord/555709976082120715?label=Discord)](https://discord.gg/cJTPXFTA)
---

Slipe Server is a C# implementation of an [MTA San Andreas](https://mtasa.com) Server.

## Goals
The goals of Slipe Server is to make an MTA server that's more performant and more configurable. But also more maintainable due to upholding higher code standards.  

Another goal is for Slipe Server to offer a platform to run "resources" in C# instead of Lua. (However Lua is also being worked on). This would allow for running native code for gameplay features where in MTA's own implementation this would all be Lua. 

## Contributing
Anyone is welcome to contribute to Slipe Server. Head over to the [projects pages](https://github.com/mta-slipe/slipe-server/projects) for to do items that are available to work on.  
The best place to contact us with questions and/or general discussion is on [our Discord server](https://discord.gg/cJTPXFTA).

## Networking
MTA San Andreas' networking library (which is based on RakNet by Facebook) is sadly closed-source. This means that this project has to make use of MTA's net.dll to be compatible with the official MTA client.

This is done using `DllImport`s from C# code, calling a C++ wrapper around MTA's Net Library.  

Due to the desire to be compatible with the MTA client (since we have no interest in building our own client) we need to send the same packets over the network.

### Packets
All packet definitions are to be recreated in the C# code. These packets use the `PacketBuilder` and `PacketReader` classes to represent the net library's bitstream. These classes also contain methods for writing MTA-specific structures used in the network packets.

In order to create a packet definition you would look at MTA's source code for the same packet, and reimplement it based on that.  
Do note that MTA's packet definitions have business logic in it as well. Slipe's packet definitions should only contain the logic for reading from or writing to the packet.  

An example of one of MTA's packets and Slipe's equivalent is:  
[Explosion packet Slipe definition](https://github.com/mta-slipe/Slipe-Server/blob/master/SlipeServer.Packets/Definitions/Explosions/ExplosionPacket.cs)  
[Explosion packet MTA definition](https://github.com/multitheftauto/mtasa-blue/blob/master/Server/mods/deathmatch/logic/packets/CExplosionSyncPacket.cpp)  

## Project architecture / layering
In order to keep the project maintainable we split the code up in different layers (and different projects at times).  

The below diagram gives an overview of the lifecycle of a single packet, and which layers do what.
![Slipe Layers](https://i.imgur.com/MvpHD7C.png)

- Network layer  
  The network layer is the layer that receives (or sends) packets.  
  This layer consists of the wrapper around MTA's net library, and queue handlers.  
  Queue handlers are responsible for handling a packet.  
  Handling a packet usually means updating properties on element classes and triggering events on them. (Or rather calling methods that trigger events).  
  In some cases queue handlers also relay messages to other players, like with sync queue handlers.
- Packet layer  
  The packet layer is only responsible for reading or writing packets in accordance with MTA's packet definitions.  
  This layer is placed in a separate project and does not know any MTA specific types like element classes, and thus works mainly in native types and some relatively simple structs / enums.  
- Element layer  
  The element layer contains all element classes, which contain MTA specific concepts and logic. These classes contain all kinds of events and properties related to these elements.  
- Behaviour layer  
  The behaviour layer is responsible for handling some of the events that are triggered on the element classes, and sending packets to other players to relay this change / event.  
- Logic layer
  The logic layer behaves much like the behaviour layer in the way that it responds to events triggered on element classes. Ideally however this layer does not need to send packets to other players but simply does so by calling methods on the event layer.
  This layer is the layer that the "end-user" (the consumer of the Slipe Server library) would use. This layer is thus responsible for the gameplay features of individual servers.  

Besides the layers as described above the "element layer"  also contains some classes in the `Services` namespace. These are services for sending packets that are not strictly related to elements, but still related to MTA.  
Examples of these services are explosions, chat, fire and so on.
