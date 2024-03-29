-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
local SlipeLuaSharedElements
local SlipeLuaSharedUtilities
System.import(function (out)
  SlipeLuaSharedElements = SlipeLua.Shared.Elements
  SlipeLuaSharedUtilities = SlipeLua.Shared.Utilities
end)
System.namespace("SlipeLua.Shared.Radar", function (namespace)
  --/ <summary>
  --/ Class representing a minimap blip
  --/ </summary>
  namespace.class("SharedBlip", function (namespace)
    local getColor, setColor, getIcon, setIcon, getOrdering, setOrdering, getSize, setSize, 
    getVisibleDistance, setVisibleDistance, __ctor__
    __ctor__ = function (this, element)
      SlipeLuaSharedElements.PhysicalElement.__ctor__(this, element)
    end
    getColor = function (this)
      local tuple = SlipeLuaMtaDefinitions.MtaShared.GetBlipColor(this.element)
      return System.new(SlipeLuaSharedUtilities.Color, 3, System.toByte(tuple[1]), System.toByte(tuple[2]), System.toByte(tuple[3]), System.toByte(tuple[4]))
    end
    setColor = function (this, value)
      SlipeLuaMtaDefinitions.MtaShared.SetBlipColor(this.element, value:getR(), value:getG(), value:getB(), value:getA())
    end
    getIcon = function (this)
      return SlipeLuaMtaDefinitions.MtaShared.GetBlipIcon(this.element)
    end
    setIcon = function (this, value)
      SlipeLuaMtaDefinitions.MtaShared.SetBlipIcon(this.element, value)
    end
    getOrdering = function (this)
      return SlipeLuaMtaDefinitions.MtaShared.GetBlipOrdering(this.element)
    end
    setOrdering = function (this, value)
      SlipeLuaMtaDefinitions.MtaShared.SetBlipOrdering(this.element, value)
    end
    getSize = function (this)
      return SlipeLuaMtaDefinitions.MtaShared.GetBlipSize(this.element)
    end
    setSize = function (this, value)
      SlipeLuaMtaDefinitions.MtaShared.SetBlipSize(this.element, value)
    end
    getVisibleDistance = function (this)
      return SlipeLuaMtaDefinitions.MtaShared.GetBlipVisibleDistance(this.element)
    end
    setVisibleDistance = function (this, value)
      SlipeLuaMtaDefinitions.MtaShared.SetBlipVisibleDistance(this.element, value)
    end
    return {
      base = function (out)
        return {
          out.SlipeLua.Shared.Elements.PhysicalElement
        }
      end,
      getColor = getColor,
      setColor = setColor,
      getIcon = getIcon,
      setIcon = setIcon,
      getOrdering = getOrdering,
      setOrdering = setOrdering,
      getSize = getSize,
      setSize = setSize,
      getVisibleDistance = getVisibleDistance,
      setVisibleDistance = setVisibleDistance,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          properties = {
            { "Color", 0x106, out.SlipeLua.Shared.Utilities.Color, getColor, setColor },
            { "Icon", 0x106, System.Int32, getIcon, setIcon },
            { "Ordering", 0x106, System.Int32, getOrdering, setOrdering },
            { "Size", 0x106, System.Int32, getSize, setSize },
            { "VisibleDistance", 0x106, System.Single, getVisibleDistance, setVisibleDistance }
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
