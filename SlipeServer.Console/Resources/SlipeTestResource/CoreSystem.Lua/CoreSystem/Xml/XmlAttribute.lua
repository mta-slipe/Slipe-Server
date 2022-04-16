local XmlAttribute = {}

XmlAttribute.__inherits__ = { System.Xml.XmlNode }

XmlAttribute.__ctor__ = function(this, prefix, name, nameSpace, document)
	this.owner = document
	this.children = {}
	this.attributes = System.Xml.XmlAttributeCollection()
	if prefix == "" or prefix == nil then
		this.name = name
	else
		this.name = prefix .. ":" .. name
	end
	this.namespaceURI = namespace
end

System.define("System.Xml.XmlAttribute", XmlAttribute)