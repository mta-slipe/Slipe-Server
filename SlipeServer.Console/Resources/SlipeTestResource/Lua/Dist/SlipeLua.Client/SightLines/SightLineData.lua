-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaSharedElements = SlipeLua.Shared.Elements
local SlipeLuaSharedHelpers = SlipeLua.Shared.Helpers
local SystemNumerics = System.Numerics
System.namespace("SlipeLua.Client.SightLines", function (namespace)
  --/ <summary>
  --/ Class that wraps the huge amount of data that can be retrieved from SightLine Process
  --/ </summary>
  namespace.class("SightLineData", function (namespace)
    local getDidHit, getCollisionPosition, getHitElement, getSurfaceMaterial, getLighting, getNormal, getBodyPart, getVehiclePart, 
    getWorldModelID, getWorldModelMatrix, getWorldLODModelID, __ctor__
    __ctor__ = function (this, d)
      this.CollisionPosition = System.default(SystemNumerics.Vector3)
      this.Normal = System.default(SystemNumerics.Vector3)
      this.WorldModelMatrix = System.default(SystemNumerics.Matrix4x4)
      this.DidHit = d[1]
      if this.DidHit then
        this.CollisionPosition = SystemNumerics.Vector3(d[2], d[3], d[4])
        if d[5] ~= nil then
          this.HitElement = SlipeLuaSharedElements.ElementManager.getInstance():GetElement(d[5], SlipeLuaSharedElements.PhysicalElement)
        end
        this.Normal = SystemNumerics.Vector3(d[6], d[7], d:getRest()[1])
        this.SurfaceMaterial = d:getRest()[2]
        this.Lighting = d:getRest()[3]
        this.piece = d:getRest()[4]

        if d:getRest()[5] ~= nil then
          this.WorldModelID = System.Nullable.Value(d:getRest()[5])
          this.WorldModelMatrix = SystemNumerics.Matrix4x4.op_Addition(SystemNumerics.Matrix4x4.CreateTranslation(d:getRest()[6], d:getRest()[7], d:getRest():getRest()[1]), SystemNumerics.Matrix4x4.CreateFromQuaternion(SlipeLuaSharedHelpers.NumericHelper.EulerToQuaternion(SystemNumerics.Vector3(d:getRest():getRest()[2], d:getRest():getRest()[3], d:getRest():getRest()[4]))))
          this.WorldLODModelID = d:getRest():getRest()[5]
        end
      end
    end
    getDidHit = System.property("DidHit", true)
    getCollisionPosition = System.property("CollisionPosition", true)
    getHitElement = System.property("HitElement", true)
    getSurfaceMaterial = System.property("SurfaceMaterial", true)
    getLighting = System.property("Lighting", true)
    getNormal = System.property("Normal", true)
    getBodyPart = function (this)
      return this.piece
    end
    getVehiclePart = function (this)
      return this.piece
    end
    getWorldModelID = System.property("WorldModelID", true)
    getWorldModelMatrix = System.property("WorldModelMatrix", true)
    getWorldLODModelID = System.property("WorldLODModelID", true)
    return {
      piece = 0,
      DidHit = false,
      getDidHit = getDidHit,
      getCollisionPosition = getCollisionPosition,
      getHitElement = getHitElement,
      SurfaceMaterial = 0,
      getSurfaceMaterial = getSurfaceMaterial,
      Lighting = 0,
      getLighting = getLighting,
      getNormal = getNormal,
      getBodyPart = getBodyPart,
      getVehiclePart = getVehiclePart,
      WorldModelID = 0,
      getWorldModelID = getWorldModelID,
      getWorldModelMatrix = getWorldModelMatrix,
      WorldLODModelID = 0,
      getWorldLODModelID = getWorldLODModelID,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          fields = {
            { "piece", 0x1, System.Int32 }
          },
          properties = {
            { "BodyPart", 0x206, System.Int32, getBodyPart },
            { "CollisionPosition", 0x206, System.Numerics.Vector3, getCollisionPosition },
            { "DidHit", 0x206, System.Boolean, getDidHit },
            { "HitElement", 0x206, out.SlipeLua.Shared.Elements.PhysicalElement, getHitElement },
            { "Lighting", 0x206, System.Single, getLighting },
            { "Normal", 0x206, System.Numerics.Vector3, getNormal },
            { "SurfaceMaterial", 0x206, System.Int32, getSurfaceMaterial },
            { "VehiclePart", 0x206, System.Int32, getVehiclePart },
            { "WorldLODModelID", 0x206, System.Int32, getWorldLODModelID },
            { "WorldModelID", 0x206, System.Int32, getWorldModelID },
            { "WorldModelMatrix", 0x206, System.Numerics.Matrix4x4, getWorldModelMatrix }
          },
          methods = {
            { ".ctor", 0x106, nil, System.Tuple }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
