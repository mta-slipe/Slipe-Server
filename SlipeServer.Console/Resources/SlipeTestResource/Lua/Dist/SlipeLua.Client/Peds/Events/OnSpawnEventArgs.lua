-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaSharedElements = SlipeLua.Shared.Elements
local SlipeLuaClientGame
System.import(function (out)
  SlipeLuaClientGame = SlipeLua.Client.Game
end)
System.namespace("SlipeLua.Client.Peds.Events", function (namespace)
  namespace.class("OnSpawnEventArgs", function (namespace)
    local getTeam, __ctor__
    __ctor__ = function (this, team)
      this.Team = SlipeLuaSharedElements.ElementManager.getInstance():GetElement(team, SlipeLuaClientGame.Team)
    end
    getTeam = System.property("Team", true)
    return {
      getTeam = getTeam,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          properties = {
            { "Team", 0x206, out.SlipeLua.Client.Game.Team, getTeam }
          },
          methods = {
            { ".ctor", 0x104, nil, out.SlipeLua.MtaDefinitions.MtaElement }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
