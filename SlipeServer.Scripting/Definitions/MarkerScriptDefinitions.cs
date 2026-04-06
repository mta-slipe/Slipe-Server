using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public class MarkerScriptDefinitions(IMtaServer server, IElementCollection elementCollection)
{
    [ScriptFunctionDefinition("createMarker")]
    public Marker CreateMarker(
        float x, float y, float z,
        string markerType = "checkpoint",
        float size = 4.0f,
        int r = 0, int g = 0, int b = 255, int a = 255,
        Player? visibleTo = null,
        bool ignoreAlphaLimits = false)
    {
        var marker = new Marker(new Vector3(x, y, z), ParseMarkerType(markerType))
        {
            Size = size,
            Color = Color.FromArgb(a, r, g, b),
            IgnoreAlphaLimits = ignoreAlphaLimits,
        };

        if (visibleTo != null)
            marker.AssociateWith(visibleTo);
        else
            marker.AssociateWith(server);

        if (ScriptExecutionContext.Current?.Owner != null)
            marker.Parent = ScriptExecutionContext.Current.Owner?.DynamicRoot;

        return marker;
    }

    [ScriptFunctionDefinition("getMarkerColor")]
    public Color GetMarkerColor(Marker marker) => marker.Color;

    [ScriptFunctionDefinition("getMarkerCount")]
    public int GetMarkerCount() => elementCollection.GetByType<Marker>().Count();

    [ScriptFunctionDefinition("getMarkerIcon")]
    public string GetMarkerIcon(Marker marker) => marker.MarkerIcon.ToString().ToLower();

    [ScriptFunctionDefinition("getMarkerSize")]
    public float GetMarkerSize(Marker marker) => marker.Size;

    [ScriptFunctionDefinition("getMarkerTarget")]
    public Vector3? GetMarkerTarget(Marker marker) => marker.TargetPosition;

    [ScriptFunctionDefinition("getMarkerTargetArrowProperties")]
    public MarkerArrowProperties GetMarkerTargetArrowProperties(Marker marker)
    {
        var color = marker.TargetArrowColor ?? Color.FromArgb(255, 255, 64, 64);
        var size = marker.TargetArrowSize > 0 ? marker.TargetArrowSize : marker.Size * 0.625f;
        return new MarkerArrowProperties(color, size);
    }

    [ScriptFunctionDefinition("getMarkerType")]
    public string GetMarkerType(Marker marker) => marker.MarkerType.ToString().ToLower();

    [ScriptFunctionDefinition("setMarkerColor")]
    public bool SetMarkerColor(Marker marker, int r, int g, int b, int a)
    {
        marker.Color = Color.FromArgb(a, r, g, b);
        return true;
    }

    [ScriptFunctionDefinition("setMarkerIcon")]
    public bool SetMarkerIcon(Marker marker, string icon)
    {
        marker.MarkerIcon = icon.ToLower() switch
        {
            "none" => MarkerIcon.None,
            "arrow" => MarkerIcon.Arrow,
            "finish" => MarkerIcon.Finish,
            _ => throw new ArgumentException($"Invalid marker icon: {icon}"),
        };
        return true;
    }

    [ScriptFunctionDefinition("setMarkerSize")]
    public bool SetMarkerSize(Marker marker, float size)
    {
        marker.Size = size;
        return true;
    }

    [ScriptFunctionDefinition("setMarkerTarget")]
    public bool SetMarkerTarget(Marker marker, float x, float y, float z)
    {
        marker.TargetPosition = new Vector3(x, y, z);
        return true;
    }

    [ScriptFunctionDefinition("setMarkerTargetArrowProperties")]
    public bool SetMarkerTargetArrowProperties(Marker marker, int r = 255, int g = 64, int b = 64, int a = 255, float size = -1f)
    {
        marker.TargetArrowColor = Color.FromArgb(a, r, g, b);
        marker.TargetArrowSize = size < 0 ? marker.Size * 0.625f : size;
        return true;
    }

    [ScriptFunctionDefinition("setMarkerType")]
    public bool SetMarkerType(Marker marker, string markerType)
    {
        marker.MarkerType = ParseMarkerType(markerType);
        return true;
    }

    private static MarkerType ParseMarkerType(string markerType) => markerType.ToLower() switch
    {
        "checkpoint" => MarkerType.Checkpoint,
        "ring" => MarkerType.Ring,
        "cylinder" => MarkerType.Cylinder,
        "arrow" => MarkerType.Arrow,
        "corona" => MarkerType.Corona,
        _ => throw new ArgumentException($"Invalid marker type: {markerType}"),
    };
}
