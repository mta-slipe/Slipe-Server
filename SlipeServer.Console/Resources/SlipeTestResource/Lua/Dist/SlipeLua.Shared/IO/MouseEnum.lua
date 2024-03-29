-- Generated by CSharp.lua Compiler
local System = System
System.namespace("SlipeLua.Shared.IO", function (namespace)
  --/ <summary>
  --/ Represents different mouse buttons
  --/ </summary>
  namespace.enum("MouseButton", function ()
    return {
      Left = 0,
      Middle = 1,
      Right = 2,
      __metadata__ = function (out)
        return {
          fields = {
            { "Left", 0xE, System.Int32 },
            { "Middle", 0xE, System.Int32 },
            { "Right", 0xE, System.Int32 }
          },
          class = { 0x6 }
        }
      end
    }
  end)

  --/ <summary>
  --/ Represents different mouse button states
  --/ </summary>
  namespace.enum("MouseButtonState", function ()
    return {
      Up = 0,
      Down = 1,
      __metadata__ = function (out)
        return {
          fields = {
            { "Down", 0xE, System.Int32 },
            { "Up", 0xE, System.Int32 }
          },
          class = { 0x6 }
        }
      end
    }
  end)

  --/ <summary>
  --/ Represents the scroll direction of the mouse wheel
  --/ </summary>
  namespace.enum("MouseWheelState", function ()
    return {
      Down = -1,
      Up = 1,
      __metadata__ = function (out)
        return {
          fields = {
            { "Down", 0xE, System.Int32 },
            { "Up", 0xE, System.Int32 }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
