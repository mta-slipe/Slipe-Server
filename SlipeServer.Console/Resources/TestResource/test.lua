outputChatBox("This is a running Lua file")

triggerServerEvent("Slipe.Test.Event", root, "String value", true, 123, {
	x = 5.5,
	y = "string",
	z = {},
	w = false,
	me = localPlayer
})

addEvent("Slipe.Test.ClientEvent", true)
addEventHandler("Slipe.Test.ClientEvent", root, function(...) 
	for k, v in pairs({...}) do
		iprint(k, v)
	end

	triggerServerEvent("Slipe.Test.SampleEvent", localPlayer, {
		Float = 0.5,
		Double = 2.5,
		Integer = 1,
		OptionalInteger = 0,
		Text = "Bob",
		Boolean = true,
		Position = { X = 1.5, Y = 2.5, Z = 3.5 },
		SubValue = {
			Header = "Header text",
			Messages = {
				"Message 1",
				"Message 2"
			}
		}
	})
end)

outputChatBox("Event ready");

addCommandHandler("crun", function(command, ...)
	outputChatBox("Running code")
	local code = table.concat({ ... }, " ")
	local result = loadstring(code)()
	outputChatBox("Result: " .. tostring(result))
end)

addCommandHandler("exporttest", function(command, ...)
	exports.MetaXmlTestResource:exportMeBaby()
end)

setDevelopmentMode(true)

addCommandHandler("TriggerTests", function(command, ...)
    triggerServerEvent("Test1", resourceRoot, "Test1")
    triggerServerEvent("Test2", resourceRoot, "Test2")
end)

setFPSLimit(60)


addCommandHandler("triggerLatent", function(command, ...)
	outputChatBox("Sending latent event...")
	triggerLatentServerEvent("exampleLatentTrigger",5000,false,root,string.rep("a", 10000))
end)

addCommandHandler("myvehprintdamageclient", function()
    local vehicle = getPedOccupiedVehicle(localPlayer)
    if not vehicle then
        outputChatBox("You are not in a vehicle.")
        return
    end

    outputChatBox("List of damaged vehicle parts:")
    
    outputChatBox("Doors:")
    for i = 0, 5 do
        local state = getVehicleDoorState(vehicle, i)
        if state ~= 0 then
            outputChatBox(" " .. i .. " - " .. state)
        end
    end

    outputChatBox("Panels:")
    for i = 0, 6 do
        local state = getVehiclePanelState(vehicle, i)
        if state ~= 0 then
            outputChatBox(" " .. i .. " - " .. state)
        end
    end

    outputChatBox("Wheels:")
    local states = {getVehicleWheelStates(vehicle, i)}
    for i = 1, 4 do
        if states[i] ~= 0 then
            outputChatBox(" " .. i .. " - " .. states[i])
        end
    end
end)

local screenX, screenY = guiGetScreenSize()
local color = tocolor(255,255,255)

local function printDebugVehicle(k, ped)
	local veh = getPedOccupiedVehicle(ped)
	local name = (getElementType(ped) == "ped") and "ped" or getPlayerName(ped);

	dxDrawText(string.format("%s - %s/%s", name, veh and tostring(veh) or "<none>", getElementData(ped, "currentVehicle") and tostring(getElementData(ped, "currentVehicle")) or "<none>"), screenX - 600, 400 + k * 25, screenX - 600, 400 + k * 25, color, 1.5, "sans")
end

addEventHandler("onClientRender", root, function()
	local k = 0
	for i,v in ipairs(getElementsByType("player"))do
		k = k + 1
		printDebugVehicle(k, v);
	end
	for i,v in ipairs(getElementsByType("ped"))do
		k = k + 1
		printDebugVehicle(k, v);
	end
end)

local clientTasks = {}
addEvent("testClientTask", true)
addEventHandler("testClientTask", root, function(clientTask)
	outputChatBox("ClientTask created.");
	clientTasks[#clientTasks + 1] = clientTask;
end)

addCommandHandler("resolveTasks", function()
	for i,task in ipairs(clientTasks)do
		ClientTask.Resolve(task, "Ok");
	end
	outputChatBox("Resolved: "..#clientTasks.." tasks.")
	clientTasks = {}
end)

addCommandHandler("rejectTasks", function()
	for i,task in ipairs(clientTasks)do
		ClientTask.Reject(task, "Ok");
	end
	outputChatBox("Failed: "..#clientTasks.." tasks.")
	clientTasks = {}
end)
