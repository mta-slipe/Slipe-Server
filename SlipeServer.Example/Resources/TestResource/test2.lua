outputChatBox("TEST TWO!")

addCommandHandler("blurlevel", function()
	triggerServerEvent("SlipeServer.Test.BlurLevel", root, getBlurLevel())
end)

addCommandHandler("ui", function()
	triggerServerEvent("SlipeServer.Test.Ui", root, {
		IsChatBoxInputActive = isChatBoxInputActive(),
		IsConsoleActive = isConsoleActive(),
		IsDebugViewActive = isDebugViewActive(),
		IsMainMenuActive = isMainMenuActive(),
		IsMTAWindowActive = isMTAWindowActive(),
		IsTransferBoxActive = isTransferBoxActive(),
	})
end)

addCommandHandler("servertime", function()
	triggerServerEvent("SlipeServer.Test.GetServerTime", root)
end)
addEvent("SlipeServer.Test.GetServerTime.Success", true)
addEventHandler("SlipeServer.Test.GetServerTime.Success", localPlayer, outputChatBox)

addCommandHandler("err", function()
	triggerServerEvent("SlipeServer.Test.ThrowError", root)
end)
addEvent("SlipeServer.Test.ThrowError.Error", true)
addEventHandler("SlipeServer.Test.ThrowError.Error", localPlayer, function() outputChatBox("An error occurred") end)

addCommandHandler("cursor", function()
	local x, y = getCursorPosition()
	triggerServerEvent("SlipeServer.Test.OutputCursorPosition", root, {
		X = x,
		Y = y
	})
end)

local weapons = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 22, 23, 24, 25, 26, 27, 28, 29, 32, 30, 31, 33, 34, 35, 36, 37, 38, 16, 17, 18, 39, 41, 42, 43, 10, 11, 12, 14, 15, 44, 45, 46, 40}
addCommandHandler("enumtest", function()
	local weapon = weapons[math.random(1, #weapons)]
	local bodypart = math.random(3, 9)
	triggerServerEvent("SlipeServer.Test.EnumTest", root, weapon, bodypart)
end)

addCommandHandler("generic", function()
	triggerServerEvent("SlipeServer.Test.GenericTest", root, {
		Value = "Generic test message"	
	})
end)
