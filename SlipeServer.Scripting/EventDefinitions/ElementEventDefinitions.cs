using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Events;

namespace SlipeServer.Scripting.EventDefinitions;

public class ElementEventDefinitions : IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
        eventRuntime.RegisterEvent<Element>(
            "onElementDestroyed",
            (callback) =>
            {
                void callbackProxy(Element e) => callback.CallbackDelegate(e);
                return new EventHandlerActions<Element>()
                {
                    Add = (element) => element.Destroyed += callbackProxy,
                    Remove = (element) => element.Destroyed -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Element>(
            "onElementDestroy",
            (callback) =>
            {
                void callbackProxy(Element e) => callback.CallbackDelegate(e);
                return new EventHandlerActions<Element>()
                {
                    Add = (element) => element.Destroyed += callbackProxy,
                    Remove = (element) => element.Destroyed -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Element>(
            "onElementDataChange",
            (callback) =>
            {
                void callbackProxy(Element sender, ElementDataChangedArgs e)
                    => callback.CallbackDelegate(sender, e.Key, e.OldValue, e.NewValue);
                return new EventHandlerActions<Element>()
                {
                    Add = (element) => element.DataChanged += callbackProxy,
                    Remove = (element) => element.DataChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Element>(
            "onElementDimensionChange",
            (callback) =>
            {
                void callbackProxy(Element sender, ElementChangedEventArgs<ushort> e)
                    => callback.CallbackDelegate(sender, e.OldValue, e.NewValue);
                return new EventHandlerActions<Element>()
                {
                    Add = (element) => element.DimensionChanged += callbackProxy,
                    Remove = (element) => element.DimensionChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Element>(
            "onElementInteriorChange",
            (callback) =>
            {
                void callbackProxy(Element sender, ElementChangedEventArgs<byte> e)
                    => callback.CallbackDelegate(sender, e.OldValue, e.NewValue);
                return new EventHandlerActions<Element>()
                {
                    Add = (element) => element.InteriorChanged += callbackProxy,
                    Remove = (element) => element.InteriorChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Element>(
            "onElementModelChange",
            (callback) =>
            {
                void pedProxy(Ped sender, ElementChangedEventArgs<Ped, ushort> e)
                    => callback.CallbackDelegate(sender, e.OldValue, e.NewValue);
                void vehicleProxy(Vehicle sender, ElementChangedEventArgs<Vehicle, ushort> e)
                    => callback.CallbackDelegate(sender, e.OldValue, e.NewValue);
                void objectProxy(WorldObject sender, ElementChangedEventArgs<WorldObject, ushort> e)
                    => callback.CallbackDelegate(sender, e.OldValue, e.NewValue);
                return new EventHandlerActions<Element>()
                {
                    Add = (element) =>
                    {
                        if (element is Ped ped) 
                            ped.ModelChanged += pedProxy;
                        else if (element is Vehicle vehicle)
                            vehicle.ModelChanged += vehicleProxy;
                        else if (element is WorldObject worldObject) 
                            worldObject.ModelChanged += objectProxy;
                    },
                    Remove = (element) =>
                    {
                        if (element is Ped ped) 
                            ped.ModelChanged -= pedProxy;
                        else if (element is Vehicle vehicle) 
                            vehicle.ModelChanged -= vehicleProxy;
                        else if (element is WorldObject worldObject) 
                            worldObject.ModelChanged -= objectProxy;
                    }
                };
            }
        );

        eventRuntime.RegisterEvent<Element>(
            "onElementColShapeHit",
            (callback) =>
            {
                void callbackProxy(Element sender, CollisionShapeHitEventArgs e)
                    => callback.CallbackDelegate(sender, e.CollisionShape, sender.Dimension == e.CollisionShape.Dimension);
                return new EventHandlerActions<Element>()
                {
                    Add = (element) => element.ColShapeEntered += callbackProxy,
                    Remove = (element) => element.ColShapeEntered -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Element>(
            "onElementColShapeLeave",
            (callback) =>
            {
                void callbackProxy(Element sender, CollisionShapeLeftEventArgs e)
                    => callback.CallbackDelegate(sender, e.CollisionShape, sender.Dimension == e.CollisionShape.Dimension);
                return new EventHandlerActions<Element>()
                {
                    Add = (element) => element.ColShapeLeft += callbackProxy,
                    Remove = (element) => element.ColShapeLeft -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Element>(
            "onElementStartSync",
            (callback) =>
            {
                void pedProxy(Ped sender, ElementChangedEventArgs<Ped, Player?> e)
                {
                    if (sender is Player || e.NewValue == null) return;
                    callback.CallbackDelegate(sender, e.NewValue);
                }
                void vehicleProxy(Vehicle sender, ElementChangedEventArgs<Vehicle, Player?> e)
                {
                    if (e.NewValue == null) return;
                    callback.CallbackDelegate(sender, e.NewValue);
                }
                return new EventHandlerActions<Element>()
                {
                    Add = (element) =>
                    {
                        if (element is Ped ped) ped.SyncerChanged += pedProxy;
                        else if (element is Vehicle vehicle) vehicle.SyncerChanged += vehicleProxy;
                    },
                    Remove = (element) =>
                    {
                        if (element is Ped ped) ped.SyncerChanged -= pedProxy;
                        else if (element is Vehicle vehicle) vehicle.SyncerChanged -= vehicleProxy;
                    }
                };
            }
        );

        eventRuntime.RegisterEvent<Element>(
            "onElementStopSync",
            (callback) =>
            {
                void pedProxy(Ped sender, ElementChangedEventArgs<Ped, Player?> e)
                {
                    if (sender is Player || e.OldValue == null) return;
                    callback.CallbackDelegate(sender, e.OldValue);
                }
                void vehicleProxy(Vehicle sender, ElementChangedEventArgs<Vehicle, Player?> e)
                {
                    if (e.OldValue == null) return;
                    callback.CallbackDelegate(sender, e.OldValue);
                }
                return new EventHandlerActions<Element>()
                {
                    Add = (element) =>
                    {
                        if (element is Ped ped) ped.SyncerChanged += pedProxy;
                        else if (element is Vehicle vehicle) vehicle.SyncerChanged += vehicleProxy;
                    },
                    Remove = (element) =>
                    {
                        if (element is Ped ped) ped.SyncerChanged -= pedProxy;
                        else if (element is Vehicle vehicle) vehicle.SyncerChanged -= vehicleProxy;
                    }
                };
            }
        );

        eventRuntime.RegisterEvent<Element>(
            "onElementClicked",
            (callback) =>
            {
                void callbackProxy(Element sender, ElementClickedEventArgs e)
                {
                    var button = e.Button switch
                    {
                        Server.Elements.Enums.CursorButton.Left => "left",
                        Server.Elements.Enums.CursorButton.Middle => "middle",
                        Server.Elements.Enums.CursorButton.Right => "right",
                        _ => "left"
                    };
                    var state = e.IsDown ? "down" : "up";
                    callback.CallbackDelegate(sender, button, state, e.Player, e.WorldPosition.X, e.WorldPosition.Y, e.WorldPosition.Z);
                }
                return new EventHandlerActions<Element>()
                {
                    Add = (element) => element.Clicked += callbackProxy,
                    Remove = (element) => element.Clicked -= callbackProxy
                };
            }
        );
    }
}
