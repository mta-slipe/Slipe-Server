﻿if(isSlipeServer)then
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
	outputServerLog("Server log")
	clearChatBox()

	print("base64 test:", base64Encode("sample text"), base64Decode(base64Encode("sample text")))
	print("Some color: ", tocolor(235,23,77,159), tocolor(235,23,77,159) == -1611983027)
	print("getColorFromString: ", getColorFromString("#ff0000"));
	print("md5 test: ", md5("qwerty") == "D8578EDF8458CE06FBC5BB76A58C5CA4");
	print("sha256 test: ", sha256("qwerty") == "65E84BE33532FB784C48129675F9EFF3A682B27168C0EA744B2CF58EE02337C5");
	print("custom definitions:",add(2,2), substract(2,2))

	-- Begin blips
	local blip = createBlip(100, 0,0, 51, 0, 0, 0, 255);
	print("blip color:", getBlipColor(blip))
	print("blip icon:", getBlipIcon(blip))
	print("blip ordering:", getBlipOrdering(blip))
	print("blip size:", getBlipSize(blip))
	print("blip visible distance:", getBlipVisibleDistance(blip))
	-- End blips
end
createExplosion(-10, 0,0, 4);

function foo()

end
callbackEqual(foo, foo)
createExplosion(-10, 0,0, 4);

local radarArea = createRadarArea(-50, -50, 50, 50, 0, 255, 0, 255);
local r,g,b,a = getRadarAreaColor(radarArea);
local sx, sy = getRadarAreaSize(radarArea);
local flashing = isRadarAreaFlashing(radarArea);
local insideA,insideB,insideC = isInsideRadarArea(radarArea, 1, 0),isInsideRadarArea(radarArea, 0, -51),isInsideRadarArea(radarArea, -25, -25)

setRadarAreaColor(radarArea, getColorFromString("pink"))
setRadarAreaFlashing(radarArea, true)
setRadarAreaSize(radarArea, -50, -50);

local insideD,insideE,insideF = isInsideRadarArea(radarArea, -75, -75),isInsideRadarArea(radarArea, -25, -25),isInsideRadarArea(radarArea, -101, -100)
print("insideD,insideE,insideF",insideD,insideE,insideF)
print("check radarArea: ",
	r == 0 and g == 255 and b == 0 and a == 255,
	sx == 50 and sy == 50,
	insideA == false and insideB == false and insideC == true,
	insideD == true and insideE == false and insideF == false,
	flashing == false
)

function commandHandler1(plr, cmd, ...)
	print("commandHandler1:",plr, cmd, ...)
end

function commandHandler2(plr, cmd, ...)
	if(cmd == "foo3" or cmd =="foo4")then
		print("This should not be printed! commandHandler2", cmd);
	end
	print("commandHandler2:",plr, cmd, ...)
end

function commandHandler3(plr, cmd, ...)
	if(cmd =="foo4")then
		print("This should not be printed! commandHandler3", cmd);
	end
	print("commandHandler3:",plr, cmd, ...)
end

function outputChatBoxTest(plr, cmd, ...)
	outputChatBox("example outputChatBox", plr)
end

addCommandHandler("foo1", commandHandler1)
addCommandHandler("foo1", commandHandler2)
addCommandHandler("foo2", commandHandler1)
addCommandHandler("foo3", commandHandler2)
addCommandHandler("foo3", commandHandler3)
addCommandHandler("foo4", commandHandler2)
addCommandHandler("foo4", commandHandler3)
removeCommandHandler("foo3", commandHandler2)
removeCommandHandler("foo4")
addCommandHandler("outputChatBoxTest", outputChatBoxTest)