print("print hello world");
outputDebugString("outputDebugString hello world");

local eventAdded = addEventHandler("onResourceStart", root, function(startedResource, b)
	print("lua started", startedResource, b)
end)

print("eventAdded", eventAdded)