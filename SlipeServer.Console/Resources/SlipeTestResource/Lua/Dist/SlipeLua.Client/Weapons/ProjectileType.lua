-- Generated by CSharp.lua Compiler
local System = System
System.namespace("SlipeLua.Client.Weapons", function (namespace)
  --/ <summary>
  --/ Represents different projectiles
  --/ </summary>
  namespace.enum("ProjectileType", function ()
    return {
      Grenade = 16,
      TearGasGrenade = 17,
      Molotov = 18,
      RocketSimple = 19,
      RocketHs = 20,
      AirBomb = 21,
      SatchelCharge = 39,
      __metadata__ = function (out)
        return {
          fields = {
            { "AirBomb", 0xE, System.Int32 },
            { "Grenade", 0xE, System.Int32 },
            { "Molotov", 0xE, System.Int32 },
            { "RocketHs", 0xE, System.Int32 },
            { "RocketSimple", 0xE, System.Int32 },
            { "SatchelCharge", 0xE, System.Int32 },
            { "TearGasGrenade", 0xE, System.Int32 }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
