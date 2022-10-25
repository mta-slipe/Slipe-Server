-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaSharedElements = SlipeLua.Shared.Elements
local SystemNumerics = System.Numerics
local SlipeLuaClientGui
System.import(function (out)
  SlipeLuaClientGui = SlipeLua.Client.Gui
end)
System.namespace("SlipeLua.Client.Gui.Events", function (namespace)
  namespace.class("OnMouseEnterEventArgs", function (namespace)
    local getPosition, getElement, __ctor__
    __ctor__ = function (this, x, y, element)
      this.Position = System.default(SystemNumerics.Vector2)
      this.Position = SystemNumerics.Vector2(System.cast(System.Single, x), System.cast(System.Single, y))
      this.Element = SlipeLuaSharedElements.ElementManager.getInstance():GetElement(element, SlipeLuaClientGui.GuiElement)
    end
    getPosition = System.property("Position", true)
    getElement = System.property("Element", true)
    return {
      getPosition = getPosition,
      getElement = getElement,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          properties = {
            { "Element", 0x206, out.SlipeLua.Client.Gui.GuiElement, getElement },
            { "Position", 0x206, System.Numerics.Vector2, getPosition }
          },
          methods = {
            { ".ctor", 0x304, nil, System.Object, System.Object, out.SlipeLua.MtaDefinitions.MtaElement }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
