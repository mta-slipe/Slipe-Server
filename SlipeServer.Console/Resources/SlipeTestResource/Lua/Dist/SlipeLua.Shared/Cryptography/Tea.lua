-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
local DictStringString = System.Dictionary(System.String, System.String)
System.namespace("SlipeLua.Shared.Cryptography", function (namespace)
  --/ <summary>
  --/ Represents static wrappers for the Tiny Encryption Algorithm 
  --/ </summary>
  namespace.class("Tea", function (namespace)
    local Encrypt, Decrypt
    Encrypt = function (input, key)
      local options = DictStringString()
      options:set("key", key)
      return SlipeLuaMtaDefinitions.MtaShared.EncodeString("tea", input, options)
    end
    Decrypt = function (input, key)
      local options = DictStringString()
      options:set("key", key)
      return SlipeLuaMtaDefinitions.MtaShared.DecodeString("tea", input, options)
    end
    return {
      Encrypt = Encrypt,
      Decrypt = Decrypt,
      __metadata__ = function (out)
        return {
          methods = {
            { "Decrypt", 0x28E, Decrypt, System.String, System.String, System.String },
            { "Encrypt", 0x28E, Encrypt, System.String, System.String, System.String }
          },
          class = { 0xE }
        }
      end
    }
  end)
end)
