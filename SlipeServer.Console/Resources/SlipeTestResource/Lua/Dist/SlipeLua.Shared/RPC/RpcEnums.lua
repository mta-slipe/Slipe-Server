-- Generated by CSharp.lua Compiler
local System = System
System.namespace("SlipeLua.Shared.Rpc", function (namespace)
  namespace.enum("ClientRpcFailedAction", function ()
    return {
      Ignore = 0,
      Queue = 1,
      __metadata__ = function (out)
        return {
          fields = {
            { "Ignore", 0xE, System.Int32 },
            { "Queue", 0xE, System.Int32 }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
