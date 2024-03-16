

local FormUrlEncodedContent = {}

FormUrlEncodedContent.__ctor__ = function(this, kvPairList)
	this.kvPairs = kvPairList 

end

FormUrlEncodedContent.ToString = function(this)
	local returnValue = ""
	
	for _, kvPair in System.each(this.kvPairs) do
		returnValue = returnValue .. kvPair.Key .. "=" .. kvPair.Value .. "&"
	end

	return returnValue
end

FormUrlEncodedContent.getFormFields = function(this)
	local returnValue = {}
	
	for _, kvPair in System.each(this.kvPairs) do
		returnValue[kvPair.Key] = kvPair.Value
	end

	return returnValue
end

System.define("System.Net.Http.FormUrlEncodedContent", FormUrlEncodedContent)