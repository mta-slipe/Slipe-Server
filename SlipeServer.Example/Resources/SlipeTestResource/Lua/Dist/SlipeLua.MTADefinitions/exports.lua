local System = System

local Export = {}

Export.Invoke = function (resource, func, ...)
	local export = exports[resource]

	local varargs = {...}
	if (#varargs == 1 and System.is(varargs[1], ArrayObject) ) then
		iprint(varargs)
	  return export[func](export, unpack(varargs[1]))	
	end

	return export[func](export, ...)	
end

System.define("SlipeLua.Exports.Export", Export)