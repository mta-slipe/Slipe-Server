-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
local SlipeLuaSharedUtilities = SlipeLua.Shared.Utilities
local SlipeLuaClientDx
System.import(function (out)
  SlipeLuaClientDx = SlipeLua.Client.Dx
end)
System.namespace("SlipeLua.Client.Dx", function (namespace)
  --/ <summary>
  --/ Represents a drawable circle
  --/ </summary>
  namespace.class("Circle", function (namespace)
    local getRadius, setRadius, getCenterColor, setCenterColor, getStartAngle, setStartAngle, getStopAngle, setStopAngle, 
    getSegments, setSegments, getRatio, setRatio, Draw, __ctor1__, __ctor2__, __ctor3__
    __ctor1__ = function (this, position, radius, color, centerColor, startAngle, stopAngle, segments, ratio, postGUI)
      SlipeLuaClientDx.Dx2DObject.__ctor__(this)
      this:setPosition(position:__clone__())
      this.Radius = radius
      this.Color = color
      this.CenterColor = centerColor
      this.StartAngle = startAngle
      this.StopAngle = stopAngle
      this.Segments = segments
      this.Ratio = ratio
      this.PostGUI = postGUI
    end
    __ctor2__ = function (this, position, radius, color)
      __ctor1__(this, position:__clone__(), radius, color, color, 0, 360, 32, 1, false)
    end
    __ctor3__ = function (this, position, radius)
      __ctor2__(this, position:__clone__(), radius, SlipeLuaSharedUtilities.Color.getWhite())
    end
    getRadius, setRadius = System.property("Radius")
    getCenterColor, setCenterColor = System.property("CenterColor")
    getStartAngle, setStartAngle = System.property("StartAngle")
    getStopAngle, setStopAngle = System.property("StopAngle")
    getSegments, setSegments = System.property("Segments")
    getRatio, setRatio = System.property("Ratio")
    Draw = function (this, source, eventArgs)
      return SlipeLuaMtaDefinitions.MtaClient.DxDrawCircle(this:getPosition().X, this:getPosition().Y, this.Radius, this.StartAngle, this.StopAngle, this.Color:getHex(), this.CenterColor:getHex(), this.Segments, this.Ratio, this.PostGUI)
    end
    return {
      base = function (out)
        return {
          out.SlipeLua.Client.Dx.Dx2DObject,
          out.SlipeLua.Client.Dx.IDrawable
        }
      end,
      Radius = 0,
      getRadius = getRadius,
      setRadius = setRadius,
      getCenterColor = getCenterColor,
      setCenterColor = setCenterColor,
      StartAngle = 0,
      StopAngle = 0,
      Segments = 0,
      Ratio = 0,
      Draw = Draw,
      __ctor__ = {
        __ctor1__,
        __ctor2__,
        __ctor3__
      },
      __metadata__ = function (out)
        return {
          properties = {
            { "CenterColor", 0x106, out.SlipeLua.Shared.Utilities.Color, getCenterColor, setCenterColor },
            { "Radius", 0x106, System.Single, getRadius, setRadius },
            { "Ratio", 0x101, System.Int32, getRatio, setRatio },
            { "Segments", 0x101, System.Int32, getSegments, setSegments },
            { "StartAngle", 0x101, System.Single, getStartAngle, setStartAngle },
            { "StopAngle", 0x101, System.Single, getStopAngle, setStopAngle }
          },
          methods = {
            { ".ctor", 0x906, __ctor1__, System.Numerics.Vector2, System.Single, out.SlipeLua.Shared.Utilities.Color, out.SlipeLua.Shared.Utilities.Color, System.Single, System.Single, System.Int32, System.Int32, System.Boolean },
            { ".ctor", 0x306, __ctor2__, System.Numerics.Vector2, System.Single, out.SlipeLua.Shared.Utilities.Color },
            { ".ctor", 0x206, __ctor3__, System.Numerics.Vector2, System.Single },
            { "Draw", 0x286, Draw, out.SlipeLua.Client.Elements.RootElement, out.SlipeLua.Client.Rendering.Events.OnRenderEventArgs, System.Boolean }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
