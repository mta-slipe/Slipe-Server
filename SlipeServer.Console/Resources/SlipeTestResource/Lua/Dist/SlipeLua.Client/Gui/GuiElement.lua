-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
local SlipeLuaSharedElements = SlipeLua.Shared.Elements
local SystemNumerics = System.Numerics
local SlipeLuaClientGui
System.import(function (out)
  SlipeLuaClientGui = SlipeLua.Client.Gui
end)
System.namespace("SlipeLua.Client.Gui", function (namespace)
  namespace.class("GuiElement", function (namespace)
    local getVisible, setVisible, getAlpha, setAlpha, getEnabled, setEnabled, getStandardFont, setStandardFont, 
    getCustomFont, setCustomFont, getPosition, setPosition, getRelativePosition, setRelativePosition, getSize, setSize, 
    getRelativeSize, setRelativeSize, getContent, setContent, BringToFront, MoveToBack, Blur, Focus, 
    SetProperty, GetProperty, addOnBlur, removeOnBlur, addOnFocus, removeOnFocus, addOnClick, removeOnClick, 
    addOnDoubleClick, removeOnDoubleClick, addOnMouseDown, removeOnMouseDown, addOnMouseUp, removeOnMouseUp, addOnMove, removeOnMove, 
    addOnResize, removeOnResize, addOnMouseEnter, removeOnMouseEnter, addOnMouseLeave, removeOnMouseLeave, addOnMouseMove, removeOnMouseMove, 
    addOnMouseWheel, removeOnMouseWheel, __ctor__
    __ctor__ = function (this, element)
      SlipeLuaSharedElements.Element.__ctor__[2](this, element)
    end
    getVisible = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GuiGetVisible(this.element)
    end
    setVisible = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.GuiSetVisible(this.element, value)
    end
    getAlpha = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GuiGetAlpha(this.element)
    end
    setAlpha = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.GuiSetAlpha(this.element, value)
    end
    getEnabled = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GuiGetEnabled(this.element)
    end
    setEnabled = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.GuiSetEnabled(this.element, value)
    end
    getStandardFont = function (this)
      local s, _ = SlipeLuaMtaDefinitions.MtaClient.GuiGetFont(this.element):Deconstruct()
      return System.cast(System.Int32, System.Enum.Parse(System.typeof(SlipeLuaClientGui.StandardGuiFont), s:Replace("-", "_"), true))
    end
    setStandardFont = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.GuiSetFont(this.element, value:EnumToString(SlipeLuaClientGui.StandardGuiFont):ToLower():Replace("_", "-"))
    end
    getCustomFont = function (this)
      local _, e = SlipeLuaMtaDefinitions.MtaClient.GuiGetFont(this.element):Deconstruct()
      return SlipeLuaSharedElements.ElementManager.getInstance():GetElement(e, SlipeLuaClientGui.GuiFont)
    end
    setCustomFont = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.GuiSetFont(this.element, value:getMTAElement())
    end
    getPosition = function (this)
      local x, y = SlipeLuaMtaDefinitions.MtaClient.GuiGetPosition(this.element, false):Deconstruct()
      return SystemNumerics.Vector2(x, y)
    end
    setPosition = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.GuiSetPosition(this.element, value.X, value.Y, false)
    end
    getRelativePosition = function (this)
      local x, y = SlipeLuaMtaDefinitions.MtaClient.GuiGetPosition(this.element, true):Deconstruct()
      return SystemNumerics.Vector2(x, y)
    end
    setRelativePosition = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.GuiSetPosition(this.element, value.X, value.Y, true)
    end
    getSize = function (this)
      local x, y = SlipeLuaMtaDefinitions.MtaClient.GuiGetSize(this.element, false):Deconstruct()
      return SystemNumerics.Vector2(x, y)
    end
    setSize = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.GuiSetSize(this.element, value.X, value.Y, false)
    end
    getRelativeSize = function (this)
      local x, y = SlipeLuaMtaDefinitions.MtaClient.GuiGetSize(this.element, true):Deconstruct()
      return SystemNumerics.Vector2(x, y)
    end
    setRelativeSize = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.GuiSetSize(this.element, value.X, value.Y, true)
    end
    getContent = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GuiGetText(this.element)
    end
    setContent = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.GuiSetText(this.element, value)
    end
    BringToFront = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GuiBringToFront(this.element)
    end
    MoveToBack = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GuiMoveToBack(this.element)
    end
    Blur = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GuiBlur(this.element)
    end
    Focus = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GuiFocus(this.element)
    end
    SetProperty = function (this, property, value)
      return SlipeLuaMtaDefinitions.MtaClient.GuiSetProperty(this.element, property, value)
    end
    GetProperty = function (this, property)
      return SlipeLuaMtaDefinitions.MtaClient.GuiGetProperty(this.element, property)
    end
    addOnBlur, removeOnBlur = System.event("OnBlur")
    addOnFocus, removeOnFocus = System.event("OnFocus")
    addOnClick, removeOnClick = System.event("OnClick")
    addOnDoubleClick, removeOnDoubleClick = System.event("OnDoubleClick")
    addOnMouseDown, removeOnMouseDown = System.event("OnMouseDown")
    addOnMouseUp, removeOnMouseUp = System.event("OnMouseUp")
    addOnMove, removeOnMove = System.event("OnMove")
    addOnResize, removeOnResize = System.event("OnResize")
    addOnMouseEnter, removeOnMouseEnter = System.event("OnMouseEnter")
    addOnMouseLeave, removeOnMouseLeave = System.event("OnMouseLeave")
    addOnMouseMove, removeOnMouseMove = System.event("OnMouseMove")
    addOnMouseWheel, removeOnMouseWheel = System.event("OnMouseWheel")
    return {
      base = function (out)
        return {
          out.SlipeLua.Shared.Elements.Element
        }
      end,
      getVisible = getVisible,
      setVisible = setVisible,
      getAlpha = getAlpha,
      setAlpha = setAlpha,
      getEnabled = getEnabled,
      setEnabled = setEnabled,
      getStandardFont = getStandardFont,
      setStandardFont = setStandardFont,
      getCustomFont = getCustomFont,
      setCustomFont = setCustomFont,
      getPosition = getPosition,
      setPosition = setPosition,
      getRelativePosition = getRelativePosition,
      setRelativePosition = setRelativePosition,
      getSize = getSize,
      setSize = setSize,
      getRelativeSize = getRelativeSize,
      setRelativeSize = setRelativeSize,
      getContent = getContent,
      setContent = setContent,
      BringToFront = BringToFront,
      MoveToBack = MoveToBack,
      Blur = Blur,
      Focus = Focus,
      SetProperty = SetProperty,
      GetProperty = GetProperty,
      addOnBlur = addOnBlur,
      removeOnBlur = removeOnBlur,
      addOnFocus = addOnFocus,
      removeOnFocus = removeOnFocus,
      addOnClick = addOnClick,
      removeOnClick = removeOnClick,
      addOnDoubleClick = addOnDoubleClick,
      removeOnDoubleClick = removeOnDoubleClick,
      addOnMouseDown = addOnMouseDown,
      removeOnMouseDown = removeOnMouseDown,
      addOnMouseUp = addOnMouseUp,
      removeOnMouseUp = removeOnMouseUp,
      addOnMove = addOnMove,
      removeOnMove = removeOnMove,
      addOnResize = addOnResize,
      removeOnResize = removeOnResize,
      addOnMouseEnter = addOnMouseEnter,
      removeOnMouseEnter = removeOnMouseEnter,
      addOnMouseLeave = addOnMouseLeave,
      removeOnMouseLeave = removeOnMouseLeave,
      addOnMouseMove = addOnMouseMove,
      removeOnMouseMove = removeOnMouseMove,
      addOnMouseWheel = addOnMouseWheel,
      removeOnMouseWheel = removeOnMouseWheel,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          properties = {
            { "Alpha", 0x106, System.Single, getAlpha, setAlpha },
            { "Content", 0x106, System.String, getContent, setContent },
            { "CustomFont", 0x106, out.SlipeLua.Client.Gui.GuiFont, getCustomFont, setCustomFont },
            { "Enabled", 0x106, System.Boolean, getEnabled, setEnabled },
            { "Position", 0x106, System.Numerics.Vector2, getPosition, setPosition },
            { "RelativePosition", 0x106, System.Numerics.Vector2, getRelativePosition, setRelativePosition },
            { "RelativeSize", 0x106, System.Numerics.Vector2, getRelativeSize, setRelativeSize },
            { "Size", 0x106, System.Numerics.Vector2, getSize, setSize },
            { "StandardFont", 0x106, System.Int32, getStandardFont, setStandardFont },
            { "Visible", 0x106, System.Boolean, getVisible, setVisible }
          },
          methods = {
            { ".ctor", 0x106, nil, out.SlipeLua.MtaDefinitions.MtaElement },
            { "Blur", 0x86, Blur, System.Boolean },
            { "BringToFront", 0x86, BringToFront, System.Boolean },
            { "Focus", 0x86, Focus, System.Boolean },
            { "GetProperty", 0x186, GetProperty, System.String, System.String },
            { "MoveToBack", 0x86, MoveToBack, System.Boolean },
            { "SetProperty", 0x286, SetProperty, System.String, System.String, System.Boolean }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
