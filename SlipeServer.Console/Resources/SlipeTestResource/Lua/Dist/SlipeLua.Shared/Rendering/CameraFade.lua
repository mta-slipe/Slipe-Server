-- Generated by CSharp.lua Compiler
local System = System
System.namespace("SlipeLua.Shared.Rendering", function (namespace)
  --/ <summary>
  --/ Represents camera fades
  --/ </summary>
  namespace.enum("CameraFade", function ()
    return {
      In = 0,
      Out = 1,
      __metadata__ = function (out)
        return {
          fields = {
            { "In", 0xE, System.Int32 },
            { "Out", 0xE, System.Int32 }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)