local Socket = {}

Socket.__ctor__ = function (this, socketType, protocol)
	if socketType ~= 1 or protocol ~= 6 then
		System.throw(System.InvalidOperationException("Only SocketType.Stream and ProtocolType.Tcp are supported"))
	end

	if (sockOpen == nil) then
		System.throw(System.IO.IOException("The sockets module is required to be installed in order to use System.Net.Sockets.Socket"))
	end

	this.onOpenBind = function(...) this:onOpen(...) end
	this.onClosedBind = function(...) this:onClose(...) end
	this.onDataBind = function(...) this:onData(...) end
end

Socket.onOpen = function (this, socket)
	if socket ~= this.socket then
		return
	end
	this.connected = true
	removeEventHandler("onSockOpened", getRootElement(), this.onOpenBind)
	addEventHandler("onSockClosed", getRootElement(), this.onClosedBind)
	addEventHandler("onSockData", getRootElement(), this.onDataBind)
end

Socket.onClose = function (this, socket)
	this.connected = false
	removeEventHandler("onSockClosed", getRootElement(), this.onClosedBind)
	removeEventHandler("onSockData", getRootElement(), this.onDataBind)
end

Socket.onData = function (this, socket, data)
	if this.beganReceiving then
		local bytes = System.Text.Encoding:getASCII():GetBytes(data)
		for i = 0, bytes:getLength() - 1 do
			this.receiveBuffer:set(i, bytes:get(i))
		end
		this.bytesRead = data:len()
		this.receiveCallback({
			getAsyncState = function()
				return this.state
			end
		})
	elseif this.receiveCallback ~= nil then
		local bytes = System.Text.Encoding:getASCII():GetBytes(data)
		for i = 0, bytes:getLength() - 1 do
			this.receiveBuffer:set(i, bytes:get(i))
		end
		this.receiveCallback(data:len())
	end
end

Socket.Connect = function (this, target, port)
	if System.is(target, System.Net.IPAddress) then
		this.ip = target.ip
	elseif type(target) == "string" then
		this.ip = target
	else
		this.ip = target.ip.ip
	end
	this.port = port

	addEventHandler("onSockOpened", getRootElement(), this.onOpenBind)
	this.socket = sockOpen(this.ip, this.port)
end

Socket.ConnectAsync = function (this, target, port)
	if System.is(target, System.Net.IPAddress) then
		this.ip = target.ip
	elseif type(target) == "string" then
		this.ip = target
	else
		this.ip = target.ip.ip
	end
	this.port = port

	local task, callback = System.Task.Callback(function(...) this:onOpen(...) end)
	this.onOpenBind = callback

	this:Connect(this.ip, port)
	return task
end

Socket.Close = function (this)
	if this.socket == nil then
		return
	end
	sockClose(this.socket)
	this.connected = false
end

Socket.Send = function (this, buffer, offset, size, flags)
	if type(size) == "number" then

	elseif type(offset) == "number" then
		size = offset
		offset = 0
	else
		size = bufferSize
		offset = 0
	end
	local message = System.Text.Encoding:getASCII():GetString(buffer, offset, size)
	sockWrite(this.socket, message)
end

Socket.BeginReceive = function (this, buffer, offset, size, flags, callback, state)
	this.receiveCallback = callback
	this.receiveBuffer = buffer
	this.offset = offset
	this.size = size
	this.state = state
	this.beganReceiving = true
end

Socket.EndReceive = function (this, asyncResult)
	this.receiveBuffer = nil
	this.receiveCallback = nil
	this.offset = nil
	this.size = nil
	this.state = nil
	this.beganReceiving = false
	return this.bytesRead
end



Socket.getConnected = function (this)
	return this.connected or false
end

System.define("System.Net.Sockets.Socket", Socket)



local SocketTaskExtensions = {}

SocketTaskExtensions.ConnectAsync = function (this, target, port)
	if System.is(target, System.Net.IPAddress) then
		this.ip = target.ip
	elseif type(target) == "string" then
		this.ip = target
	else
		this.ip = target.ip.ip
	end
	this.port = port

	local task, callback = System.Task.Callback(function(...) this:onOpen(...) end)
	this.onOpenBind = callback
	this:Connect(this.ip, port)
	return task
end

SocketTaskExtensions.ReceiveAsync = function (this, buffer, offset, size)
	local task, callback =  System.Task.Callback(function() return this.receiveBuffer:getLength() end)

	this.receiveCallback = callback
	this.receiveBuffer = buffer

	return task;
end

System.define("System.Net.Sockets.SocketTaskExtensions", SocketTaskExtensions)