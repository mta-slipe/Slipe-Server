

local HttpContent = {}

HttpContent.ReadAsStringAsync = function(this)
	return Task.Instant(this.stringValue)
end

HttpContent.ToString = function(this)
	return this.stringValue
end

System.define("System.Net.Http.HttpContent", HttpContent)