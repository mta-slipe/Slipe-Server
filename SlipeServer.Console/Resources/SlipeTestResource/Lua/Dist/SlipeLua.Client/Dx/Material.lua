-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
local SystemNumerics = System.Numerics
System.namespace("SlipeLua.Client.Dx", function (namespace)
  --/ <summary>
  --/ Class representing a texture or shader
  --/ </summary>
  namespace.class("Material", function (namespace)
    local getMaterialElement, getMaterialSize, getVolumeSize, Destroy
    getMaterialElement = function (this)
      return this.materialElement
    end
    getMaterialSize = function (this)
      local result = SlipeLuaMtaDefinitions.MtaClient.DxGetMaterialSize(this.materialElement)
      return SystemNumerics.Vector2(result[1], result[2])
    end
    getVolumeSize = function (this)
      local result = SlipeLuaMtaDefinitions.MtaClient.DxGetMaterialSize(this.materialElement)
      return SystemNumerics.Vector3(result[1], result[2], result[3])
    end
    Destroy = function (this)
      SlipeLuaMtaDefinitions.MtaShared.DestroyElement(this.materialElement)
    end
    return {
      getMaterialElement = getMaterialElement,
      getMaterialSize = getMaterialSize,
      getVolumeSize = getVolumeSize,
      Destroy = Destroy,
      __metadata__ = function (out)
        return {
          fields = {
            { "materialElement", 0x3, out.SlipeLua.MtaDefinitions.MtaElement }
          },
          properties = {
            { "MaterialElement", 0x206, out.SlipeLua.MtaDefinitions.MtaElement, getMaterialElement },
            { "MaterialSize", 0x206, System.Numerics.Vector2, getMaterialSize },
            { "VolumeSize", 0x206, System.Numerics.Vector3, getVolumeSize }
          },
          methods = {
            { "Destroy", 0x6, Destroy }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
