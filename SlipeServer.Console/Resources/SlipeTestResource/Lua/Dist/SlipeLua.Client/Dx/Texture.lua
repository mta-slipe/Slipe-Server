-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
local SlipeLuaSharedUtilities = SlipeLua.Shared.Utilities
local SystemNumerics = System.Numerics
local SlipeLuaClientDx
System.import(function (out)
  SlipeLuaClientDx = SlipeLua.Client.Dx
end)
System.namespace("SlipeLua.Client.Dx", function (namespace)
  --/ <summary>
  --/ Drawable texture
  --/ </summary>
  namespace.class("Texture", function (namespace)
    local GetPixels, GetPixels1, SetEdge, SetEdge1, SetPixels, __ctor1__, __ctor2__, __ctor3__
    __ctor1__ = function (this)
      SlipeLuaClientDx.Material.__ctor__(this)
    end
    __ctor2__ = function (this, filePathOrPixels, textureFormat, mipmaps, textureEdge)
      SlipeLuaClientDx.Material.__ctor__(this)
      this.materialElement = SlipeLuaMtaDefinitions.MtaClient.DxCreateTexture(filePathOrPixels, textureFormat:EnumToString(SlipeLuaClientDx.TextureFormat):ToLower(), mipmaps, textureEdge:EnumToString(SlipeLuaClientDx.TextureEdge):ToLower())
    end
    __ctor3__ = function (this, dimensions, textureFormat, textureEdge, textureType, depth)
      SlipeLuaClientDx.Material.__ctor__(this)
      local textureTypeString = "cube"
      if textureType == 0 --[[TextureType.TwoDimensional]] then
        textureTypeString = "2d"
      elseif textureType == 1 --[[TextureType.ThreeDimensional]] then
        textureTypeString = "3d"
      end

      this.materialElement = SlipeLuaMtaDefinitions.MtaClient.DxCreateTexture(System.ToInt32(dimensions.X), System.ToInt32(dimensions.Y), textureFormat:EnumToString(SlipeLuaClientDx.TextureFormat):ToLower(), textureEdge:EnumToString(SlipeLuaClientDx.TextureEdge):ToLower(), textureTypeString, depth)
    end
    GetPixels = function (this, topLeft, dimensions, surfaceIndex)
      return SlipeLuaClientDx.TexturePixels(SlipeLuaMtaDefinitions.MtaClient.DxGetTexturePixels(surfaceIndex, this.materialElement, System.ToInt32(topLeft.X), System.ToInt32(topLeft.Y), System.ToInt32(dimensions.X), System.ToInt32(dimensions.Y)))
    end
    GetPixels1 = function (this, surfaceIndex)
      return GetPixels(this, SystemNumerics.Vector2.getZero(), SystemNumerics.Vector2.getZero(), surfaceIndex)
    end
    SetEdge = function (this, edge, borderColor)
      return SlipeLuaMtaDefinitions.MtaClient.DxSetTextureEdge(this.materialElement, (edge == 4 --[[TextureEdge.MirrorOnce]]) and "mirror-once" or edge:EnumToString(SlipeLuaClientDx.TextureEdge), borderColor:getHex())
    end
    SetEdge1 = function (this, edge)
      return SetEdge(this, edge, SlipeLuaSharedUtilities.Color.getWhite())
    end
    SetPixels = function (this, pixels, topLeft, dimensions, surfaceIndex)
      local p = pixels:Convert(0 --[[ImageFormat.plain]], 80)
      return SlipeLuaMtaDefinitions.MtaClient.DxSetTexturePixels(surfaceIndex, this.materialElement, p, System.ToInt32(topLeft.X), System.ToInt32(topLeft.Y), System.ToInt32(dimensions.X), System.ToInt32(dimensions.Y))
    end
    return {
      base = function (out)
        return {
          out.SlipeLua.Client.Dx.Material
        }
      end,
      GetPixels = GetPixels,
      GetPixels1 = GetPixels1,
      SetEdge = SetEdge,
      SetEdge1 = SetEdge1,
      SetPixels = SetPixels,
      __ctor__ = {
        __ctor1__,
        __ctor2__,
        __ctor3__
      },
      __metadata__ = function (out)
        return {
          methods = {
            { ".ctor", 0x3, __ctor1__ },
            { ".ctor", 0x406, __ctor2__, System.String, System.Int32, System.Boolean, System.Int32 },
            { ".ctor", 0x506, __ctor3__, System.Numerics.Vector2, System.Int32, System.Int32, System.Int32, System.Int32 },
            { "GetPixels", 0x386, GetPixels, System.Numerics.Vector2, System.Numerics.Vector2, System.Int32, out.SlipeLua.Client.Dx.TexturePixels },
            { "GetPixels", 0x186, GetPixels1, System.Int32, out.SlipeLua.Client.Dx.TexturePixels },
            { "SetEdge", 0x286, SetEdge, System.Int32, out.SlipeLua.Shared.Utilities.Color, System.Boolean },
            { "SetEdge", 0x186, SetEdge1, System.Int32, System.Boolean },
            { "SetPixels", 0x486, SetPixels, out.SlipeLua.Client.Dx.TexturePixels, System.Numerics.Vector2, System.Numerics.Vector2, System.Int32, System.Boolean }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
