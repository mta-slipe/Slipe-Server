-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
System.namespace("SlipeLua.Shared.IO", function (namespace)
  namespace.class("SharedMtaDebug", function (namespace)
    local WriteLine, WriteLine1, WriteLine2
    WriteLine = function (this, line, level)
      SlipeLuaMtaDefinitions.MtaShared.OutputDebugString(line, level, 255, 255, 255)
    end
    WriteLine1 = function (this, line, color)
      SlipeLuaMtaDefinitions.MtaShared.OutputDebugString(line, 0 --[[(int)DebugMessageLevel.Custom]], color:getR(), color:getG(), color:getB())
    end
    WriteLine2 = function (this, line, level, color)
      SlipeLuaMtaDefinitions.MtaShared.OutputDebugString(line, level, color:getR(), color:getG(), color:getB())
    end
    return {
      WriteLine = WriteLine,
      WriteLine1 = WriteLine1,
      WriteLine2 = WriteLine2,
      __metadata__ = function (out)
        return {
          methods = {
            { ".ctor", 0x5, nil },
            { "WriteLine", 0x206, WriteLine, System.String, System.Int32 },
            { "WriteLine", 0x206, WriteLine1, System.String, out.SlipeLua.Shared.Utilities.Color },
            { "WriteLine", 0x306, WriteLine2, System.String, System.Int32, out.SlipeLua.Shared.Utilities.Color }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
