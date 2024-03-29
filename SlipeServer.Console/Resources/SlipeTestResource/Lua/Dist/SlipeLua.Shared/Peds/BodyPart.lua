-- Generated by CSharp.lua Compiler
local System = System
System.namespace("SlipeLua.Shared.Peds", function (namespace)
  --/ <summary>
  --/ Represents either a ped body part
  --/ </summary>
  namespace.enum("BodyPart", function ()
    return {
      Torso = 3,
      Ass = 4,
      LeftArm = 5,
      RightArm = 6,
      LeftLeg = 7,
      RightLeg = 8,
      Head = 9,
      __metadata__ = function (out)
        return {
          fields = {
            { "Ass", 0xE, System.Int32 },
            { "Head", 0xE, System.Int32 },
            { "LeftArm", 0xE, System.Int32 },
            { "LeftLeg", 0xE, System.Int32 },
            { "RightArm", 0xE, System.Int32 },
            { "RightLeg", 0xE, System.Int32 },
            { "Torso", 0xE, System.Int32 }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
