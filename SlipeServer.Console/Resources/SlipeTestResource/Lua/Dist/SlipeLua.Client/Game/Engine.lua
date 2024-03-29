-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
System.namespace("SlipeLua.Client.Game", function (namespace)
  namespace.class("Engine", function (namespace)
    local getInstance, GetModelIDFromName, GetModelNameFromId, GetModelLODDistance, SetModelLODDistance, GetModelTextureNames, GetModelTextureNames1, GetVisibleTextureNames, 
    SetAsynchronousLoading, ReplaceAnimation, RestoreAnimation, class
    getInstance = function ()
      class.instance = class.instance or class()
      return class.instance
    end
    GetModelIDFromName = function (this, name)
      return SlipeLuaMtaDefinitions.MtaClient.EngineGetModelIDFromName(name)
    end
    GetModelNameFromId = function (this, id)
      return SlipeLuaMtaDefinitions.MtaClient.EngineGetModelNameFromID(id)
    end
    GetModelLODDistance = function (this, model)
      return SlipeLuaMtaDefinitions.MtaClient.EngineGetModelLODDistance(model)
    end
    SetModelLODDistance = function (this, model, distance)
      return SlipeLuaMtaDefinitions.MtaClient.EngineSetModelLODDistance(model, distance)
    end
    GetModelTextureNames = function (this, model)
      local table = SlipeLuaMtaDefinitions.MtaClient.EngineGetModelTextureNames(model:ToString())

      return SlipeLuaMtaDefinitions.MtaShared.GetArrayFromTable(table, "System.String", T)
    end
    GetModelTextureNames1 = function (this, model)
      local table = SlipeLuaMtaDefinitions.MtaClient.EngineGetModelTextureNames(model)

      return SlipeLuaMtaDefinitions.MtaShared.GetArrayFromTable(table, "System.String", T)
    end
    GetVisibleTextureNames = function (this, nameFilter, modelId)
      local table = SlipeLuaMtaDefinitions.MtaClient.EngineGetVisibleTextureNames(nameFilter, modelId)
      return SlipeLuaMtaDefinitions.MtaShared.GetArrayFromTable(table, "System.String", T)
    end
    SetAsynchronousLoading = function (this, value, forced)
      return SlipeLuaMtaDefinitions.MtaClient.EngineSetAsynchronousLoading(value, forced)
    end
    ReplaceAnimation = function (this, ped, internalBlock, internalAnim, customBlock, customAnim)
      return SlipeLuaMtaDefinitions.MtaClient.EngineReplaceAnimation(ped:getMTAElement(), internalBlock, internalAnim, customBlock, customAnim)
    end
    RestoreAnimation = function (this, ped, internalBlock, internalAnim)
      return SlipeLuaMtaDefinitions.MtaClient.EngineRestoreAnimation(ped:getMTAElement(), internalBlock, internalAnim)
    end
    class = {
      getInstance = getInstance,
      GetModelIDFromName = GetModelIDFromName,
      GetModelNameFromId = GetModelNameFromId,
      GetModelLODDistance = GetModelLODDistance,
      SetModelLODDistance = SetModelLODDistance,
      GetModelTextureNames = GetModelTextureNames,
      GetModelTextureNames1 = GetModelTextureNames1,
      GetVisibleTextureNames = GetVisibleTextureNames,
      SetAsynchronousLoading = SetAsynchronousLoading,
      ReplaceAnimation = ReplaceAnimation,
      RestoreAnimation = RestoreAnimation,
      __metadata__ = function (out)
        return {
          fields = {
            { "instance", 0xB, class }
          },
          properties = {
            { "Instance", 0x20E, class, getInstance }
          },
          methods = {
            { "GetModelIDFromName", 0x186, GetModelIDFromName, System.String, System.Int32 },
            { "GetModelLODDistance", 0x186, GetModelLODDistance, System.Int32, System.Single },
            { "GetModelNameFromId", 0x186, GetModelNameFromId, System.Int32, System.String },
            { "GetModelTextureNames", 0x186, GetModelTextureNames, System.Int32, System.Array(System.String) },
            { "GetModelTextureNames", 0x186, GetModelTextureNames1, System.String, System.Array(System.String) },
            { "GetVisibleTextureNames", 0x286, GetVisibleTextureNames, System.String, System.String, System.Array(System.String) },
            { "ReplaceAnimation", 0x586, ReplaceAnimation, out.SlipeLua.Client.Peds.Ped, System.String, System.String, System.String, System.String, System.Boolean },
            { "RestoreAnimation", 0x386, RestoreAnimation, out.SlipeLua.Client.Peds.Ped, System.String, System.String, System.Boolean },
            { "SetAsynchronousLoading", 0x286, SetAsynchronousLoading, System.Boolean, System.Boolean, System.Boolean },
            { "SetModelLODDistance", 0x286, SetModelLODDistance, System.Int32, System.Single, System.Boolean }
          },
          class = { 0x6 }
        }
      end
    }
    return class
  end)
end)
