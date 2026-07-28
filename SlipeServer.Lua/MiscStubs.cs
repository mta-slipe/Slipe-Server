// MiscStubs

namespace SlipeServer.Lua;

public static class MiscStubs
{
    public const string TableGetN = """
        function table.getn(t)
            return #t
        end
        """;

    /// <summary>
    /// Provides a MTA-compatible debug.getregistry() that returns a registry table with
    /// an 'mt' key containing element-type metatables, matching MTA SA's OOP system.
    /// Also wraps dbConnect to return a Lua proxy table with proper OOP dispatch,
    /// and wraps dbQuery/dbExec to unwrap proxy tables back to C# handles.
    /// </summary>
    public const string MtaOopRegistry = """
        do
            local _mtaElementTypes = {
                "Element", "Player", "Ped", "Vehicle", "Object", "Blip", "Marker",
                "ColShape", "Team", "Connection", "Timer", "Resource",
                "RadarArea", "Pickup", "Sound", "Light", "Effect", "Water",
                "Weapon", "Projectile", "DxTexture", "DxFont", "DxShader",
                "DxScreenSource", "DxRenderTarget", "Browser", "EngineCOL",
                "EngineTXD", "EngineDFF", "GuiElement"
            }

            local _registryMt = {}
            for _, typeName in ipairs(_mtaElementTypes) do
                _registryMt[typeName] = {
                    __set = {},
                    __get = {},
                    __class = nil
                }
            end

            local _registryTable = { mt = _registryMt }

            -- Override debug.getregistry to return our MTA-compatible registry
            local _origDebug = debug
            debug = setmetatable({}, {
                __index = function(_, key)
                    if key == "getregistry" then
                        return function() return _registryTable end
                    end
                    if _origDebug then
                        return _origDebug[key]
                    end
                    return nil
                end
            })

            -- Wrap dbConnect to return a Lua proxy with OOP dispatch
            if dbConnect then
                local _originalDbConnect = dbConnect
                dbConnect = function(dbtype, host, username, password, options)
                    local handle = _originalDbConnect(dbtype, host, username, password, options)
                    local proxy = { __handle = handle }
                    setmetatable(proxy, {
                        __index = function(self, key)
                            local mt = _registryMt["Connection"]
                            if mt and mt.__class then
                                return mt.__class[key]
                            end
                        end
                    })
                    return proxy
                end
            end

            -- Wrap dbQuery to handle all MTA:SA call forms and unwrap proxy tables:
            --   dbQuery(connection, query, ...)                         -- no callback
            --   dbQuery(callback, connection, query, ...)               -- with callback
            --   dbQuery(callback, callbackArgs, connection, query, ...) -- with callback + args table
            if dbQuery then
                local _originalDbQuery = dbQuery
                dbQuery = function(callback_or_conn, conn_or_query, ...)
                    -- Form 1a: no callback, proxy-table connection: dbQuery(proxyConn, query, ...)
                    if type(callback_or_conn) == "table" and rawget(callback_or_conn, "__handle") then
                        return _originalDbQuery(nil, rawget(callback_or_conn, "__handle"), conn_or_query, ...)
                    end
                    -- Form 1b: no callback, raw (non-function) connection: dbQuery(rawConn, query, ...)
                    if type(callback_or_conn) ~= "function" then
                        return _originalDbQuery(nil, callback_or_conn, conn_or_query, ...)
                    end
                    -- callback_or_conn is a function from here on
                    -- Form 3: dbQuery(callback, callbackArgsTable, connection, query, ...)
                    -- Detect by: conn_or_query is a plain table without __handle
                    if type(conn_or_query) == "table" and not rawget(conn_or_query, "__handle") then
                        local args = { ... }
                        local conn = args[1]
                        local query = args[2]
                        local rest = {}
                        for i = 3, #args do rest[#rest + 1] = args[i] end
                        if type(conn) == "table" and rawget(conn, "__handle") then
                            conn = rawget(conn, "__handle")
                        end
                        return _originalDbQuery(callback_or_conn, conn, query, table.unpack(rest))
                    end
                    -- Form 2: dbQuery(callback, connection, query, ...)
                    if type(conn_or_query) == "table" and rawget(conn_or_query, "__handle") then
                        return _originalDbQuery(callback_or_conn, rawget(conn_or_query, "__handle"), ...)
                    end
                    return _originalDbQuery(callback_or_conn, conn_or_query, ...)
                end
            end

            -- Wrap dbExec to unwrap proxy tables
            if dbExec then
                local _originalDbExec = dbExec
                dbExec = function(conn, query, ...)
                    if type(conn) == "table" and rawget(conn, "__handle") then
                        return _originalDbExec(rawget(conn, "__handle"), query, ...)
                    end
                    return _originalDbExec(conn, query, ...)
                end
            end

            -- Wrap dbPrepareString to unwrap proxy tables
            if dbPrepareString then
                local _originalDbPrepareString = dbPrepareString
                dbPrepareString = function(conn, query, ...)
                    if type(conn) == "table" and rawget(conn, "__handle") then
                        return _originalDbPrepareString(rawget(conn, "__handle"), query, ...)
                    end
                    return _originalDbPrepareString(conn, query, ...)
                end
            end

            -- isElement check for connection proxies
            if isElement then
                local _originalIsElement = isElement
                isElement = function(element)
                    if type(element) == "table" and rawget(element, "__handle") then
                        return true
                    end
                    return _originalIsElement(element)
                end
            end
        end
        """;

    public const string All = $"""
        {TableGetN}
        {MtaOopRegistry}
    """;
}
