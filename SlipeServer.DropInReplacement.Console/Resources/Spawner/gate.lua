
local object = createObject(5856, 10, 0, 4)
local colshape = createColSphere(10, 0, 3, 5)

addEventHandler("onColShapeHit", colshape, function(hitElement)
    if getElementType(hitElement) == "player" then
        moveObject(object, 1000, 10, 0, 8)
    end
end)

addEventHandler("onColShapeLeave", colshape, function(leftElement)
    if getElementType(leftElement) == "player" then
        moveObject(object, 1000, 10, 0, 4)
    end
end)

outputChatBox("Gates loaded")
