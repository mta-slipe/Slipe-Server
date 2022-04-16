local IPAddress = {}

IPAddress.__ctor__ = function (this, address)
	if type(address) == "table" then

	else
		this.ip = address
	end
end

IPAddress.Parse = function (ipString)
	return IPAddress(ipString)
end

IPAddress.TryParse = function (ipString)
	return true, IPAddress(ipString)
end

IPAddress.ToString = function (this)
	return this.ip
end

System.define("System.Net.IPAddress", IPAddress)