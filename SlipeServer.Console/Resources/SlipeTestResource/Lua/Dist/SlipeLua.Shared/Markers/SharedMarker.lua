-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
local SystemNumerics = System.Numerics
local SlipeLuaSharedElements
local SlipeLuaSharedMarkers
local SlipeLuaSharedUtilities
System.import(function (out)
  SlipeLuaSharedElements = SlipeLua.Shared.Elements
  SlipeLuaSharedMarkers = SlipeLua.Shared.Markers
  SlipeLuaSharedUtilities = SlipeLua.Shared.Utilities
end)
System.namespace("SlipeLua.Shared.Markers", function (namespace)
  --/ <summary>
  --/ Class that represents different types of markers
  --/ </summary>
  namespace.class("SharedMarker", function (namespace)
    local getCount, getColor, setColor, getIcon, setIcon, getSize, setSize, getTarget, 
    setTarget, getMarkerType, setMarkerType, addOnHit, removeOnHit, addOnLeave, removeOnLeave, __ctor__
    __ctor__ = function (this, element)
      SlipeLuaSharedElements.PhysicalElement.__ctor__(this, element)
    end
    getCount = function ()
      return SlipeLuaMtaDefinitions.MtaShared.GetMarkerCount()
    end
    getColor = function (this)
      local tuple = SlipeLuaMtaDefinitions.MtaShared.GetMarkerColor(this.element)
      return System.new(SlipeLuaSharedUtilities.Color, 3, System.toByte(tuple[1]), System.toByte(tuple[2]), System.toByte(tuple[3]), System.toByte(tuple[4]))
    end
    setColor = function (this, value)
      SlipeLuaMtaDefinitions.MtaShared.SetMarkerColor(this.element, value:getR(), value:getG(), value:getB(), value:getA())
    end
    getIcon = function (this)
      local _, result = System.Enum.TryParse(SlipeLuaSharedMarkers.MarkerIcon, SlipeLuaMtaDefinitions.MtaShared.GetMarkerIcon(this.element), true)
      return result
    end
    setIcon = function (this, value)
      SlipeLuaMtaDefinitions.MtaShared.SetMarkerIcon(this.element, value:EnumToString(SlipeLuaSharedMarkers.MarkerIcon):ToLower())
    end
    getSize = function (this)
      return SlipeLuaMtaDefinitions.MtaShared.GetMarkerSize(this.element)
    end
    setSize = function (this, value)
      SlipeLuaMtaDefinitions.MtaShared.SetMarkerSize(this.element, value)
    end
    getTarget = function (this)
      local result = SlipeLuaMtaDefinitions.MtaShared.GetMarkerTarget(this.element)
      return SystemNumerics.Vector3(result[1], result[2], result[3])
    end
    setTarget = function (this, value)
      SlipeLuaMtaDefinitions.MtaShared.SetMarkerTarget(this.element, value.X, value.Y, value.Z)
    end
    getMarkerType = function (this)
      local _, result = System.Enum.TryParse(SlipeLuaSharedMarkers.MarkerType, SlipeLuaMtaDefinitions.MtaShared.GetMarkerType(this.element), true)
      return result
    end
    setMarkerType = function (this, value)
      SlipeLuaMtaDefinitions.MtaShared.SetMarkerType(this.element, value:EnumToString(SlipeLuaSharedMarkers.MarkerType):ToLower())
    end
    addOnHit, removeOnHit = System.event("OnHit")
    addOnLeave, removeOnLeave = System.event("OnLeave")
    return {
      base = function (out)
        return {
          out.SlipeLua.Shared.Elements.PhysicalElement
        }
      end,
      getCount = getCount,
      getColor = getColor,
      setColor = setColor,
      getIcon = getIcon,
      setIcon = setIcon,
      getSize = getSize,
      setSize = setSize,
      getTarget = getTarget,
      setTarget = setTarget,
      getMarkerType = getMarkerType,
      setMarkerType = setMarkerType,
      addOnHit = addOnHit,
      removeOnHit = removeOnHit,
      addOnLeave = addOnLeave,
      removeOnLeave = removeOnLeave,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          properties = {
            { "Color", 0x106, out.SlipeLua.Shared.Utilities.Color, getColor, setColor },
            { "Count", 0x20E, System.Int32, getCount },
            { "Icon", 0x106, System.Int32, getIcon, setIcon },
            { "MarkerType", 0x106, System.Int32, getMarkerType, setMarkerType },
            { "Size", 0x106, System.Single, getSize, setSize },
            { "Target", 0x106, System.Numerics.Vector3, getTarget, setTarget }
          },
          methods = {
            { ".ctor", 0x106, nil, out.SlipeLua.MtaDefinitions.MtaElement }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
