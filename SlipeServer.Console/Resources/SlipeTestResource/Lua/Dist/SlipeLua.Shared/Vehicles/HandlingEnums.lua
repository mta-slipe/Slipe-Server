-- Generated by CSharp.lua Compiler
local System = System
System.namespace("SlipeLua.Shared.Vehicles", function (namespace)
  --/ <summary>
  --/ Represents different vehicle drive types
  --/ </summary>
  namespace.enum("DriveType", function ()
    return {
      Rwd = 0,
      Fwd = 1,
      Awd = 2,
      __metadata__ = function (out)
        return {
          fields = {
            { "Awd", 0xE, System.Int32 },
            { "Fwd", 0xE, System.Int32 },
            { "Rwd", 0xE, System.Int32 }
          },
          class = { 0x6 }
        }
      end
    }
  end)

  --/ <summary>
  --/ Represents dfiferent engine types
  --/ </summary>
  namespace.enum("EngineType", function ()
    return {
      Petrol = 0,
      Diesel = 1,
      Electric = 2,
      __metadata__ = function (out)
        return {
          fields = {
            { "Diesel", 0xE, System.Int32 },
            { "Electric", 0xE, System.Int32 },
            { "Petrol", 0xE, System.Int32 }
          },
          class = { 0x6 }
        }
      end
    }
  end)

  --/ <summary>
  --/ Represents lights used in Vehicle Handling
  --/ </summary>
  namespace.enum("VehicleLightType", function ()
    return {
      Long = 0,
      Small = 1,
      Big = 2,
      Tall = 3,
      __metadata__ = function (out)
        return {
          fields = {
            { "Big", 0xE, System.Int32 },
            { "Long", 0xE, System.Int32 },
            { "Small", 0xE, System.Int32 },
            { "Tall", 0xE, System.Int32 }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
