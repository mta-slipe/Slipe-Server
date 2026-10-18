# Slipe Server

Slipe Server is a C# implementation of an [MTA: San Andreas](https://mtasa.com) (Multi Theft Auto) server.

## Goals

- Provide an MTA server that is more performant, more configurable, and more maintainable than the original by upholding higher code standards.
- Offer a platform for running "resources" in C# instead of Lua (Lua is also being worked on). This allows gameplay features to run as native code where MTA's own implementation is Lua-only.

## Architecture

The code is split into layers to keep the project maintainable. A single packet flows through them like this:

- **Network layer** — receives and sends packets. Contains the wrapper around MTA's net library (`SlipeServer.Net`) and the queue handlers. Queue handlers handle a packet, usually by updating element properties and triggering events on them, and in some cases relay messages to other players (e.g. sync queue handlers).
- **Packet layer** (`SlipeServer.Packets`) — only reads and writes packets in accordance with MTA's packet definitions. It knows no MTA-specific types such as element classes, and works with native types plus some relatively simple structs/enums.
- **Element layer** (`SlipeServer.Server`, element classes) — contains the MTA-specific concepts and logic: element classes with their events and properties.
- **Behaviour layer** — handles some of the events triggered on element classes and sends packets to other players to relay the change/event.
- **Logic layer** — also responds to events on element classes, but ideally does not send packets itself and instead calls methods on the element layer. This is the layer end-users (consumers of the library) use for the gameplay features of individual servers.

The element layer also contains classes in the `Services` namespace. These are services for sending packets that are not strictly tied to elements, but are still MTA-related (explosions, chat, fire, and so on).

## Networking and packets

MTA: San Andreas' networking library (based on RakNet) is closed source, so this project calls MTA's `net.dll` through `DllImport`s to a C++ wrapper (`NetModuleWrapper`). Because the project must stay compatible with the official MTA client, it has to send exactly the same packets over the network.

All packet definitions are recreated in C#. They use the `PacketBuilder` and `PacketReader` classes, which represent the net library's bitstream and provide methods for writing MTA-specific structures used in network packets.

To create a packet definition, look at MTA's source code for the same packet and reimplement it based on that. Slipe's packet definitions must contain **only** the logic for reading from or writing to the packet — MTA's packet definitions also contain business logic, which does not belong in this layer.

Example of an MTA packet and its Slipe equivalent:

- Slipe: `SlipeServer.Packets/Definitions/Explosions/ExplosionPacket.cs`
- MTA: `Server/mods/deathmatch/logic/packets/CExplosionSyncPacket.cpp`

## Reference implementation (`Reference/mtasa-blue` submodule)

`Reference/mtasa-blue` is a git submodule pointing at [multitheftauto/mtasa-blue](https://github.com/multitheftauto/mtasa-blue), the source of the original MTA server.

**Treat this submodule as the reference implementation** whenever you reimplement, port, or verify MTA behaviour — packet definitions, sync logic, element semantics, RPCs, scripting functions, and so on. When in doubt about how something is supposed to work, the answer is in there.

Before relying on it, make sure the submodule is initialized and up to date with the most recent version of the branch checked out in it:

```sh
git submodule update --init --remote Reference/mtasa-blue
```

If it is already checked out on a branch, you can instead pull inside it:

```sh
git -C Reference/mtasa-blue pull
```

Run this update before starting work that depends on MTA behaviour. Never edit files inside `Reference/mtasa-blue`: it is a read-only reference, and any changes made there will be lost.

## Building

Any time you make code changes, you should verify the code still builds.