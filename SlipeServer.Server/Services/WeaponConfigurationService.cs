using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Weapons;
using SlipeServer.Server.Constants;
using SlipeServer.Server.ElementConcepts;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server.Services;

public class WeaponConfigurationService
{
    private readonly Dictionary<WeaponId, Dictionary<WeaponSkillLevel, WeaponConfiguration>> weaponConfigurations;
    private readonly MtaServer server;

    public WeaponConfigurationService(MtaServer server)
    {
        this.server = server;

        this.weaponConfigurations = new();
        this.LoadDefaults();
    }

    public void SetWeaponConfiguration(WeaponId weapon, WeaponConfiguration weaponConfiguration, WeaponSkillLevel skill = WeaponSkillLevel.Poor)
    {
        this.weaponConfigurations[weapon][skill] = weaponConfiguration;
        var packets = CreateSetStatPackets(weapon, weaponConfiguration, skill);
        foreach (var packet in packets)
            this.server.BroadcastPacket(packet);
    }

    public void SetWeaponConfigurationFor(WeaponId weapon, WeaponConfiguration weaponConfiguration, IEnumerable<Player> players, WeaponSkillLevel skill = WeaponSkillLevel.Poor)
    {
        foreach (var packet in CreateSetStatPackets(weapon, weaponConfiguration, skill))
            packet.SendTo(players);
    }

    public void SetWeaponConfigurationFor(WeaponId weapon, WeaponConfiguration weaponConfiguration, Player player, WeaponSkillLevel skill = WeaponSkillLevel.Poor)
    {
        SetWeaponConfigurationFor(weapon, weaponConfiguration, new Player[] { player }, skill);
    }

    private IEnumerable<SetWeaponPropertyRpcPacket> CreateSetStatPackets(WeaponId weapon, WeaponConfiguration weaponConfiguration, WeaponSkillLevel skill)
    {
        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.FireType, (byte)skill, (short)weaponConfiguration.FireType);
        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.TargetRange, (byte)skill, weaponConfiguration.TargetRange);
        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.WeaponRange, (byte)skill, weaponConfiguration.WeaponRange);

        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.Flags, (byte)skill, weaponConfiguration.Flags);

        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.Damage, (byte)skill, weaponConfiguration.Damage);

        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.Accuracy, (byte)skill, weaponConfiguration.Accuracy);
        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.MoveSpeed, (byte)skill, weaponConfiguration.MoveSpeed);

        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.AnimationLoopStart, (byte)skill, weaponConfiguration.AnimationLoopStart);
        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.AnimationLoopStop, (byte)skill, weaponConfiguration.AnimationLoopStop);
        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.AnimationLoopReleaseBulletTime, (byte)skill, weaponConfiguration.AnimationLoopBulletFire);

        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.Animation2LoopStart, (byte)skill, weaponConfiguration.Animation2LoopStart);
        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.Animation2LoopStop, (byte)skill, weaponConfiguration.Animation2LoopStop);
        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.Animation2LoopReleaseBulletTime, (byte)skill, weaponConfiguration.Animation2LoopBulletFire);

        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.AnimationBreakoutTime, (byte)skill, weaponConfiguration.AnimationBreakoutTime);


        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.Model, (byte)skill, weaponConfiguration.Model);
        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.Model2, (byte)skill, weaponConfiguration.Model2);

        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.AnimationGroup, (byte)skill, weaponConfiguration.AnimationGroup);

        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.MaxClipAmmo, (byte)skill, weaponConfiguration.MaximumClipAmmo);
        //yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.FireOffset, (byte)skill, weaponConfiguration.FireOffset);

        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.SkillLevel, (byte)skill, (short)weaponConfiguration.SkillLevel);
        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.RequiredSkillLevel, (byte)skill, weaponConfiguration.RequiredSkillLevelStat);

        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.FiringSpeed, (byte)skill, weaponConfiguration.FiringSpeed);
        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.Radius, (byte)skill, weaponConfiguration.Radius);
        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.LifeSpan, (byte)skill, weaponConfiguration.LifeSpan);
        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.Spread, (byte)skill, weaponConfiguration.Spread);

        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.AimOffset, (byte)skill, weaponConfiguration.AimOffset);
        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.DefaultCombo, (byte)skill, weaponConfiguration.DefaultCombo);
        yield return new SetWeaponPropertyRpcPacket((byte)weapon, (byte)WeaponProperty.CombosAvailable, (byte)skill, weaponConfiguration.CombosAvailable);

    }

    public WeaponConfiguration GetWeaponConfiguration(WeaponId weapon, WeaponSkillLevel skill = WeaponSkillLevel.Poor)
    {
        return this.weaponConfigurations[weapon][skill];
    }

    private void LoadDefaults()
    {
        foreach (var weapon in Enum.GetValues<WeaponId>())
        {
            this.weaponConfigurations[weapon] = new();
            foreach (var skill in Enum.GetValues<WeaponSkillLevel>())
            {
                this.weaponConfigurations[weapon][skill] = GetDefaultConfiguration(weapon, skill);
            }
        }
    }

    private WeaponConfiguration GetDefaultConfiguration(WeaponId weapon, WeaponSkillLevel skill)
    {
        if (!WeaponConfigurationConstants.Defaults.ContainsKey(weapon))
            return new() { WeaponType = weapon };

        if (!WeaponConfigurationConstants.Defaults[weapon].ContainsKey(skill))
        {
            if (WeaponConfigurationConstants.Defaults[weapon].ContainsKey(WeaponSkillLevel.Std))
                return WeaponConfigurationConstants.Defaults[weapon][WeaponSkillLevel.Std];

            return new() { WeaponType = weapon };
        }

        return WeaponConfigurationConstants.Defaults[weapon][skill];
    }
}
