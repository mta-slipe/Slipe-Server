local EndPoint = {}

EndPoint.__ctor__ = function (this, ip, port)
	this.ip = ip
	this.port = port
end

System.define("System.Net.EndPoint", EndPoint)