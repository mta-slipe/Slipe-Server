-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
local SlipeLuaSharedElements = SlipeLua.Shared.Elements
local SlipeLuaSharedHelpers = SlipeLua.Shared.Helpers
local SystemNumerics = System.Numerics
local SlipeLuaClientGame
System.import(function (out)
  SlipeLuaClientGame = SlipeLua.Client.Game
end)
System.namespace("SlipeLua.Client.Lights", function (namespace)
  --/ <summary>
  --/ This function creates a searchlight. A searchlight is a spotlight which looks like the one available in the Police Maverick.
  --/ </summary>
  namespace.class("SearchLight", function (namespace)
    local getOffset, setOffset, getToAttached, getIsAttached, getStartPosition, setStartPosition, getEndPosition, setEndPosition, 
    getStartRadius, setStartRadius, getEndRadius, setEndRadius, AttachTo, AttachTo1, AttachTo2, AttachTo3, 
    Detach, Update, internal, __ctor1__, __ctor2__, __ctor3__
    internal = function (this)
      this.relativeEndPosition = System.default(SystemNumerics.Vector3)
      this.Offset = System.default(SystemNumerics.Matrix4x4)
    end
    __ctor1__ = function (this, element)
      internal(this)
      SlipeLuaSharedElements.Element.__ctor__[2](this, element)
    end
    __ctor2__ = function (this, start, end_, startRadius, endRadius, renderSpot)
      __ctor1__(this, SlipeLuaMtaDefinitions.MtaClient.CreateSearchLight(start.X, start.Y, start.Z, end_.X, end_.Y, end_.Z, startRadius, endRadius, renderSpot))
      this.relativeEndPosition = SystemNumerics.Vector3.op_Subtraction(end_, start)
    end
    __ctor3__ = function (this, attachTo, relativeEnd, offset, startRadius, endRadius, renderSpot)
      __ctor2__(this, SystemNumerics.Vector3.getZero(), relativeEnd:__clone__(), startRadius, endRadius, renderSpot)
      AttachTo(this, attachTo, offset:__clone__())
    end
    getOffset, setOffset = System.property("Offset")
    getToAttached = function (this)
      return this.toAttached
    end
    getIsAttached = function (this)
      return this.toAttached ~= nil
    end
    getStartPosition = function (this)
      local result = SlipeLuaMtaDefinitions.MtaClient.GetSearchLightStartPosition(this.element)
      return SystemNumerics.Vector3(result[1], result[2], result[3])
    end
    setStartPosition = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.SetSearchLightStartPosition(this.element, value.X, value.Y, value.Z)
    end
    getEndPosition = function (this)
      local result = SlipeLuaMtaDefinitions.MtaClient.GetSearchLightEndPosition(this.element)
      return SystemNumerics.Vector3(result[1], result[2], result[3])
    end
    setEndPosition = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.SetSearchLightEndPosition(this.element, value.X, value.Y, value.Z)
    end
    getStartRadius = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GetSearchLightStartRadius(this.element)
    end
    setStartRadius = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.SetSearchLightStartRadius(this.element, value)
    end
    getEndRadius = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GetSearchLightEndRadius(this.element)
    end
    setEndRadius = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.SetSearchLightEndRadius(this.element, value)
    end
    AttachTo = function (this, toElement, offsetMatrix)
      this.toAttached = toElement
      this.Offset = offsetMatrix:__clone__()
      SlipeLuaClientGame.GameClient.addOnUpdate(System.fn(this, Update))
    end
    AttachTo1 = function (this, toElement, positionOffset, rotationOffset)
      AttachTo2(this, toElement, positionOffset:__clone__(), SlipeLuaSharedHelpers.NumericHelper.EulerToQuaternion(rotationOffset))
    end
    AttachTo2 = function (this, toElement, positionOffset, rotationOffset)
      AttachTo(this, toElement, SystemNumerics.Matrix4x4.Transform(SystemNumerics.Matrix4x4.CreateTranslation(positionOffset), rotationOffset))
    end
    AttachTo3 = function (this, toElement)
      AttachTo(this, toElement, SystemNumerics.Matrix4x4.getIdentity())
    end
    Detach = function (this)
      SlipeLuaClientGame.GameClient.removeOnUpdate(System.fn(this, Update))
    end
    Update = function (this, source, eventArgs)
      setStartPosition(this, SystemNumerics.Vector3.op_Addition(getToAttached(this):getPosition(), this.Offset:__clone__():getTranslation()))
      setEndPosition(this, SystemNumerics.Vector3.Transform(this.relativeEndPosition, SystemNumerics.Matrix4x4.op_Multiply(getToAttached(this):getMatrix(), this.Offset:__clone__())))
    end
    return {
      base = function (out)
        return {
          out.SlipeLua.Shared.Elements.Element
        }
      end,
      getOffset = getOffset,
      setOffset = setOffset,
      getToAttached = getToAttached,
      getIsAttached = getIsAttached,
      getStartPosition = getStartPosition,
      setStartPosition = setStartPosition,
      getEndPosition = getEndPosition,
      setEndPosition = setEndPosition,
      getStartRadius = getStartRadius,
      setStartRadius = setStartRadius,
      getEndRadius = getEndRadius,
      setEndRadius = setEndRadius,
      AttachTo = AttachTo,
      AttachTo1 = AttachTo1,
      AttachTo2 = AttachTo2,
      AttachTo3 = AttachTo3,
      Detach = Detach,
      Update = Update,
      __ctor__ = {
        __ctor1__,
        __ctor2__,
        __ctor3__
      },
      __metadata__ = function (out)
        return {
          fields = {
            { "relativeEndPosition", 0x3, System.Numerics.Vector3 },
            { "toAttached", 0x3, out.SlipeLua.Shared.Elements.PhysicalElement }
          },
          properties = {
            { "EndPosition", 0x106, System.Numerics.Vector3, getEndPosition, setEndPosition },
            { "EndRadius", 0x106, System.Single, getEndRadius, setEndRadius },
            { "IsAttached", 0x206, System.Boolean, getIsAttached },
            { "Offset", 0x106, System.Numerics.Matrix4x4, getOffset, setOffset },
            { "StartPosition", 0x106, System.Numerics.Vector3, getStartPosition, setStartPosition },
            { "StartRadius", 0x106, System.Single, getStartRadius, setStartRadius },
            { "ToAttached", 0x206, out.SlipeLua.Shared.Elements.PhysicalElement, getToAttached }
          },
          methods = {
            { ".ctor", 0x106, __ctor1__, out.SlipeLua.MtaDefinitions.MtaElement },
            { ".ctor", 0x506, __ctor2__, System.Numerics.Vector3, System.Numerics.Vector3, System.Single, System.Single, System.Boolean },
            { ".ctor", 0x606, __ctor3__, out.SlipeLua.Shared.Elements.PhysicalElement, System.Numerics.Vector3, System.Numerics.Matrix4x4, System.Single, System.Single, System.Boolean },
            { "AttachTo", 0x206, AttachTo, out.SlipeLua.Shared.Elements.PhysicalElement, System.Numerics.Matrix4x4 },
            { "AttachTo", 0x306, AttachTo1, out.SlipeLua.Shared.Elements.PhysicalElement, System.Numerics.Vector3, System.Numerics.Vector3 },
            { "AttachTo", 0x306, AttachTo2, out.SlipeLua.Shared.Elements.PhysicalElement, System.Numerics.Vector3, System.Numerics.Quaternion },
            { "AttachTo", 0x106, AttachTo3, out.SlipeLua.Shared.Elements.PhysicalElement },
            { "Detach", 0x6, Detach },
            { "Update", 0x203, Update, out.SlipeLua.Client.Elements.RootElement, out.SlipeLua.Client.Game.Events.OnUpdateEventArgs }
          },
          class = { 0x6, System.new(out.SlipeLua.Shared.Elements.DefaultElementClassAttribute, 2, 33 --[[ElementType.SearchLight]]) }
        }
      end
    }
  end)
end)
