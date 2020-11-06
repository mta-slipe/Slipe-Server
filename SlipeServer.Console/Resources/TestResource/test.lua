outputChatBox("This is a running Lua file")

triggerServerEvent("Slipe.Test.Event", root, "String value", true, 123, {
	x = 5.5,
	y = "string",
	z = {},
	w = false
})

addEvent("Slipe.Test.ClientEvent", true)
addEventHandler("Slipe.Test.ClientEvent", root, function(...) 
	for k, v in pairs({...}) do
		iprint(k, v)
	end
end)

outputChatBox("Event ready");