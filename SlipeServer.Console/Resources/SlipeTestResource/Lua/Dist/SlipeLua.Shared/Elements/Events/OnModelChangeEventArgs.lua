-- Generated by CSharp.lua Compiler
local System = System
System.namespace("SlipeLua.Shared.Elements.Events", function (namespace)
  namespace.class("OnModelChangeEventArgs", function (namespace)
    local getOldModel, getNewModel, __ctor__
    __ctor__ = function (this, oldModel, newModel)
      this.OldModel = System.cast(System.Int32, oldModel)
      this.NewModel = System.cast(System.Int32, newModel)
    end
    getOldModel = System.property("OldModel", true)
    getNewModel = System.property("NewModel", true)
    return {
      OldModel = 0,
      NewModel = 0,
      __ctor__ = __ctor__,
      __metadata__ = function (out)
        return {
          properties = {
            { "NewModel", 0x201, System.Int32, getNewModel },
            { "OldModel", 0x201, System.Int32, getOldModel }
          },
          methods = {
            { ".ctor", 0x204, nil, System.Object, System.Object }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
