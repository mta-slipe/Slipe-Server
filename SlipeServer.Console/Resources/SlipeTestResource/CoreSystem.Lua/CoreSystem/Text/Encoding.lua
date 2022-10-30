local Encoding = {}

Encoding.__ctor__ = function (this, type)
	this.type = type
end

Encoding.getASCII = function(this)
	if Encoding.ascii == nil then
		Encoding.ascii = Encoding("ascii")
	end
	return Encoding.ascii
end

Encoding.GetString = function(this, bytes, start, count)
	local value = ""
	start = start or 0
	count = count or (bytes:getLength() - start) - 1
	for i = start, start + count do
		value = value .. string.char(bytes:get(i))
	end
	return value
end

Encoding.GetBytes = function(this, value)
	local list = System.Array(System.Byte):new(value:len())
	for i = 1, value:len() do
		list:set(i - 1, value:byte(i))
	end
	return list
end

System.define("System.Text.Encoding", Encoding)