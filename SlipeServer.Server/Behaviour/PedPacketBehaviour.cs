using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.Behaviour
{
    /// <summary>
    /// Behaviour responsible for handling ped events and sending corresponding packets
    /// </summary>
    public class PedPacketBehaviour
    {
        private readonly MtaServer server;

        public PedPacketBehaviour(MtaServer server)
        {
            this.server = server;

            server.ElementCreated += HandleElementCreate;
        }

        private void HandleElementCreate(Element obj)
        {
            if (obj is Ped ped)
            {
                ped.ModelChanged += RelayModelChange;
                ped.HealthChanged += RelayHealthChange;
                ped.ArmourChanged += RelayArmourChange;
                ped.WeaponSlotChanged += RelayWeaponSlotChange;
                ped.WeaponReceived += RelayPedWeaponReceive;
                ped.WeaponRemoved += RelayPedWeaponRemove;
                ped.AmmoUpdated += RelayPedAmmoCountUpdate;
            }
        }

        private void RelayModelChange(object sender, ElementChangedEventArgs<Ped, ushort> args)
        {
            if (!args.IsSync)
                this.server.BroadcastPacket(PedPacketFactory.CreateSetModelPacket(args.Source));
        }

        private void RelayHealthChange(object sender, ElementChangedEventArgs<Ped, float> args)
        {
            if (!args.IsSync)
                this.server.BroadcastPacket(PedPacketFactory.CreateSetHealthPacket(args.Source));
        }

        private void RelayArmourChange(object sender, ElementChangedEventArgs<Ped, float> args)
        {
            if (!args.IsSync)
                this.server.BroadcastPacket(PedPacketFactory.CreateSetArmourPacket(args.Source));
        }

        private void RelayWeaponSlotChange(object sender, ElementChangedEventArgs<Ped, WeaponSlot> args)
        {
            if (!args.IsSync)
                this.server.BroadcastPacket(new SetWeaponSlotRpcPacket(args.Source.Id, (byte)args.NewValue));
        }

        private void RelayPedWeaponReceive(object? sender, WeaponReceivedEventArgs e)
        {
            this.server.BroadcastPacket(new GiveWeaponRpcPacket(e.Ped.Id, (byte)e.WeaponId, e.AmmoCount, e.SetAsCurrent));
        }

        private void RelayPedWeaponRemove(object? sender, WeaponRemovedEventArgs e)
        {
            this.server.BroadcastPacket(new TakeWeaponRpcPacket(e.Ped.Id, (byte)e.WeaponId, e.AmmoCount));
        }

        private void RelayPedAmmoCountUpdate(object? sender, AmmoUpdateEventArgs e)
        {
            this.server.BroadcastPacket(new SetAmmoCountRpcPacket(e.Ped.Id, (byte)e.WeaponId, e.AmmoCount, e.AmmoInClipCount));
        }
    }
}
