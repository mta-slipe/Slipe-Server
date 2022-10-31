-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
System.namespace("SlipeLua.Client.IO", function (namespace)
  namespace.class("MtaConsole", function (namespace)
    local getActive, WriteLine
    getActive = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.IsConsoleActive()
    end
    WriteLine = function (this, line)
      SlipeLuaMtaDefinitions.MtaClient.OutputConsole(line)
    end
    return {
      getActive = getActive,
      WriteLine = WriteLine,
      __metadata__ = function (out)
        return {
          methods = {
            { ".ctor", 0x4, nil },
            { "WriteLine", 0x106, WriteLine, System.String }
          },
          properties = {
            { "Active", 0x206, System.Boolean, getActive }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)