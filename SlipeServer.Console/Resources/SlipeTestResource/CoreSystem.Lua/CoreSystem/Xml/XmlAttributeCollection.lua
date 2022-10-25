local XmlAttributeCollection = {}


XmlAttributeCollection.__ctor__ = function(this)
	this.values = {}
end

XmlAttributeCollection.get = function(this, key)
	return this.values[key]
end  

XmlAttributeCollection.set = function(this, key, value)
	this.values[key] = value
end  

XmlAttributeCollection.GetEnumerator = function(this)
	local valueTable = {}
	for key, value in pairs(this.values) do
		valueTable[#valueTable + 1] = value
	end
	return System.Array.GetEnumerator(valueTable, System.Xml.XmlAttribute)
end

System.define("System.Xml.XmlAttributeCollection", XmlAttributeCollection)