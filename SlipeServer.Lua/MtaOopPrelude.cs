namespace SlipeServer.Lua;

/// <summary>
/// Compile-time Lua snippets that implement MTA-style OOP constructors and math types.
/// Each constant covers one element category; <see cref="Full"/> concatenates them all.
/// </summary>
public static class MtaOopPrelude
{
    public const string Vectors = """
        -- Vector3
        Vector3 = {}
        Vector3.__index = Vector3
        setmetatable(Vector3, {
            __call = function(cls, x, y, z)
                return setmetatable({ x = x or 0, y = y or 0, z = z or 0 }, cls)
            end
        })
        Vector3.__add      = function(a, b) return Vector3(a.x+b.x, a.y+b.y, a.z+b.z) end
        Vector3.__sub      = function(a, b) return Vector3(a.x-b.x, a.y-b.y, a.z-b.z) end
        Vector3.__unm      = function(a)    return Vector3(-a.x, -a.y, -a.z) end
        Vector3.__eq       = function(a, b) return a.x == b.x and a.y == b.y and a.z == b.z end
        Vector3.__tostring = function(v)    return ("%.4f, %.4f, %.4f"):format(v.x, v.y, v.z) end
        Vector3.__mul = function(a, b)
            if type(a) == "number" then return Vector3(a*b.x, a*b.y, a*b.z) end
            if type(b) == "number" then return Vector3(a.x*b, a.y*b, a.z*b) end
            return Vector3(a.x*b.x, a.y*b.y, a.z*b.z)
        end
        Vector3.__div = function(a, b)
            if type(b) == "number" then return Vector3(a.x/b, a.y/b, a.z/b) end
            return Vector3(a.x/b.x, a.y/b.y, a.z/b.z)
        end
        function Vector3:length()
            return math.sqrt(self.x^2 + self.y^2 + self.z^2)
        end
        function Vector3:normalize()
            local len = self:length()
            if len == 0 then return Vector3(0, 0, 0) end
            return Vector3(self.x/len, self.y/len, self.z/len)
        end
        function Vector3:dot(other)
            return self.x*other.x + self.y*other.y + self.z*other.z
        end
        function Vector3:cross(other)
            return Vector3(
                self.y*other.z - self.z*other.y,
                self.z*other.x - self.x*other.z,
                self.x*other.y - self.y*other.x)
        end

        -- Vector2
        Vector2 = {}
        Vector2.__index = Vector2
        setmetatable(Vector2, {
            __call = function(cls, x, y)
                return setmetatable({ x = x or 0, y = y or 0 }, cls)
            end
        })
        Vector2.__add      = function(a, b) return Vector2(a.x+b.x, a.y+b.y) end
        Vector2.__sub      = function(a, b) return Vector2(a.x-b.x, a.y-b.y) end
        Vector2.__unm      = function(a)    return Vector2(-a.x, -a.y) end
        Vector2.__eq       = function(a, b) return a.x == b.x and a.y == b.y end
        Vector2.__tostring = function(v)    return ("%.4f, %.4f"):format(v.x, v.y) end
        Vector2.__mul = function(a, b)
            if type(a) == "number" then return Vector2(a*b.x, a*b.y) end
            if type(b) == "number" then return Vector2(a.x*b, a.y*b) end
            return Vector2(a.x*b.x, a.y*b.y)
        end
        Vector2.__div = function(a, b)
            if type(b) == "number" then return Vector2(a.x/b, a.y/b) end
            return Vector2(a.x/b.x, a.y/b.y)
        end
        function Vector2:length()    return math.sqrt(self.x^2 + self.y^2) end
        function Vector2:normalize()
            local len = self:length()
            if len == 0 then return Vector2(0, 0) end
            return Vector2(self.x/len, self.y/len)
        end
        function Vector2:dot(other)  return self.x*other.x + self.y*other.y end

        """;

