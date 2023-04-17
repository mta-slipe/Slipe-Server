using SlipeServer.Server.Elements;
using System.Numerics;
using MoonSharp.Interpreter;
using SlipeServer.Server.ElementCollections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using SlipeServer.Server;
using System.Text.RegularExpressions;
using System;
using System.Collections;
using SlipeServer.Server.Elements.ColShapes;
using System.Runtime.InteropServices;

namespace SlipeServer.Scripting.Definitions;

public class ElementScriptDefinitions
{
    private readonly MtaServer server;
    private readonly IDictionary<ElementType, object> elementsForVariants;

    public ElementScriptDefinitions(MtaServer _server)
    {
        this.server = _server;
        this.elementsForVariants = new Dictionary<ElementType, object>(Enum.GetNames(typeof(ElementType)).Length)
        {
            [ElementType.Player] = typeof(Player),
            [ElementType.Weapon] = typeof(WeaponObject),
            [ElementType.Pickup] = typeof(Pickup),
            [ElementType.Blip] = typeof(Blip),
            [ElementType.Colshape] = typeof(CollisionShape),
            [ElementType.Console] = typeof(Console),
            [ElementType.Dummy] = typeof(DummyElement),
            [ElementType.Marker] = typeof(Marker),
            [ElementType.Object] = typeof(Object),
            [ElementType.Ped] = typeof(Ped),
            [ElementType.RadarArea] = typeof(RadarArea),
            [ElementType.Team] = typeof(Team),
            [ElementType.Vehicle] = typeof(Vehicle),
            [ElementType.Water] = typeof(Water),
            // TBD: [ElementType.WorldMeshUnused] = ?
            // TBD: [ElementType.PathNodeUnused] = 
            // TBD: [ElementType.DatabaseConnection] = ?
        };
    }

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
    public Element CreateElement(string type, int? elementID = null)
    {
        BasicCompoundElementCollection elementCollection = (BasicCompoundElementCollection)this.server.GetRequiredService<IElementCollection>();

        IEnumerable<Element> allElements= elementCollection.GetAll();

        Element newElement;

        if (Enum.IsDefined(typeof(ElementType), type))
        {
            Type classType = (Type)this.elementsForVariants[Enum.Parse<ElementType>(type)];
            newElement = (Element)Activator.CreateInstance(classType);
            // TODO: Maybe there's a better alternative to handle this
            if (newElement is null)
                newElement = new CustomElement(type);
        } else
        {
            newElement = new CustomElement(type);
        }

        return newElement;
    }
}
