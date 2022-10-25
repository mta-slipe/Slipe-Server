

local StringContent = {}

StringContent.__ctor__ = function(this, value)
	this.stringvalue = value
end

System.define("System.Net.Http.StringContent", StringContent)