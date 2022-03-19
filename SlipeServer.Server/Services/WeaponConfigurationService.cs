using SlipeServer.Packets;
using SlipeServer.Server.Constants;
using SlipeServer.Server.ElementConcepts;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server.Services
{
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
            var packet = CreateSetStatPacket(weapon, weaponConfiguration, skill);
            this.server.BroadcastPacket(packet);
        }

        public void SetWeaponConfigurationFor(WeaponId weapon, WeaponConfiguration weaponConfiguration, IEnumerable<Player> players, WeaponSkillLevel skill = WeaponSkillLevel.Poor)
        {
            CreateSetStatPacket(weapon, weaponConfiguration, skill).SendTo(players);
        }

        public void SetWeaponConfigurationFor(WeaponId weapon, WeaponConfiguration weaponConfiguration, Player player, WeaponSkillLevel skill = WeaponSkillLevel.Poor)
        {
            SetWeaponConfigurationFor(weapon, weaponConfiguration, new Player[] { player }, skill);
        }

        private Packet CreateSetStatPacket(WeaponId weapon, WeaponConfiguration weaponConfiguration, WeaponSkillLevel skill)
        {
            throw new NotImplementedException();
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
}
