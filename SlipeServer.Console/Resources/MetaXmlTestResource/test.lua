outputChatBox("Meta.xml resource is loaded")

local file = fileOpen("file.txt")
local content = fileRead(file, fileGetSize(file))
fileClose(file)
outputChatBox(content)

function exportMeBaby()
	outputChatBox("I AM EXPORTED!")
end