local XmlDocument = {}

XmlDocument.__inherits__ = { System.Xml.XmlNode }

XmlDocument.__ctor__ = function(this)
	this.children = {}
end


XmlDocument.CreateElement = function(this, name)
	return System.Xml.XmlElement("", name, "", this)
end

XmlDocument.AppendChild = function(this, child)
	if this.children[1] ~= nil then
		System.Throw(System.Exception("An XmlDocument can not have mulitple root elements."))
	end
	this.children[1] = child
end

XmlDocument.RemoveChild = function(this, child)
	if this.children[1] == child then
		this.children[1] = nil
	end
end

XmlDocument.Save = function (this, filepath)
	local rootNode = xmlCreateFile(filepath, this.children[1].name)
	
	this.children[1]:save(rootNode)	

	xmlSaveFile(rootNode)
	xmlUnloadFile(rootNode)
end

XmlDocument.Load = function(this, filepath)
	local rootNode = xmlLoadFile(filepath, true)
	this.children[1] = this:CreateElement(xmlNodeGetName(rootNode))
	this.children[1]:index(rootNode)	

	xmlUnloadFile(rootNode)
end

System.define("System.Xml.XmlDocument",  XmlDocument)