

local HttpResponseMessage = {}

HttpResponseMessage.getContent = function(this)
	return this.content
end

HttpResponseMessage.setContent = function(this, content)
	this.content = content
end

HttpResponseMessage.getHeaders = function(this)
	return this.headers
end

HttpResponseMessage.setHeaders = function(this, headers)
	this.headers = headers
end

HttpResponseMessage.getStatusCode = function(this)
	return this.statusCode
end

HttpResponseMessage.setStatusCode = function(this, statusCode)
	this.statusCode = statusCode
end

HttpResponseMessage.getReasonPhrase = function(this)
	return this.reasonPhrase
end

HttpResponseMessage.setReasonPhrase = function(this, reasonPhrase)
	this.reasonPhrase = reasonPhrase
end

HttpResponseMessage.getVersion = function(this)
	return this.version
end

HttpResponseMessage.setVersion = function(this, version)
	this.version = version
end

HttpResponseMessage.getRequestMessage = function(this)
	return this.requestMessage
end

HttpResponseMessage.setRequestMessage = function(this, requestMessage)
	this.requestMessage = requestMessage
end

HttpResponseMessage.getIsSuccessStatusCode = function(this)
	return (this.statusCode >= 200 and this.statusCode <= 300)
end

System.define("System.Net.Http.HttpResponseMessage", HttpResponseMessage)