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
		Position = { X = 1.5, Y = 2.5, Z = 3.5 }
	})
end)

outputChatBox("Event ready");

addCommandHandler("crun", function(command, ...)
	outputChatBox("Running code")
	local code = table.concat({ ... }, " ")
	loadstring(code)()
end)

setDevelopmentMode(true)

addCommandHandler("TriggerTests", function(command, ...)
    triggerServerEvent("Test1", resourceRoot, "Test1")
    triggerServerEvent("Test2", resourceRoot, "Test2")
end)

setFPSLimit(60)