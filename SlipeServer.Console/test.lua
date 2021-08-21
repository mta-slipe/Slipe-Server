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

outputDebugString("Debug message, elapsed time: "..getTickCount())

print("base64 test:", base64Encode("sample text"), base64Decode(base64Encode("sample text")))
print("Some color: ", tocolor(235,23,77,159), tocolor(235,23,77,159) == -1611983027)
print("getColorFromString: ", getColorFromString("#ff0000"));
print("md5 test: ", md5("qwerty") == "D8578EDF8458CE06FBC5BB76A58C5CA4");
print("sha256 test: ", sha256("qwerty") == "65E84BE33532FB784C48129675F9EFF3A682B27168C0EA744B2CF58EE02337C5");

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