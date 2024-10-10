print("print hello world");
outputDebugString("outputDebugString hello world");

local eventAdded = addEventHandler("onResourceStart", root, function(startedResource)
	print("lua started", getResourceName(startedResource))
	print("eventName inside", eventName)
end)

print("eventName outside", eventName)
print("eventAdded", eventAdded)