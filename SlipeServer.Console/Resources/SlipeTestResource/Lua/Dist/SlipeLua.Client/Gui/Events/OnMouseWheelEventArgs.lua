-- Generated by CSharp.lua Compiler
local System = System
System.namespace("SlipeLua.Client.Gui.Events", function (namespace)
  namespace.class("OnMouseWheelEventArgs", function (namespace)
    local getState, __ctor__
    __ctor__ = function (this, state)
      this.State = System.cast(System.Int32, state)
    end
    getState = System.property("State", true)
    return {
      State = 0,
      getState = getState,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          properties = {
            { "State", 0x206, System.Int32, getState }
          },
          methods = {
            { ".ctor", 0x104, nil, System.Object }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
