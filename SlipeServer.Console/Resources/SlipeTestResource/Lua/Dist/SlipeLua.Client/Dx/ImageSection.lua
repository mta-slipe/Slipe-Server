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
  --/ Differing from DxImage, this clas only represnts a part of an image.
  --/ </summary>
  namespace.class("ImageSection", function (namespace)
    local getSectionTopLeft, setSectionTopLeft, getSectionDimensions, setSectionDimensions, Draw, internal, __ctor1__, __ctor2__, 
    __ctor3__, __ctor4__, __ctor5__, __ctor6__
    internal = function (this)
      this.SectionTopLeft = System.default(SystemNumerics.Vector2)
      this.SectionDimensions = System.default(SystemNumerics.Vector2)
    end
    __ctor1__ = function (this, position, dimensions, sectionUV, UVDimensions, filePath, rotation, rotationCenter, color, postGUI)
      internal(this)
      SlipeLuaClientDx.Image.__ctor__[1](this, position:__clone__(), dimensions:__clone__(), filePath, rotation, rotationCenter:__clone__(), color, postGUI)
      this.SectionTopLeft = sectionUV:__clone__()
      this.SectionDimensions = UVDimensions:__clone__()
    end
    __ctor2__ = function (this, position, dimensions, sectionUV, UVDimensions, filePath, rotation, rotationCenter)
      __ctor1__(this, position:__clone__(), dimensions:__clone__(), sectionUV:__clone__(), UVDimensions:__clone__(), filePath, rotation, rotationCenter:__clone__(), SlipeLuaSharedUtilities.Color.getWhite(), false)
    end
    __ctor3__ = function (this, position, dimensions, sectionUV, UVDimensions, filePath, rotation)
      __ctor2__(this, position:__clone__(), dimensions:__clone__(), sectionUV:__clone__(), UVDimensions:__clone__(), filePath, rotation, SystemNumerics.Vector2.getZero())
    end
    __ctor4__ = function (this, position, dimensions, sectionUV, UVDimensions, material, rotation, rotationCenter, color, postGUI)
      internal(this)
      SlipeLuaClientDx.Image.__ctor__[4](this, position:__clone__(), dimensions:__clone__(), material, rotation, rotationCenter:__clone__(), color, postGUI)
      this.SectionTopLeft = sectionUV:__clone__()
      this.SectionDimensions = UVDimensions:__clone__()
    end
    __ctor5__ = function (this, position, dimensions, sectionUV, UVDimensions, material, rotation, rotationCenter)
      __ctor4__(this, position:__clone__(), dimensions:__clone__(), sectionUV:__clone__(), UVDimensions:__clone__(), material, rotation, rotationCenter:__clone__(), SlipeLuaSharedUtilities.Color.getWhite(), false)
    end
    __ctor6__ = function (this, position, dimensions, sectionUV, UVDimensions, material, rotation)
      __ctor5__(this, position:__clone__(), dimensions:__clone__(), sectionUV:__clone__(), UVDimensions:__clone__(), material, rotation, SystemNumerics.Vector2.getZero())
    end
    getSectionTopLeft, setSectionTopLeft = System.property("SectionTopLeft")
    getSectionDimensions, setSectionDimensions = System.property("SectionDimensions")
    Draw = function (this, source, eventArgs)
      if this.usePath then
        return SlipeLuaMtaDefinitions.MtaClient.DxDrawImageSection(this:getPosition().X, this:getPosition().Y, this.Dimensions:__clone__().X, this.Dimensions:__clone__().Y, this.SectionTopLeft:__clone__().X, this.SectionTopLeft:__clone__().Y, this.SectionDimensions:__clone__().X, this.SectionDimensions:__clone__().Y, this:getFilePath(), this.Rotation, this.RotationCenter:__clone__().X, this.RotationCenter:__clone__().Y, this.Color:getHex(), this.PostGUI)
      else
        local default = this:getMaterial()
        if default ~= nil then
          default = default:getMaterialElement()
        end
        return SlipeLuaMtaDefinitions.MtaClient.DxDrawImageSection(this:getPosition().X, this:getPosition().Y, this.Dimensions:__clone__().X, this.Dimensions:__clone__().Y, this.SectionTopLeft:__clone__().X, this.SectionTopLeft:__clone__().Y, this.SectionDimensions:__clone__().X, this.SectionDimensions:__clone__().Y, default, this.Rotation, this.RotationCenter:__clone__().X, this.RotationCenter:__clone__().Y, this.Color:getHex(), this.PostGUI)
      end
    end
    return {
      base = function (out)
        return {
          out.SlipeLua.Client.Dx.Image
        }
      end,
      getSectionTopLeft = getSectionTopLeft,
      setSectionTopLeft = setSectionTopLeft,
      getSectionDimensions = getSectionDimensions,
      setSectionDimensions = setSectionDimensions,
      Draw = Draw,
      __ctor__ = {
        __ctor1__,
        __ctor2__,
        __ctor3__,
        __ctor4__,
        __ctor5__,
        __ctor6__
      },
      __metadata__ = function (out)
        return {
          properties = {
            { "SectionDimensions", 0x106, System.Numerics.Vector2, getSectionDimensions, setSectionDimensions },
            { "SectionTopLeft", 0x106, System.Numerics.Vector2, getSectionTopLeft, setSectionTopLeft }
          },
          methods = {
            { ".ctor", 0x906, __ctor1__, System.Numerics.Vector2, System.Numerics.Vector2, System.Numerics.Vector2, System.Numerics.Vector2, System.String, System.Single, System.Numerics.Vector2, out.SlipeLua.Shared.Utilities.Color, System.Boolean },
            { ".ctor", 0x706, __ctor2__, System.Numerics.Vector2, System.Numerics.Vector2, System.Numerics.Vector2, System.Numerics.Vector2, System.String, System.Single, System.Numerics.Vector2 },
            { ".ctor", 0x606, __ctor3__, System.Numerics.Vector2, System.Numerics.Vector2, System.Numerics.Vector2, System.Numerics.Vector2, System.String, System.Single },
            { ".ctor", 0x906, __ctor4__, System.Numerics.Vector2, System.Numerics.Vector2, System.Numerics.Vector2, System.Numerics.Vector2, out.SlipeLua.Client.Dx.Material, System.Single, System.Numerics.Vector2, out.SlipeLua.Shared.Utilities.Color, System.Boolean },
            { ".ctor", 0x706, __ctor5__, System.Numerics.Vector2, System.Numerics.Vector2, System.Numerics.Vector2, System.Numerics.Vector2, out.SlipeLua.Client.Dx.Material, System.Single, System.Numerics.Vector2 },
            { ".ctor", 0x606, __ctor6__, System.Numerics.Vector2, System.Numerics.Vector2, System.Numerics.Vector2, System.Numerics.Vector2, out.SlipeLua.Client.Dx.Material, System.Single },
            { "Draw", 0x286, Draw, out.SlipeLua.Client.Elements.RootElement, out.SlipeLua.Client.Rendering.Events.OnRenderEventArgs, System.Boolean }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
