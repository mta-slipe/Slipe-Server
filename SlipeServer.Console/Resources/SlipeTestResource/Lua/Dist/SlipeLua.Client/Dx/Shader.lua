-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
local SlipeLuaSharedElements = SlipeLua.Shared.Elements
local SystemNumerics = System.Numerics
local SlipeLuaClientDx
System.import(function (out)
  SlipeLuaClientDx = SlipeLua.Client.Dx
end)
System.namespace("SlipeLua.Client.Dx", function (namespace)
  --/ <summary>
  --/ A shader is a graphical 
  --/ </summary>
  namespace.class("Shader", function (namespace)
    local getTechniqueName, SetValue, SetValue1, SetTessellation, SetTransform, SetTransform1, SetTransform2, Apply, 
    Apply1, Remove, Remove1, __ctor__
    __ctor__ = function (this, filePathOrRaw, priority, maxDistance, layered, shaderElementType)
      SlipeLuaClientDx.Material.__ctor__(this)
      local result = SlipeLuaMtaDefinitions.MtaClient.DxCreateShader(filePathOrRaw, priority, maxDistance, layered, shaderElementType:EnumToString(SlipeLuaClientDx.ShaderElementType):ToLower())
      this.materialElement = result[1]
      this.TechniqueName = result[2]
    end
    getTechniqueName = System.property("TechniqueName", true)
    SetValue = function (this, parameterName, value)
      return SlipeLuaMtaDefinitions.MtaClient.DxSetShaderValue(this.materialElement, parameterName, value)
    end
    SetValue1 = function (this, parameterName, values)
      do
       return SlipeMtaDefinitions.MtaClient.DxSetShaderValue(this.materialElement, parameterName, unpack(values))
      end
      return SlipeLuaMtaDefinitions.MtaClient.DxSetShaderValue(this.materialElement, parameterName, values)
    end
    SetTessellation = function (this, tesselation)
      return SlipeLuaMtaDefinitions.MtaClient.DxSetShaderTessellation(this.materialElement, System.ToInt32(tesselation.X), System.ToInt32(tesselation.Y))
    end
    SetTransform = function (this, rotation, rotationCenterOffset, perspectiveCenterOffset, rotationCenterOffsetOriginIsScreen, perspectiveCenterOffsetOriginIsScren)
      return SlipeLuaMtaDefinitions.MtaClient.DxSetShaderTransform(this.materialElement, rotation.X, rotation.Y, rotation.Z, rotationCenterOffset.X, rotationCenterOffset.Y, rotationCenterOffset.Z, rotationCenterOffsetOriginIsScreen, perspectiveCenterOffset.X, perspectiveCenterOffset.Y, perspectiveCenterOffsetOriginIsScren)
    end
    SetTransform1 = function (this, rotation, rotationCenterOffset, rotationCenterOffsetOriginIsScreen)
      return SetTransform(this, rotation:__clone__(), rotationCenterOffset:__clone__(), SystemNumerics.Vector2.getZero(), rotationCenterOffsetOriginIsScreen, false)
    end
    SetTransform2 = function (this, rotation)
      return SetTransform1(this, rotation:__clone__(), SystemNumerics.Vector3.getZero(), false)
    end
    Apply = function (this, textureName, targetElement, appendLayers)
      return SlipeLuaMtaDefinitions.MtaClient.EngineApplyShaderToWorldTexture(this.materialElement, textureName, targetElement:getMTAElement(), appendLayers)
    end
    Apply1 = function (this, textureName, appendLayers)
      return SlipeLuaMtaDefinitions.MtaClient.EngineApplyShaderToWorldTexture(this.materialElement, textureName, nil, appendLayers)
    end
    Remove = function (this, textureName, targetElement)
      return SlipeLuaMtaDefinitions.MtaClient.EngineRemoveShaderFromWorldTexture(this.materialElement, textureName, targetElement:getMTAElement())
    end
    Remove1 = function (this, textureName)
      return Remove(this, textureName, SlipeLuaSharedElements.Element.getRoot())
    end
    return {
      base = function (out)
        return {
          out.SlipeLua.Client.Dx.Material
        }
      end,
      getTechniqueName = getTechniqueName,
      SetValue = SetValue,
      SetValue1 = SetValue1,
      SetTessellation = SetTessellation,
      SetTransform = SetTransform,
      SetTransform1 = SetTransform1,
      SetTransform2 = SetTransform2,
      Apply = Apply,
      Apply1 = Apply1,
      Remove = Remove,
      Remove1 = Remove1,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          properties = {
            { "TechniqueName", 0x206, System.String, getTechniqueName }
          },
          methods = {
            { ".ctor", 0x506, nil, System.String, System.Single, System.Single, System.Boolean, System.Int32 },
            { "Apply", 0x386, Apply, System.String, out.SlipeLua.Shared.Elements.Element, System.Boolean, System.Boolean },
            { "Apply", 0x286, Apply1, System.String, System.Boolean, System.Boolean },
            { "Remove", 0x286, Remove, System.String, out.SlipeLua.Shared.Elements.Element, System.Boolean },
            { "Remove", 0x186, Remove1, System.String, System.Boolean },
            { "SetTessellation", 0x186, SetTessellation, System.Numerics.Vector2, System.Boolean },
            { "SetTransform", 0x586, SetTransform, System.Numerics.Vector3, System.Numerics.Vector3, System.Numerics.Vector2, System.Boolean, System.Boolean, System.Boolean },
            { "SetTransform", 0x386, SetTransform1, System.Numerics.Vector3, System.Numerics.Vector3, System.Boolean, System.Boolean },
            { "SetTransform", 0x186, SetTransform2, System.Numerics.Vector3, System.Boolean },
            { "SetValue", 0x286, SetValue, System.String, System.Object, System.Boolean },
            { "SetValue", 0x286, SetValue1, System.String, System.Array(System.Object), System.Boolean }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
