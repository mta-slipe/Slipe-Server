-- Generated by CSharp.lua Compiler
local System = System
System.namespace("SlipeLua.Client.IO.Events", function (namespace)
  namespace.class("OnCharacterEventArgs", function (namespace)
    local getCharacter, __ctor__
    __ctor__ = function (this, character)
      this.Character = System.cast(System.String, character)
    end
    getCharacter = System.property("Character", true)
    return {
      getCharacter = getCharacter,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          properties = {
            { "Character", 0x206, System.String, getCharacter }
          },
          methods = {
            { ".ctor", 0x104, nil, System.Object }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
