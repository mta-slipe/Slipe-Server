

local HttpClient = {}


HttpClient.__ctor__ = function(this, handler, disposeHandler)

end

local function contentStringRequestCallback(data, responseInfo)
	return data
end

HttpClient.GetStringAsync = function(this, url)
	local task, callback =  System.Task.Callback(contentStringRequestCallback)
	fetchRemote(url, {}, callback)
	return task;
end

local function requestCallback(data, responseInfo)
	local status = responseInfo.statusCode
	local isSuccessful = responseInfo.success
	local headers = responseInfo.headers

	local content = data

	local response = System.Net.Http.HttpResponseMessage()
	response:setContent(content)
	response:setStatusCode(status)
	response:setHeaders(headers)
	
	return response
end

HttpClient.GetAsync = function(this, url)
	local task, callback = System.Task.Callback(requestCallback)
	fetchRemote(url, {}, callback)
	return task;
end

HttpClient.DeleteAsync = function(this, url)
	local task, callback =  System.Task.Callback(requestCallback)
	fetchRemote(url, {
		method = "DELETE"
	}, callback)
	return task;
end

local function fetchWithContent(url, method, httpContent, callback)
	if (httpContent ~= nil and System.is(httpContent, System.Net.Http.FormUrlEncodedContent)) then
		local formFields = httpContent:getFormFields()
		fetchRemote(url, {
			method = method,
			formFields = formFields	
		}, callback)
	else
		local postData = httpContent:ToString()
		fetchRemote(url, {
			method = method,
			postData = postData	
		}, callback)
	end
end

HttpClient.PostAsync = function(this, url, httpContent)
	local task, callback =  System.Task.Callback(requestCallback)
	fetchWithContent(url, "POST", httpContent, callback)
	return task;
end

HttpClient.PutAsync = function(this, url, httpContent)
	local task, callback =  System.Task.Callback(requestCallback)
	fetchWithContent(url, "PUT", httpContent, callback)
	return task;
end

HttpClient.PatchAsync = function(this, url, httpContent)
	local task, callback =  System.Task.Callback(requestCallback)
	fetchWithContent(url, "PATCH", httpContent, callback)
	return task;
end

System.define("System.Net.Http.HttpClient", HttpClient)