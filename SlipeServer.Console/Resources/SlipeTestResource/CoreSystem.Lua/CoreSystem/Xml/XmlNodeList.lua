local XmlNodeList = {}


XmlNodeList.__ctor__ = function(this)
	this.values = {}
end

XmlNodeList.get = function(this, key)
	return this.values[key]
end  

XmlNodeList.getCount = function(this)
	return #this.values
end

XmlNodeList.Item = function(this, index)
	return this.values[key]
end

XmlNodeList.GetEnumerator = function(this)
	return System.Array.GetEnumerator(this.values, System.Xml.XmlNode)
end

System.define("System.Xml.XmlNodeList", XmlNodeList)