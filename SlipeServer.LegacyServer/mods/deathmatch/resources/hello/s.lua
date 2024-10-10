print("print hello world");
outputDebugString("outputDebugString hello world");

local eventAdded = addEventHandler("onResourceStart", root, function(startedResource)
	print("lua started", getResourceName(startedResource))
end)

print("eventAdded", eventAdded)