-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
local SlipeLuaClientBrowsers
local SlipeLuaClientGui
System.import(function (out)
  SlipeLuaClientBrowsers = SlipeLua.Client.Browsers
  SlipeLuaClientGui = SlipeLua.Client.Gui
end)
System.namespace("SlipeLua.Client.Gui", function (namespace)
  --/ <summary>
  --/ GUI variant of a browser element
  --/ </summary>
  namespace.class("GuiBrowser", function (namespace)
    local getBrowser, __ctor1__, __ctor2__
    __ctor1__ = function (this, element)
      SlipeLuaClientGui.GuiElement.__ctor__(this, element)
    end
    __ctor2__ = function (this, position, width, height, isLocal, isTransparent, relative, parent)
      local default = parent
      if default ~= nil then
        default = default:getMTAElement()
      end
      __ctor1__(this, SlipeLuaMtaDefinitions.MtaClient.GuiCreateBrowser(position.X, position.Y, width, height, isLocal, isTransparent, relative, default))
      this.browser = SlipeLuaClientBrowsers.Browser(SlipeLuaMtaDefinitions.MtaClient.GuiGetBrowser(this.element))
    end
    getBrowser = function (this)
      return this.browser
    end
    return {
      base = function (out)
        return {
          out.SlipeLua.Client.Gui.GuiElement
        }
      end,
      getBrowser = getBrowser,
      __ctor__ = {
        __ctor1__,
        __ctor2__
      },
      __metadata__ = function (out)
        return {
          fields = {
            { "browser", 0x1, out.SlipeLua.Client.Browsers.Browser }
          },
          properties = {
            { "Browser", 0x206, out.SlipeLua.Client.Browsers.Browser, getBrowser }
          },
          methods = {
            { ".ctor", 0x106, __ctor1__, out.SlipeLua.MtaDefinitions.MtaElement },
            { ".ctor", 0x706, __ctor2__, System.Numerics.Vector2, System.Single, System.Single, System.Boolean, System.Boolean, System.Boolean, out.SlipeLua.Client.Gui.GuiElement }
          },
          class = { 0x6, System.new(out.SlipeLua.Shared.Elements.DefaultElementClassAttribute, 2, 13 --[[ElementType.GuiBrowser]]) }
        }
      end
    }
  end)
end)
