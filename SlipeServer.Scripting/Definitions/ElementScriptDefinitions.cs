using SlipeServer.Server;
using SlipeServer.Server.Elements;
using System;
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
}
