-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaSharedElements
local SlipeLuaSharedPeds
System.import(function (out)
  SlipeLuaSharedElements = SlipeLua.Shared.Elements
  SlipeLuaSharedPeds = SlipeLua.Shared.Peds
end)
System.namespace("SlipeLua.Shared.Pickups.Events", function (namespace)
  namespace.class("OnHitArgs", function (namespace)
    local getPlayer, getIsDimensionMatching, __ctor__
    __ctor__ = function (this, hitPlayer, matchingDimension)
      this.Player = SlipeLuaSharedElements.ElementManager.getInstance():GetElement(hitPlayer, SlipeLuaSharedPeds.SharedPed)
      this.IsDimensionMatching = System.cast(System.Boolean, matchingDimension)
    end
    getPlayer = System.property("Player", true)
    getIsDimensionMatching = System.property("IsDimensionMatching", true)
    return {
      getPlayer = getPlayer,
      IsDimensionMatching = false,
      getIsDimensionMatching = getIsDimensionMatching,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          properties = {
            { "IsDimensionMatching", 0x206, System.Boolean, getIsDimensionMatching },
            { "Player", 0x206, out.SlipeLua.Shared.Peds.SharedPed, getPlayer }
          },
          methods = {
            { ".ctor", 0x204, nil, out.SlipeLua.MtaDefinitions.MtaElement, System.Object }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
