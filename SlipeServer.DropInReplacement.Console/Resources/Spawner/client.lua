outputChatBox("Client script has started")


addCommandHandler("crun", function(command, ...)
	outputChatBox("Running code")
	local code = table.concat({ ... }, " ")
	local result = loadstring(code)()
	outputChatBox("Result: " .. tostring(result))
end)

