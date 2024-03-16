local XmlElement = {}

XmlElement.__inherits__ = { System.Xml.XmlNode }

XmlElement.__ctor__ = function(this, prefix, name, nameSpace, document)
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

XmlElement.SetAttribute = function (this, name, value)
	local attribute = System.Xml.XmlAttribute("", name, "", this.owner)
	attribute.value = value
	this.attributes:set(name, attribute)
end

XmlElement.GetAttribute = function (this, name, namespace)
	return this.attributes:get(name, value).value
end


XmlElement.index = function (this, node)
	local attributes = xmlNodeGetAttributes(node)
	for key, value in pairs(attributes) do
		this:SetAttribute(key, value)
	end
	
	local children = xmlNodeGetChildren(node)
	for _, childNode in ipairs(children) do
		local child = this.owner:CreateElement(xmlNodeGetName(childNode))
		child:index(childNode)
		this:AppendChild(child)
	end

	this.value = xmlNodeGetValue(node)
end

XmlElement.save = function (this, node)
	xmlNodeSetValue(node, this.value)
	xmlNodeSetName(node, this.name)

	for key, value in pairs(this.attributes.values) do
		xmlNodeSetAttribute(node, key, value)
	end

	for _, child in ipairs(this.children) do
		local childNode = xmlCreateChild(node, child.name)
		child:save(childNode)
	end
end


System.define("System.Xml.XmlElement", XmlElement)