using SlipeServer.Server.ElementConcepts;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System.Collections.Generic;

namespace SlipeServer.Server.Services;

public interface IWeaponConfigurationService
{
    WeaponConfiguration GetWeaponConfiguration(WeaponId weapon, WeaponSkillLevel skill = WeaponSkillLevel.Poor);
    void SetWeaponConfiguration(WeaponId weapon, WeaponConfiguration weaponConfiguration, WeaponSkillLevel skill = WeaponSkillLevel.Poor);
    void SetWeaponConfigurationFor(WeaponId weapon, WeaponConfiguration weaponConfiguration, IEnumerable<Player> players, WeaponSkillLevel skill = WeaponSkillLevel.Poor);
    void SetWeaponConfigurationFor(WeaponId weapon, WeaponConfiguration weaponConfiguration, Player player, WeaponSkillLevel skill = WeaponSkillLevel.Poor);
}