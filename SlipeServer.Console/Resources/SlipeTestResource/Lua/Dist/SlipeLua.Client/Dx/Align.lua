-- Generated by CSharp.lua Compiler
local System = System
System.namespace("SlipeLua.Client.Dx", function (namespace)
  --/ <summary>
  --/ Represents a horizontal alignment
  --/ </summary>
  namespace.enum("HorizontalAlign", function ()
    return {
      Left = 0,
      Center = 1,
      Right = 2,
      __metadata__ = function (out)
        return {
          fields = {
            { "Center", 0xE, System.Int32 },
            { "Left", 0xE, System.Int32 },
            { "Right", 0xE, System.Int32 }
          },
          class = { 0x6 }
        }
      end
    }
  end)

  --/ <summary>
  --/ Represents a vertical alignment
  --/ </summary>
  namespace.enum("VerticalAlign", function ()
    return {
      Top = 0,
      Center = 1,
      Bottom = 2,
      __metadata__ = function (out)
        return {
          fields = {
            { "Bottom", 0xE, System.Int32 },
            { "Center", 0xE, System.Int32 },
            { "Top", 0xE, System.Int32 }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
