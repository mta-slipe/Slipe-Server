-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
local SlipeLuaClientAssets
System.import(function (out)
  SlipeLuaClientAssets = SlipeLua.Client.Assets
end)
System.namespace("SlipeLua.Client.Assets", function (namespace)
  namespace.class("Txd", function (namespace)
    local Load, ApplyTo, __ctor__
    __ctor__ = function (this, filepath)
      SlipeLuaClientAssets.Asset.__ctor__(this, filepath)
    end
    Load = function (this, filteringEnabled)
      if this.txd ~= nil then
        return
      end
      this.txd = SlipeLuaMtaDefinitions.MtaClient.EngineLoadTXD(this.filepath, filteringEnabled)
    end
    ApplyTo = function (this, model)
      if this.txd == nil then
        System.throw(System.Exception(System.String.Format("TXD file {0} has not yet been loaded", this.filepath)))
      end

      SlipeLuaMtaDefinitions.MtaClient.EngineImportTXD(this.txd, model)
    end
    return {
      base = function (out)
        return {
          out.SlipeLua.Client.Assets.Asset
        }
      end,
      Load = Load,
      ApplyTo = ApplyTo,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          fields = {
            { "txd", 0x1, out.SlipeLua.MtaDefinitions.MtaElement }
          },
          methods = {
            { ".ctor", 0x106, nil, System.String },
            { "ApplyTo", 0x106, ApplyTo, System.Int32 },
            { "Load", 0x106, Load, System.Boolean }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
