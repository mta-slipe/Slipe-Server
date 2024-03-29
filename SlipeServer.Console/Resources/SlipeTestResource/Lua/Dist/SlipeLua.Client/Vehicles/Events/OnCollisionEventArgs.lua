-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaSharedElements = SlipeLua.Shared.Elements
local SystemNumerics = System.Numerics
System.namespace("SlipeLua.Client.Vehicles.Events", function (namespace)
  namespace.class("OnCollisionEventArgs", function (namespace)
    local getElement, getForce, getPart, getPosition, getNormal, getModel, __ctor__
    __ctor__ = function (this, hitElement, force, bodyPart, cx, cy, cz, nx, ny, nz, model)
      this.Position = System.default(SystemNumerics.Vector3)
      this.Normal = System.default(SystemNumerics.Vector3)
      this.Element = SlipeLuaSharedElements.ElementManager.getInstance():GetElement(hitElement, SlipeLuaSharedElements.PhysicalElement)
      this.Force = System.cast(System.Single, force)
      this.Part = System.cast(System.Int32, bodyPart)
      this.Position = SystemNumerics.Vector3(System.cast(System.Single, cx), System.cast(System.Single, cy), System.cast(System.Single, cz))
      this.Normal = SystemNumerics.Vector3(System.cast(System.Single, nx), System.cast(System.Single, ny), System.cast(System.Single, nz))
      this.Model = System.cast(System.Int32, model)
    end
    getElement = System.property("Element", true)
    getForce = System.property("Force", true)
    getPart = System.property("Part", true)
    getPosition = System.property("Position", true)
    getNormal = System.property("Normal", true)
    getModel = System.property("Model", true)
    return {
      getElement = getElement,
      Force = 0,
      getForce = getForce,
      Part = 0,
      getPart = getPart,
      getPosition = getPosition,
      getNormal = getNormal,
      Model = 0,
      getModel = getModel,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          properties = {
            { "Element", 0x206, out.SlipeLua.Shared.Elements.PhysicalElement, getElement },
            { "Force", 0x206, System.Single, getForce },
            { "Model", 0x206, System.Int32, getModel },
            { "Normal", 0x206, System.Numerics.Vector3, getNormal },
            { "Part", 0x206, System.Int32, getPart },
            { "Position", 0x206, System.Numerics.Vector3, getPosition }
          },
          methods = {
            { ".ctor", 0xA04, nil, out.SlipeLua.MtaDefinitions.MtaElement, System.Object, System.Object, System.Object, System.Object, System.Object, System.Object, System.Object, System.Object, System.Object }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
