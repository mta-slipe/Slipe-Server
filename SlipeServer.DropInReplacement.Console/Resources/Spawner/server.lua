

function handlePlayerJoin()
	spawnPlayer(source, 0, 0, 3, 0, 7)
	setCameraTarget(source, source)
	fadeCamera(source, true)
end
addEventHandler("onPlayerJoin", root, handlePlayerJoin)


function handlePlayerWasted()
	setTimer(function()
	
		spawnPlayer(source, 0, 0, 3, 0, 7)

	end, 1000, 1)
end
addEventHandler("onPlayerWasted", root, handlePlayerWasted)

for _, player in pairs(getElementsByType("player")) do
	spawnPlayer(player, 0, 0, 3, 0, 7)
	setCameraTarget(player, player)
	fadeCamera(player, true)
end

