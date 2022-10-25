-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaSharedWeapons = SlipeLua.Shared.Weapons
System.namespace("SlipeLua.Client.Peds.Events", function (namespace)
  namespace.class("OnWeaponSwitchEventArgs", function (namespace)
    local getPreviousWeapon, getNewWeapon, __ctor__
    __ctor__ = function (this, previousWeapon, newWeapon)
      this.PreviousWeapon = SlipeLuaSharedWeapons.SharedWeaponModel(System.cast(System.Int32, previousWeapon))
      this.NewWeapon = SlipeLuaSharedWeapons.SharedWeaponModel(System.cast(System.Int32, newWeapon))
    end
    getPreviousWeapon = System.property("PreviousWeapon", true)
    getNewWeapon = System.property("NewWeapon", true)
    return {
      getPreviousWeapon = getPreviousWeapon,
      getNewWeapon = getNewWeapon,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          properties = {
            { "NewWeapon", 0x206, out.SlipeLua.Shared.Weapons.SharedWeaponModel, getNewWeapon },
            { "PreviousWeapon", 0x206, out.SlipeLua.Shared.Weapons.SharedWeaponModel, getPreviousWeapon }
          },
          methods = {
            { ".ctor", 0x204, nil, System.Object, System.Object }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
