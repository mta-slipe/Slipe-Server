-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
local SlipeLuaSharedUtilities = SlipeLua.Shared.Utilities
local SystemNumerics = System.Numerics
System.namespace("SlipeLua.Client.IO", function (namespace)
  --/ <summary>
  --/ Represents the ingame chatbox
  --/ </summary>
  namespace.class("ChatBox", function (namespace)
    local getActive, getVisible, setVisible, getInputBoxActive, getFont, getLines, getBackgroundColor, getTextColor, 
    getInputColor, getInputPrefixColor, getInputTextColor, getScale, getOffset, getPositionalAlignment, getAllignment, getWidth, 
    getTextFades, getBackgroundFades, getLineLife, getLineFadeOut, getUseCegui, getTextScale, WriteLine, WriteLine1, 
    Clear, addOnMessage, removeOnMessage, class
    getActive = function ()
      return SlipeLuaMtaDefinitions.MtaClient.IsChatBoxInputActive()
    end
    getVisible = function ()
      return SlipeLuaMtaDefinitions.MtaClient.IsChatVisible()
    end
    setVisible = function (value)
      SlipeLuaMtaDefinitions.MtaClient.ShowChat(value)
    end
    getInputBoxActive = function ()
      return SlipeLuaMtaDefinitions.MtaClient.IsChatBoxInputActive()
    end
    getFont = function ()
      return System.cast(System.Int32, SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_font"))
    end
    getLines = function ()
      return SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_lines")
    end
    getBackgroundColor = function ()
      local r = SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_color")
      return System.new(SlipeLuaSharedUtilities.Color, 3, r[1], r[2], r[3], r[4])
    end
    getTextColor = function ()
      local r = SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_text_color")
      return System.new(SlipeLuaSharedUtilities.Color, 3, r[1], r[2], r[3], r[4])
    end
    getInputColor = function ()
      local r = SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_input_color")
      return System.new(SlipeLuaSharedUtilities.Color, 3, r[1], r[2], r[3], r[4])
    end
    getInputPrefixColor = function ()
      local r = SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_input_prefix_color")
      return System.new(SlipeLuaSharedUtilities.Color, 3, r[1], r[2], r[3], r[4])
    end
    getInputTextColor = function ()
      local r = SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_input_text_color")
      return System.new(SlipeLuaSharedUtilities.Color, 3, r[1], r[2], r[3], r[4])
    end
    getScale = function ()
      local r = SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_scale")
      return SystemNumerics.Vector2(r[1], r[2])
    end
    getOffset = function ()
      local x = SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_position_offset_x")
      local y = SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_position_offset_y")
      return SystemNumerics.Vector2(x, y)
    end
    getPositionalAlignment = function ()
      local x = SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_position_horizontal")
      local y = SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_position_vertical")
      return SystemNumerics.Vector2(x, y)
    end
    getAllignment = function ()
      return SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_position_vertical")
    end
    getWidth = function ()
      return SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_width")
    end
    getTextFades = function ()
      return SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_css_style_text")
    end
    getBackgroundFades = function ()
      return SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_css_style_background")
    end
    getLineLife = function ()
      return SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_line_life")
    end
    getLineFadeOut = function ()
      return SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_line_fade_out")
    end
    getUseCegui = function ()
      return SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("chat_use_cegui")
    end
    getTextScale = function ()
      return SlipeLuaMtaDefinitions.MtaClient.GetChatboxLayout("text_scale")
    end
    WriteLine = function (message, color, colorCoded)
      SlipeLuaMtaDefinitions.MtaClient.OutputChatBox(message, color:getR(), color:getG(), color:getB(), colorCoded)
    end
    WriteLine1 = function (message)
      WriteLine(message, SlipeLuaSharedUtilities.Color.getWhite(), false)
    end
    Clear = function ()
      SlipeLuaMtaDefinitions.MtaClient.ClearChatBox()
    end
    addOnMessage = function (value)
      class.OnMessage = class.OnMessage + value
    end
    removeOnMessage = function (value)
      class.OnMessage = class.OnMessage - value
    end
    class = {
      getActive = getActive,
      getVisible = getVisible,
      setVisible = setVisible,
      getInputBoxActive = getInputBoxActive,
      getFont = getFont,
      getLines = getLines,
      getBackgroundColor = getBackgroundColor,
      getTextColor = getTextColor,
      getInputColor = getInputColor,
      getInputPrefixColor = getInputPrefixColor,
      getInputTextColor = getInputTextColor,
      getScale = getScale,
      getOffset = getOffset,
      getPositionalAlignment = getPositionalAlignment,
      getAllignment = getAllignment,
      getWidth = getWidth,
      getTextFades = getTextFades,
      getBackgroundFades = getBackgroundFades,
      getLineLife = getLineLife,
      getLineFadeOut = getLineFadeOut,
      getUseCegui = getUseCegui,
      getTextScale = getTextScale,
      WriteLine = WriteLine,
      WriteLine1 = WriteLine1,
      Clear = Clear,
      addOnMessage = addOnMessage,
      removeOnMessage = removeOnMessage,
      __metadata__ = function (out)
        return {
          properties = {
            { "Active", 0x20E, System.Boolean, getActive },
            { "Allignment", 0x20E, System.Int32, getAllignment },
            { "BackgroundColor", 0x20E, out.SlipeLua.Shared.Utilities.Color, getBackgroundColor },
            { "BackgroundFades", 0x20E, System.Boolean, getBackgroundFades },
            { "Font", 0x20E, System.Int32, getFont },
            { "InputBoxActive", 0x20E, System.Boolean, getInputBoxActive },
            { "InputColor", 0x20E, out.SlipeLua.Shared.Utilities.Color, getInputColor },
            { "InputPrefixColor", 0x20E, out.SlipeLua.Shared.Utilities.Color, getInputPrefixColor },
            { "InputTextColor", 0x20E, out.SlipeLua.Shared.Utilities.Color, getInputTextColor },
            { "LineFadeOut", 0x20E, System.Single, getLineFadeOut },
            { "LineLife", 0x20E, System.Single, getLineLife },
            { "Lines", 0x20E, System.Int32, getLines },
            { "Offset", 0x20E, System.Numerics.Vector2, getOffset },
            { "PositionalAlignment", 0x20E, System.Numerics.Vector2, getPositionalAlignment },
            { "Scale", 0x20E, System.Numerics.Vector2, getScale },
            { "TextColor", 0x20E, out.SlipeLua.Shared.Utilities.Color, getTextColor },
            { "TextFades", 0x20E, System.Boolean, getTextFades },
            { "TextScale", 0x20E, System.Single, getTextScale },
            { "UseCegui", 0x20E, System.Boolean, getUseCegui },
            { "Visible", 0x10E, System.Boolean, getVisible, setVisible },
            { "Width", 0x20E, System.Single, getWidth }
          },
          methods = {
            { "Clear", 0xE, Clear },
            { "WriteLine", 0x10E, WriteLine1, System.String },
            { "WriteLine", 0x30E, WriteLine, System.String, out.SlipeLua.Shared.Utilities.Color, System.Boolean }
          },
          class = { 0xE }
        }
      end
    }
    return class
  end)
end)
