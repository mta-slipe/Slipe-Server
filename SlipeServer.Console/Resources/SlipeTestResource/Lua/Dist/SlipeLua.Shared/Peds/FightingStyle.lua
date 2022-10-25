-- Generated by CSharp.lua Compiler
local System = System
System.namespace("SlipeLua.Shared.Peds", function (namespace)
  --/ <summary>
  --/ Represents the fighting style of a ped
  --/ </summary>
  namespace.enum("FightingStyle", function ()
    return {
      Standard = 4,
      Boxing = 5,
      Kung_Fu = 6,
      Knee_Head = 7,
      Grab_Kick = 15,
      Elbows = 16,
      __metadata__ = function (out)
        return {
          fields = {
            { "Boxing", 0xE, System.Int32 },
            { "Elbows", 0xE, System.Int32 },
            { "Grab_Kick", 0xE, System.Int32 },
            { "Knee_Head", 0xE, System.Int32 },
            { "Kung_Fu", 0xE, System.Int32 },
            { "Standard", 0xE, System.Int32 }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
