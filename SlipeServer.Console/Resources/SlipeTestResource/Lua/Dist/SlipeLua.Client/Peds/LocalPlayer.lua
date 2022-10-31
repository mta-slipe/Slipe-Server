-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
local SlipeLuaSharedPeds = SlipeLua.Shared.Peds
local SystemNumerics = System.Numerics
local SlipeLuaClientPeds
System.import(function (out)
  SlipeLuaClientPeds = SlipeLua.Client.Peds
end)
System.namespace("SlipeLua.Client.Peds", function (namespace)
  namespace.class("LocalPlayer", function (namespace)
    local instance, getInstance, getForceMap, setForceMap, getBlurLevel, setBlurLevel, getMoney, setMoney, 
    getWantedLevel, getIsMapVisible, getMapBoundingBox, getActiveRadioStation, setActiveRadioStation, DetonateSatchels, GiveMoney, SetHudComponentVisible, 
    IsHudComponentVisible, TakeMoney, PlaySoundFrontEnd, addOnChoke, removeOnChoke, addOnRadioSwitch, removeOnRadioSwitch, addOnStealthKill, 
    removeOnStealthKill, addOnStuntStart, removeOnStuntStart, addOnStuntFinish, removeOnStuntFinish, addOnTarget, removeOnTarget, addOnConsole, 
    removeOnConsole, class, __ctor__
    __ctor__ = function (this, mtaElement)
      SlipeLuaClientPeds.Player.__ctor__(this, mtaElement)
    end
    getInstance = function ()
      if instance == nil then
        instance = class(SlipeLuaMtaDefinitions.MtaClient.GetLocalPlayer())
      end
      return instance
    end
    getForceMap = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.IsPlayerMapForced()
    end
    setForceMap = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.ForcePlayerMap(value)
    end
    getBlurLevel = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GetPlayerBlurLevel()
    end
    setBlurLevel = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.SetPlayerBlurLevel(value)
    end
    getMoney = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GetPlayerMoney()
    end
    setMoney = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.SetPlayerMoney(value, false)
    end
    getWantedLevel = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GetPlayerWantedLevel()
    end
    getIsMapVisible = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.IsPlayerMapVisible()
    end
    getMapBoundingBox = function (this)
      local r = SlipeLuaMtaDefinitions.MtaClient.GetPlayerMapBoundingBox()
      return System.Tuple(SystemNumerics.Vector2(r[1], r[2]), SystemNumerics.Vector2(r[3], r[4]))
    end
    getActiveRadioStation = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GetRadioChannel()
    end
    setActiveRadioStation = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.SetRadioChannel(value)
    end
    DetonateSatchels = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.DetonateSatchels()
    end
    GiveMoney = function (this, amount)
      return SlipeLuaMtaDefinitions.MtaClient.GivePlayerMoney(amount)
    end
    SetHudComponentVisible = function (this, component, visible)
      return SlipeLuaMtaDefinitions.MtaClient.SetPlayerHudComponentVisible(component:EnumToString(SlipeLuaSharedPeds.HudComponent):ToLower(), visible)
    end
    IsHudComponentVisible = function (this, component)
      return SlipeLuaMtaDefinitions.MtaClient.IsPlayerHudComponentVisible(component:EnumToString(SlipeLuaSharedPeds.HudComponent):ToLower())
    end
    TakeMoney = function (this, amount)
      return SlipeLuaMtaDefinitions.MtaClient.TakePlayerMoney(amount)
    end
    PlaySoundFrontEnd = function (this, frontEndSound)
      return SlipeLuaMtaDefinitions.MtaClient.PlaySoundFrontEnd(frontEndSound)
    end
    addOnChoke, removeOnChoke = System.event("OnChoke")
    addOnRadioSwitch, removeOnRadioSwitch = System.event("OnRadioSwitch")
    addOnStealthKill, removeOnStealthKill = System.event("OnStealthKill")
    addOnStuntStart, removeOnStuntStart = System.event("OnStuntStart")
    addOnStuntFinish, removeOnStuntFinish = System.event("OnStuntFinish")
    addOnTarget, removeOnTarget = System.event("OnTarget")
    addOnConsole, removeOnConsole = System.event("OnConsole")
    class = {
      base = function (out)
        return {
          out.SlipeLua.Client.Peds.Player
        }
      end,
      getInstance = getInstance,
      getForceMap = getForceMap,
      setForceMap = setForceMap,
      getBlurLevel = getBlurLevel,
      setBlurLevel = setBlurLevel,
      getMoney = getMoney,
      setMoney = setMoney,
      getWantedLevel = getWantedLevel,
      getIsMapVisible = getIsMapVisible,
      getMapBoundingBox = getMapBoundingBox,
      getActiveRadioStation = getActiveRadioStation,
      setActiveRadioStation = setActiveRadioStation,
      DetonateSatchels = DetonateSatchels,
      GiveMoney = GiveMoney,
      SetHudComponentVisible = SetHudComponentVisible,
      IsHudComponentVisible = IsHudComponentVisible,
      TakeMoney = TakeMoney,
      PlaySoundFrontEnd = PlaySoundFrontEnd,
      addOnChoke = addOnChoke,
      removeOnChoke = removeOnChoke,
      addOnRadioSwitch = addOnRadioSwitch,
      removeOnRadioSwitch = removeOnRadioSwitch,
      addOnStealthKill = addOnStealthKill,
      removeOnStealthKill = removeOnStealthKill,
      addOnStuntStart = addOnStuntStart,
      removeOnStuntStart = removeOnStuntStart,
      addOnStuntFinish = addOnStuntFinish,
      removeOnStuntFinish = removeOnStuntFinish,
      addOnTarget = addOnTarget,
      removeOnTarget = removeOnTarget,
      addOnConsole = addOnConsole,
      removeOnConsole = removeOnConsole,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          fields = {
            { "instance", 0x9, class }
          },
          properties = {
            { "ActiveRadioStation", 0x106, System.Int32, getActiveRadioStation, setActiveRadioStation },
            { "BlurLevel", 0x106, System.Int32, getBlurLevel, setBlurLevel },
            { "ForceMap", 0x106, System.Boolean, getForceMap, setForceMap },
            { "Instance", 0x20E, class, getInstance },
            { "IsMapVisible", 0x206, System.Boolean, getIsMapVisible },
            { "MapBoundingBox", 0x206, System.Tuple, getMapBoundingBox },
            { "Money", 0x106, System.Int32, getMoney, setMoney },
            { "WantedLevel", 0x206, System.Int32, getWantedLevel }
          },
          methods = {
            { ".ctor", 0x106, nil, out.SlipeLua.MtaDefinitions.MtaElement },
            { "DetonateSatchels", 0x86, DetonateSatchels, System.Boolean },
            { "GiveMoney", 0x186, GiveMoney, System.Int32, System.Boolean },
            { "IsHudComponentVisible", 0x186, IsHudComponentVisible, System.Int32, System.Boolean },
            { "PlaySoundFrontEnd", 0x186, PlaySoundFrontEnd, System.Int32, System.Boolean },
            { "SetHudComponentVisible", 0x286, SetHudComponentVisible, System.Int32, System.Boolean, System.Boolean },
            { "TakeMoney", 0x186, TakeMoney, System.Int32, System.Boolean }
          },
          class = { 0x6 }
        }
      end
    }
    return class
  end)
end)