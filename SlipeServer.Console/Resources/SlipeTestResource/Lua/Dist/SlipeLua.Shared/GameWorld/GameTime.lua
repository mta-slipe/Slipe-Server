-- Generated by CSharp.lua Compiler
local System = System
System.namespace("SlipeLua.Shared.GameWorld", function (namespace)
  --/ <summary>
  --/ Class representing a game time, contrary to real time
  --/ </summary>
  namespace.struct("GameTime", function (namespace)
    local getHour, setHour, getMinute, setMinute, __ctor1__, __ctor2__
    __ctor1__ = function (this, hour, minute)
      if hour == nil then
        return
      end
      this.Hour = math.Max(math.Min(hour, 0), 23)
      this.Minute = math.Max(math.Min(minute, 0), 59)
    end
    __ctor2__ = function (this, time)
      this.Hour = time:getHour()
      this.Minute = time:getMinute()
    end
    getHour, setHour = System.property("Hour")
    getMinute, setMinute = System.property("Minute")
    return {
      Hour = 0,
      getHour = getHour,
      setHour = setHour,
      Minute = 0,
      getMinute = getMinute,
      setMinute = setMinute,
      __ctor__ = {
        __ctor1__,
        __ctor2__
      },
      __metadata__ = function (out)
        return {
          properties = {
            { "Hour", 0x106, System.Int32, getHour, setHour },
            { "Minute", 0x106, System.Int32, getMinute, setMinute }
          },
          methods = {
            { ".ctor", 0x206, __ctor1__, System.Int32, System.Int32 },
            { ".ctor", 0x106, __ctor2__, System.DateTime }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
