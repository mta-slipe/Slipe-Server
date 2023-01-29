using SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;
using SlipeServer.Packets.Definitions.Ped;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;

namespace SlipeServer.Server.Extensions.Relaying;

public static class PedPropertyRelayingExtensions
{
    public static void AddPedRelayers(this Ped ped)
    {
        ped.ModelChanged += RelayModelChange;
        ped.HealthChanged += RelayHealthChange;
        ped.ArmourChanged += RelayArmourChange;
        ped.WeaponSlotChanged += RelayWeaponSlotChange;
        ped.FightingStyleChanged += RelayFightingStyleChange;
        ped.WeaponReceived += RelayPedWeaponReceive;
        ped.WeaponRemoved += RelayPedWeaponRemove;
        ped.AmmoUpdated += RelayPedAmmoCountUpdate;
        ped.JetpackStateChanged += RelayJetpackStateChanged;
        ped.StatChanged += RelayStatChanged;
        ped.ClothingChanged += RelayClothesChanged;
        ped.AnimationStarted += RelayPedAnimationStart;
        ped.AnimationStopped += RelayPedAnimationStop;
        ped.AnimationProgressChanged += RelayPedAnimationProgress;
        ped.AnimationSpeedChanged += RelayPedAnimationSpeed;
        ped.GravityChanged += RelayPedGravityChange;
        ped.WeaponReloaded += RelayWeaponReload;

        if (ped is not Player)
        {
            ped.Wasted += RelayPedWasted;
        }
    }

    private static void RelayPedWasted(Ped sender, PedWastedEventArgs e)
    {
        var packet = new PedWastedPacket(
            e.Source.Id, e.Killer?.Id ?? 0, (byte)e.WeaponType, (byte)e.BodyPart, e.Ammo, false, sender.GetAndIncrementTimeContext(), e.AnimationGroup, e.AnimationId
        )
        {
            Ammo = e.Ammo
        };
        sender.RelayChange(packet);
    }

    private static void RelayClothesChanged(Ped sender, ClothingChangedEventArgs args)
    {
        sender.RelayChange(PedPacketFactory.CreateClothesPacket(args.Ped, args.ClothingType, args.Current));
    }

    private static void RelayJetpackStateChanged(Element sender, ElementChangedEventArgs<Ped, bool> args)
    {
        if (!args.IsSync)
            if (args.NewValue)
                sender.RelayChange(PedPacketFactory.CreateGiveJetpack(args.Source));
            else
                sender.RelayChange(PedPacketFactory.CreateRemoveJetpack(args.Source));
    }

    private static void RelayModelChange(Ped sender, ElementChangedEventArgs<Ped, ushort> args)
    {
        if (!args.IsSync)
            sender.RelayChange(PedPacketFactory.CreateSetModelPacket(args.Source));
    }

    private static void RelayHealthChange(Ped sender, ElementChangedEventArgs<Ped, float> args)
    {
        if (!args.IsSync)
            sender.RelayChange(PedPacketFactory.CreateSetHealthPacket(args.Source));
    }

    private static void RelayArmourChange(Ped sender, ElementChangedEventArgs<Ped, float> args)
    {
        if (!args.IsSync)
            sender.RelayChange(PedPacketFactory.CreateSetArmourPacket(args.Source));
    }

    private static void RelayWeaponSlotChange(Ped sender, ElementChangedEventArgs<Ped, WeaponSlot> args)
    {
        if (!args.IsSync)
            sender.RelayChange(new SetWeaponSlotRpcPacket(args.Source.Id, (byte)args.NewValue));
    }

    private static void RelayFightingStyleChange(Ped sender, ElementChangedEventArgs<Ped, FightingStyle> args)
    {
        if (!args.IsSync)
            sender.RelayChange(new SetPedFightingStyleRpcPacket(args.Source.Id, (byte)args.NewValue));
    }

    private static void RelayPedWeaponReceive(Ped sender, WeaponReceivedEventArgs e)
    {
        sender.RelayChange(new GiveWeaponRpcPacket(e.Ped.Id, (byte)e.WeaponId, e.AmmoCount, e.SetAsCurrent));
    }

    private static void RelayPedWeaponRemove(Ped sender, WeaponRemovedEventArgs e)
    {
        sender.RelayChange(new TakeWeaponRpcPacket(e.Ped.Id, (byte)e.WeaponId, e.AmmoCount));
    }

    private static void RelayPedAmmoCountUpdate(Ped sender, AmmoUpdateEventArgs e)
    {
        sender.RelayChange(new SetAmmoCountRpcPacket(e.Ped.Id, (byte)e.WeaponId, e.AmmoCount, e.AmmoInClipCount));
    }

    private static void RelayStatChanged(Ped sender, PedStatChangedEventArgs e)
    {
        var packet = PedPacketFactory.CreatePlayerStatsPacket(sender);
        sender.RelayChange(packet);
    }

    private static void RelayPedAnimationStart(Ped sender, PedAnimationStartedEventArgs e)
    {
        sender.RelayChange(new SetPedAnimationRpcPacket(
            sender.Id,
            e.Block,
            e.Animation,
            (int)e.Time.TotalMilliseconds,
            e.Loops,
            e.UpdatesPosition,
            e.IsInteruptable,
            e.FreezesOnLastFrame,
            e.BlendTime.Milliseconds,
            e.RetainPedState));
    }

    private static void RelayPedAnimationStop(Ped sender, System.EventArgs e)
    {
        sender.RelayChange(new StopPedAnimationRpcPacket(sender.Id));
    }

    private static void RelayPedAnimationProgress(Ped sender, PedAnimationProgressChangedEventArgs e)
    {
        sender.RelayChange(new SetPedAnimationProgressRpcPacket(sender.Id, e.Animation, e.Progress));
    }

    private static void RelayPedAnimationSpeed(Ped sender, PedAnimationSpeedChangedEventArgs e)
    {
        sender.RelayChange(new SetPedAnimationSpeedRpcPacket(sender.Id, e.Animation, e.Speed));
    }

    private static void RelayPedGravityChange(Ped sender, ElementChangedEventArgs<Ped, float> args)
    {
        sender.RelayChange(new SetPedGravityRpcPacket(sender.Id, args.NewValue));
    }

    private static void RelayWeaponReload(Ped sender, System.EventArgs e)
    {
        sender.RelayChange(new ReloadPedWeaponRpcPacket(sender.Id));
    }
}
