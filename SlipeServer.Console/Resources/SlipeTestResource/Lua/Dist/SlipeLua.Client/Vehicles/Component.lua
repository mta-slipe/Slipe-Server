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
  --/ Represents a component of a vehicle
  --/ </summary>
  namespace.class("Component", function (namespace)
    local getPosition, setPosition, getRotation, setRotation, getVisible, setVisible, getScale, setScale, 
    getBase, setBase, ResetPosition, ResetRotation, ResetScale, __ctor1__, __ctor2__
    __ctor1__ = function (this, vehicle, type, relativeBase)
      this.vehicle = vehicle
      this.component = type:EnumToString(SlipeLuaClientVehicles.ComponentType):ToLower()
      this.relativeBase = relativeBase:EnumToString(SlipeLuaClientVehicles.ComponentBase):ToLower()
    end
    __ctor2__ = function (this, vehicle, type, relativeBase)
      this.vehicle = vehicle
      this.component = type
      this.relativeBase = relativeBase:EnumToString(SlipeLuaClientVehicles.ComponentBase):ToLower()
    end
    getPosition = function (this)
      local r = SlipeLuaMtaDefinitions.MtaClient.GetVehicleComponentPosition(this.vehicle:getMTAElement(), this.component, this.relativeBase)
      return SystemNumerics.Vector3(r[1], r[2], r[3])
    end
    setPosition = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.SetVehicleComponentPosition(this.vehicle:getMTAElement(), this.component, value.X, value.Y, value.Z, this.relativeBase)
    end
    getRotation = function (this)
      local r = SlipeLuaMtaDefinitions.MtaClient.GetVehicleComponentRotation(this.vehicle:getMTAElement(), this.component, this.relativeBase)
      return SystemNumerics.Vector3(r[1], r[2], r[3])
    end
    setRotation = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.SetVehicleComponentRotation(this.vehicle:getMTAElement(), this.component, value.X, value.Y, value.Z, this.relativeBase)
    end
    getVisible = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GetVehicleComponentVisible(this.vehicle:getMTAElement(), this.component)
    end
    setVisible = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.SetVehicleComponentVisible(this.vehicle:getMTAElement(), this.component, value)
    end
    getScale = function (this)
      local r = SlipeLuaMtaDefinitions.MtaClient.GetVehicleComponentScale(this.vehicle:getMTAElement(), this.component, this.relativeBase)
      return SystemNumerics.Vector3(r[1], r[2], r[3])
    end
    setScale = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.SetVehicleComponentScale(this.vehicle:getMTAElement(), this.component, value.X, value.Y, value.Z, this.relativeBase)
    end
    getBase, setBase = System.property("Base")
    ResetPosition = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.ResetVehicleComponentPosition(this.vehicle:getMTAElement(), this.component)
    end
    ResetRotation = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.ResetVehicleComponentRotation(this.vehicle:getMTAElement(), this.component)
    end
    ResetScale = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.ResetVehicleComponentScale(this.vehicle:getMTAElement(), this.component)
    end
    return {
      getPosition = getPosition,
      setPosition = setPosition,
      getRotation = getRotation,
      setRotation = setRotation,
      getVisible = getVisible,
      setVisible = setVisible,
      getScale = getScale,
      setScale = setScale,
      Base = 0,
      getBase = getBase,
      setBase = setBase,
      ResetPosition = ResetPosition,
      ResetRotation = ResetRotation,
      ResetScale = ResetScale,
      __ctor__ = {
        __ctor1__,
        __ctor2__
      },
      __metadata__ = function (out)
        return {
          fields = {
            { "component", 0x1, System.String },
            { "relativeBase", 0x1, System.String },
            { "vehicle", 0x1, out.SlipeLua.Client.Vehicles.BaseVehicle }
          },
          properties = {
            { "Base", 0x106, System.Int32, getBase, setBase },
            { "Position", 0x106, System.Numerics.Vector3, getPosition, setPosition },
            { "Rotation", 0x106, System.Numerics.Vector3, getRotation, setRotation },
            { "Scale", 0x106, System.Numerics.Vector3, getScale, setScale },
            { "Visible", 0x106, System.Boolean, getVisible, setVisible }
          },
          methods = {
            { ".ctor", 0x306, __ctor1__, out.SlipeLua.Client.Vehicles.BaseVehicle, System.Int32, System.Int32 },
            { ".ctor", 0x306, __ctor2__, out.SlipeLua.Client.Vehicles.BaseVehicle, System.String, System.Int32 },
            { "ResetPosition", 0x86, ResetPosition, System.Boolean },
            { "ResetRotation", 0x86, ResetRotation, System.Boolean },
            { "ResetScale", 0x86, ResetScale, System.Boolean }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
