local object = createObject(321, 5, 5, 5)
setElementPosition(object, 50, 50, 250)
setElementRotation(object, 180, 180, 90)

local object2 = createObject(321, 10, 10, 10)

addEventHandler("onElementDestroyed", object, function() print('OBJECT ELEMENT DESTROYED') end)
addEventHandler("onElementDestroyed", root, function() print('ANY ELEMENT DESTROYED') end)

print(type(object), getElementType(object), object)
print(type(root), getElementType(root), root)

setElementPosition(object, 1337, 2337, 3337)
local x, y, z = getElementPosition(object)
print(x, y, z)

destroyElement(object2)
destroyElement(object)

outputDebugString("Debug message")

function commandHandler(plr, cmd, ...)
	print("command:",plr, cmd, ...)
end

function commandHandler21(plr, cmd, ...)
	print("command21:",plr, cmd, ...)
end
function commandHandler22(plr, cmd, ...)
	print("command22:",plr, cmd, ...)
end

addCommandHandler("foo1", commandHandler) --
addCommandHandler("foo2", commandHandler)
addCommandHandler("foo3", commandHandler) --
addCommandHandler("foo4", commandHandler21)
addCommandHandler("foo4", commandHandler22)
addCommandHandler("foo5", commandHandler21) --
addCommandHandler("foo5", commandHandler22) --
removeCommandHandler("foo2", commandHandler)
removeCommandHandler("foo4")