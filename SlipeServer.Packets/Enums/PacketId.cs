﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Packets.Enums
{
    public enum PacketId
    {
        PACKET_ID_SERVER_JOIN = 0,
        PACKET_ID_SERVER_JOIN_DATA,
        PACKET_ID_SERVER_JOIN_COMPLETE,

        PACKET_ID_PLAYER_JOIN,
        PACKET_ID_PLAYER_JOINDATA,
        PACKET_ID_PLAYER_QUIT,
        PACKET_ID_PLAYER_TIMEOUT,

        PACKET_ID_MOD_NAME,
        PACKET_ID_PACKET_PROGRESS,
        PACKET_ID_MTA_RESERVED_03,
        PACKET_ID_MTA_RESERVED_04,
        PACKET_ID_MTA_RESERVED_05,
        PACKET_ID_MTA_RESERVED_06,
        PACKET_ID_MTA_RESERVED_07,
        PACKET_ID_MTA_RESERVED_08,
        PACKET_ID_MTA_RESERVED_09,
        PACKET_ID_MTA_RESERVED_10,
        PACKET_ID_MTA_RESERVED_11,
        PACKET_ID_MTA_RESERVED_12,
        PACKET_ID_MTA_RESERVED_13,
        PACKET_ID_MTA_RESERVED_14,
        PACKET_ID_MTA_RESERVED_15,

        PACKET_ID_END_OF_INTERNAL_PACKETS,
#pragma warning disable CA1069 // Enums values should not be duplicated
        PACKET_ID_SERVER_JOINEDGAME = 22,
#pragma warning restore CA1069 // Enums values should not be duplicated

        PACKET_ID_SERVER_DISCONNECTED,

        // All our outgoing only packets use this
        PACKET_ID_RPC,

        // Player packets
        PACKET_ID_PLAYER_LIST,
        PACKET_ID_PLAYER_SPAWN,
        PACKET_ID_PLAYER_WASTED,
        PACKET_ID_PLAYER_CHANGE_NICK,
        PACKET_ID_PLAYER_STATS,
        PACKET_ID_PLAYER_CLOTHES,

        // Main sync packets
        PACKET_ID_PLAYER_KEYSYNC,
        PACKET_ID_PLAYER_PURESYNC,
        PACKET_ID_PLAYER_VEHICLE_PURESYNC,
        PACKET_ID_LIGHTSYNC,
        PACKET_ID_VEHICLE_RESYNC,
        PACKET_ID_RETURN_SYNC,

        PACKET_ID_EXPLOSION,
        PACKET_ID_FIRE,
        PACKET_ID_PROJECTILE,
        PACKET_ID_DETONATE_SATCHELS,
        PACKET_ID_DESTROY_SATCHELS,

        // Console, chat and command packets
        PACKET_ID_COMMAND,
        PACKET_ID_CHAT_ECHO,
        PACKET_ID_CONSOLE_ECHO,
        PACKET_ID_DEBUG_ECHO,

        // Map related packets
        PACKET_ID_MAP_INFO,
        PACKET_ID_MAP_START,
        PACKET_ID_MAP_RESTART,
        PACKET_ID_MAP_STOP,

        // Entity related packets
        PACKET_ID_ENTITY_ADD,
        PACKET_ID_ENTITY_REMOVE,
        PACKET_ID_PICKUP_HIDESHOW,
        PACKET_ID_PICKUP_HIT_CONFIRM,

        // Vehicle related packets
        PACKET_ID_UNOCCUPIED_VEHICLE_STARTSYNC,
        PACKET_ID_UNOCCUPIED_VEHICLE_STOPSYNC,
        PACKET_ID_UNOCCUPIED_VEHICLE_SYNC,
        PACKET_ID_VEHICLE_SPAWN,
        PACKET_ID_VEHICLE_INOUT,
        PACKET_ID_VEHICLE_DAMAGE_SYNC,
        PACKET_ID_VEHICLE_TRAILER,

        // Ped sync
        PACKET_ID_PED_STARTSYNC,
        PACKET_ID_PED_STOPSYNC,
        PACKET_ID_PED_SYNC,
        PACKET_ID_PED_WASTED,

        // Rcon related
        PACKET_ID_PLAYER_RCON,
        PACKET_ID_PLAYER_RCON_LOGIN,
        PACKET_ID_PLAYER_RCON_KICK,
        PACKET_ID_PLAYER_RCON_BAN,
        PACKET_ID_PLAYER_RCON_MUTE,
        PACKET_ID_PLAYER_RCON_FREEZE,

        // Voice
        PACKET_ID_VOICE_DATA,
        PACKET_ID_VOICE_END,

        // Anticheat memory challenges
        PACKET_ID_CHEAT_CHALLENGEMEMORY,
        PACKET_ID_CHEAT_RETURN,

        // Map related packets
        PACKET_ID_MAP_LIST,
        PACKET_ID_LUA,
        PACKET_ID_LUA_ELEMENT_RPC,

        // GUI related packets
        PACKET_ID_TEXT_ITEM,

        // Score table updates
        PACKET_ID_SCOREBOARD,

        // Team related packets
        PACKET_ID_TEAMS,

        // Lua related packets
        PACKET_ID_LUA_EVENT,

        // Resource related packets
        PACKET_ID_RESOURCE_START,
        PACKET_ID_RESOURCE_STOP,

        // Custom data related packets
        PACKET_ID_CUSTOM_DATA,

        // Camera related packets
        PACKET_ID_CAMERA_SYNC,

        // Object sync
        PACKET_ID_OBJECT_STARTSYNC,
        PACKET_ID_OBJECT_STOPSYNC,
        PACKET_ID_OBJECT_SYNC,

        PACKET_ID_UPDATE_INFO,
        PACKET_ID_DISCONNECT_MESSAGE,
        PACKET_ID_PLAYER_TRANSGRESSION,
        PACKET_ID_PLAYER_DIAGNOSTIC,
        PACKET_ID_PLAYER_MODINFO,

        PACKET_ID_PLAYER_SCREENSHOT,
        PACKET_ID_RESOURCE_CLIENT_SCRIPTS,
        PACKET_ID_LATENT_TRANSFER,

        // Vehicle Push Sync
        PACKET_ID_VEHICLE_PUSH_SYNC,

        PACKET_ID_PLAYER_BULLETSYNC,
        PACKET_ID_SYNC_SETTINGS,
        PACKET_ID_WEAPON_BULLETSYNC,

        PACKET_ID_PED_TASK,

        PACKET_ID_PLAYER_NO_SOCKET,
        PACKET_ID_PLAYER_NETWORK_STATUS,
        PACKET_ID_PLAYER_ACINFO,
        PACKET_ID_CHAT_CLEAR,
        PACKET_ID_SERVER_INFO_SYNC,
    }
}
