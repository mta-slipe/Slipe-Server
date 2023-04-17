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
using Microsoft.Extensions.Primitives;
using SlipeServer.Server.Elements.Enums;
using System.ComponentModel;

namespace SlipeServer.Scripting.Definitions;

public class ElementScriptDefinitions
{
    private readonly MtaServer server;
    private readonly IDictionary<ElementType, object> elementsForVariants;
    private readonly IElementCollection elementCollection;
    private readonly Script ownerScript;

    public ElementScriptDefinitions(MtaServer _server, IElementCollection elementCollection, Script ownerScript)
    {
        this.server = _server;
        this.elementCollection = (RTreeCompoundElementCollection)elementCollection;
        this.ownerScript = ownerScript;
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
        //BasicCompoundElementCollection elementCollection = (BasicCompoundElementCollection)this.server.GetRequiredService<IElementCollection>();

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

        elementCollection.Add(newElement);

        return newElement;
    }

    [ScriptFunctionDefinition("detachElements")]
    public void DeatchElements(Element childElement, Element? detachFrom = null)
    {
        childElement.DetachFrom(detachFrom ?? null);
    }

    [ScriptFunctionDefinition("getAllElementData")]
    public Table GetAllElementData(Element element)
    {
        var elementDatas = element.GetAllElementData();
        DynValue theTable = DynValue.NewTable(ownerScript);
        
        foreach ( var elementData in elementDatas )
        {
            theTable.Table.Set(elementData.Key, DynValue.FromObject(ownerScript, elementData.Value));
        }


        return theTable.Table;
    }

    [ScriptFunctionDefinition("setElementData")]
    public void SetElementData(Element element, string key, DynValue dynValue, bool synchronizeType = true)
    {
        Dictionary<bool, DataSyncType> syncTypes = new()
        {
            [true] = DataSyncType.Broadcast,
            [false] = DataSyncType.Local,
        };
        element.SetData(key, dynValue.String, syncTypes[synchronizeType]);
    }
}
