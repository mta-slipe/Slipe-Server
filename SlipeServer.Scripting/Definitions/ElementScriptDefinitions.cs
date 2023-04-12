using SlipeServer.Server.Elements;
using System.Numerics;
using MoonSharp.Interpreter;
using SlipeServer.Server.ElementCollections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;

namespace SlipeServer.Scripting.Definitions;

public class ElementScriptDefinitions
{
    [ScriptFunctionDefinition("destroyElement")]
    public void DestroyElement(Element element) => element.Destroy();


    [ScriptFunctionDefinition("getElementPosition")]
    public Vector3 GetElementPosition(Element element) => element.Position;

    [ScriptFunctionDefinition("setElementPosition")]
    public void SetElementPosition(Element element, Vector3 position) => element.Position = position;


    [ScriptFunctionDefinition("getElementRotation")]
    public Vector3 GetElementRotation(Element element) => element.Rotation;

    [ScriptFunctionDefinition("setElementRotation")]
    public void SetElementRotation(Element element, Vector3 rotation) => element.Rotation = rotation;


    [ScriptFunctionDefinition("getElementType")]
    public string GetElementType(Element element) => element.ElementType.ToString().ToLower();

    [ScriptFunctionDefinition("createElement")]
    public Element CreateElement(BasicCompoundElementCollection elementCollection, string type, int? elementID = null)
    {
        IEnumerable<Element> allElements= elementCollection.GetAll();
        IEnumerable<Element>? elementType = from element in allElements where element.Name == type select element;

        if (elementType != null)
        {
            var actualElementType = elementType.FirstOrDefault();
        }

        return new object() as Element;
    }
}
