using System.Xml.Linq;

namespace SlipeServer.Scripting;

public class ScriptXmlNode(XElement element, string? filePath = null, bool readOnly = false)
{
    public XElement Element { get; } = element;
    public string? FilePath { get; } = filePath;
    public bool ReadOnly { get; } = readOnly;
}
