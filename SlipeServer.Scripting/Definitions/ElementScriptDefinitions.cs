using SlipeServer.Server.Elements;
using System.Numerics;

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


    [ScriptFunctionDefinition("getElementModel")]
    public ushort GetElementModel(Element element) => element switch
    {
        WorldObject worldObject => worldObject.Model,
        Vehicle vehicle => vehicle.Model,
        Ped ped => ped.Model,
        _ => throw new System.Exception("Unsupported element")
    };

    [ScriptFunctionDefinition("setElementModel")]
    public bool SetElementModel(Element element, ushort model)
    {
        switch (element)
        {
            case WorldObject worldObject:
                worldObject.Model = model;
                return true;
            case Vehicle vehicle:
                vehicle.Model = model;
                return true;
            case Ped ped:
                ped.Model = model;
                return true;
            default:
                return false;
        }
    }
}

