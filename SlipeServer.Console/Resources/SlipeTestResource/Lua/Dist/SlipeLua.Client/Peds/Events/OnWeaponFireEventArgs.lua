-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaSharedElements = SlipeLua.Shared.Elements
local SlipeLuaSharedWeapons = SlipeLua.Shared.Weapons
local SystemNumerics = System.Numerics
System.namespace("SlipeLua.Client.Peds.Events", function (namespace)
  namespace.class("OnWeaponFireEventArgs", function (namespace)
    local getWeapon, getHitElement, getHitPosition, getAmmoLeft, getAmmoLeftInClip, __ctor__
    __ctor__ = function (this, weapon, ammoLeft, ammoLeftInClip, ex, ey, ez, hitElement)
      this.HitPosition = System.default(SystemNumerics.Vector3)
      this.Weapon = SlipeLuaSharedWeapons.SharedWeaponModel(System.cast(System.Int32, weapon))
      this.HitElement = SlipeLuaSharedElements.ElementManager.getInstance():GetElement(hitElement, SlipeLuaSharedElements.PhysicalElement)
      this.HitPosition = SystemNumerics.Vector3(System.cast(System.Single, ex), System.cast(System.Single, ey), System.cast(System.Single, ez))
      this.AmmoLeft = System.cast(System.Int32, ammoLeft)
      this.AmmoLeftInClip = System.cast(System.Int32, ammoLeftInClip)
    end
    getWeapon = System.property("Weapon", true)
    getHitElement = System.property("HitElement", true)
    getHitPosition = System.property("HitPosition", true)
    getAmmoLeft = System.property("AmmoLeft", true)
    getAmmoLeftInClip = System.property("AmmoLeftInClip", true)
    return {
      getWeapon = getWeapon,
      getHitElement = getHitElement,
      getHitPosition = getHitPosition,
      AmmoLeft = 0,
      getAmmoLeft = getAmmoLeft,
      AmmoLeftInClip = 0,
      getAmmoLeftInClip = getAmmoLeftInClip,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          properties = {
            { "AmmoLeft", 0x206, System.Int32, getAmmoLeft },
            { "AmmoLeftInClip", 0x206, System.Int32, getAmmoLeftInClip },
            { "HitElement", 0x206, out.SlipeLua.Shared.Elements.PhysicalElement, getHitElement },
            { "HitPosition", 0x206, System.Numerics.Vector3, getHitPosition },
            { "Weapon", 0x206, out.SlipeLua.Shared.Weapons.SharedWeaponModel, getWeapon }
          },
          methods = {
            { ".ctor", 0x704, nil, System.Object, System.Object, System.Object, System.Object, System.Object, System.Object, out.SlipeLua.MtaDefinitions.MtaElement }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
