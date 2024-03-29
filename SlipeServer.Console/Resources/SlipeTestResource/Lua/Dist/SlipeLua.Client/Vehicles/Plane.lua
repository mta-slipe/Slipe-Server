-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
local SystemNumerics = System.Numerics
local SlipeLuaClientVehicles
System.import(function (out)
  SlipeLuaClientVehicles = SlipeLua.Client.Vehicles
end)
System.namespace("SlipeLua.Client.Vehicles", function (namespace)
  --/ <summary>
  --/ Planes as a special type of vehicle
  --/ </summary>
  namespace.class("Plane", function (namespace)
    local getLandingGearDown, setLandingGearDown, op_Explicit1, class, __ctor1__, __ctor2__, __ctor3__
    __ctor1__ = function (this, element)
      SlipeLuaClientVehicles.BaseVehicle.__ctor__[1](this, element)
    end
    __ctor2__ = function (this, model, position)
      __ctor3__(this, model, position:__clone__(), SystemNumerics.Vector3.getZero(), "", 1, 1)
    end
    __ctor3__ = function (this, model, position, rotation, numberplate, variant1, variant2)
      SlipeLuaClientVehicles.BaseVehicle.__ctor__[2](this, model, position:__clone__(), rotation:__clone__(), numberplate, variant1, variant2)
    end
    getLandingGearDown = function (this)
      return SlipeLuaMtaDefinitions.MtaShared.GetVehicleLandingGearDown(this.element)
    end
    setLandingGearDown = function (this, value)
      SlipeLuaMtaDefinitions.MtaShared.SetVehicleLandingGearDown(this.element, value)
    end
    op_Explicit1 = function (vehicle)
      if System.is(SlipeLuaClientVehicles.VehicleModel.FromId(vehicle:getModel()), SlipeLuaClientVehicles.PlaneModel) then
        return class(vehicle:getMTAElement())
      end

      System.throw((System.InvalidCastException("The vehicle is not a plane")))
    end
    class = {
      base = function (out)
        return {
          out.SlipeLua.Client.Vehicles.BaseVehicle
        }
      end,
      getLandingGearDown = getLandingGearDown,
      setLandingGearDown = setLandingGearDown,
      op_Explicit1 = op_Explicit1,
      __ctor__ = {
        __ctor1__,
        __ctor2__,
        __ctor3__
      },
      __metadata__ = function (out)
        return {
          properties = {
            { "LandingGearDown", 0x106, System.Boolean, getLandingGearDown, setLandingGearDown }
          },
          methods = {
            { ".ctor", 0x106, __ctor1__, out.SlipeLua.MtaDefinitions.MtaElement },
            { ".ctor", 0x206, __ctor2__, out.SlipeLua.Client.Vehicles.PlaneModel, System.Numerics.Vector3 },
            { ".ctor", 0x606, __ctor3__, out.SlipeLua.Client.Vehicles.PlaneModel, System.Numerics.Vector3, System.Numerics.Vector3, System.String, System.Int32, System.Int32 }
          },
          class = { 0x6 }
        }
      end
    }
    return class
  end)

  --/ <summary>
  --/ Represents models that represent planes
  --/ </summary>
  namespace.class("PlaneModel", function (namespace)
    local __ctor__
    __ctor__ = function (this, id)
      SlipeLuaClientVehicles.VehicleModel.__ctor__(this, id)
    end
    return {
      base = function (out)
        return {
          out.SlipeLua.Client.Vehicles.VehicleModel
        }
      end,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          methods = {
            { ".ctor", 0x104, nil, System.Int32 }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