    public const string Matrices = """
        -- Matrix (position + ZXY euler rotation in degrees -> right/forward/up basis vectors)
        Matrix = {}
        Matrix.__index = Matrix
        setmetatable(Matrix, {
            __call = function(cls, position, rotation)
                local yaw   = math.rad(rotation and rotation.z or 0)
                local pitch = math.rad(rotation and rotation.y or 0)
                local roll  = math.rad(rotation and rotation.x or 0)
                local cosY, sinY = math.cos(yaw),   math.sin(yaw)
                local cosP, sinP = math.cos(pitch),  math.sin(pitch)
                local cosR, sinR = math.cos(roll),   math.sin(roll)
                return setmetatable({
                    position = position or Vector3(0, 0, 0),
                    rotation = rotation or Vector3(0, 0, 0),
                    right   = Vector3( cosY*cosR + sinY*sinP*sinR,  sinY*cosR - cosY*sinP*sinR,  cosP*sinR),
                    forward = Vector3(-sinY*cosP,                    cosY*cosP,                   sinP),
                    up      = Vector3( cosY*sinR - sinY*sinP*cosR,   sinY*sinR + cosY*sinP*cosR,  cosP*cosR),
                }, cls)
            end
        })

        """;

    public const string Vehicles = """
        -- Vehicle constructor (OOP alias for createVehicle)
        if createVehicle then
            Vehicle = function(model, pos, rot)
                if type(pos) == "table" then
                    return createVehicle(
                        model,
                        pos.x or 0, pos.y or 0, pos.z or 0,
                        rot and (rot.x or 0) or 0,
                        rot and (rot.y or 0) or 0,
                        rot and (rot.z or 0) or 0
                    )
                end
                return createVehicle(model, pos, rot)
            end
        end

        """;

    public const string Peds = """
        -- Ped constructor (OOP alias for createPed)
        if createPed then
            Ped = function(model, x, y, z, rot)
                if type(x) == "table" then
                    return createPed(model, x.x, x.y, x.z, y or 0)
                end
                return createPed(model, x or 0, y or 0, z or 0, rot or 0)
            end
        end

        """;

    public const string Markers = """
        -- Marker constructor (OOP alias for createMarker)
        if createMarker then
            Marker = function(x, y, z, markerType, size, r, g, b, a)
                if type(x) == "table" then
                    local pos = x
                    return createMarker(pos.x or 0, pos.y or 0, pos.z or 0, y, z, rot, g, b, a)
                end
                return createMarker(x or 0, y or 0, z or 0, markerType, size, r, g, b, a)
            end
        end

        """;

    public const string Objects = """
        -- Object constructor (OOP alias for createObject)
        if createObject then
            Object = function(model, x, y, z, rx, ry, rz, isLowLod)
                if type(x) == "table" then
                    local pos = x
                    local rot = y
                    return createObject(model,
                        pos.x or 0, pos.y or 0, pos.z or 0,
                        rot and (rot.x or 0) or 0,
                        rot and (rot.y or 0) or 0,
                        rot and (rot.z or 0) or 0,
                        z)
                end
                return createObject(model, x or 0, y or 0, z or 0, rx or 0, ry or 0, rz or 0, isLowLod)
            end
        end

        """;

    public const string ColShapes = """
        -- ColShape constructors (OOP aliases for createCol* functions)
        if createColSphere then
            ColShape = {}
            ColShape.Sphere    = function(x, y, z, radius)    return createColSphere(x, y, z, radius) end
            ColShape.Cube      = function(x, y, z, w, h, d)   return createColCuboid(x, y, z, w, h, d) end
            ColShape.Rectangle = function(x, y, w, h)         return createColRectangle(x, y, w, h) end
            ColShape.Circle    = function(x, y, radius)       return createColCircle(x, y, radius) end
            ColShape.Tube      = function(x, y, z, radius, h) return createColTube(x, y, z, radius, h) end
            ColShape.Polygon   = function(x, y, ...)          return createColPolygon(x, y, ...) end
        end

        """;

    public const string Full =
        Vectors +
        Matrices +
        Vehicles +
        Peds +
        Markers +
        Objects +
        ColShapes;
}
