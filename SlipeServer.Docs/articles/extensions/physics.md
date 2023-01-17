# Server Side Physics

One of Slipe Server's extension packages is [SlipeServer.Physics](https://www.nuget.org/packages/SlipeServer.Physics/). This package allows you to do server sided physics simulation, and raycasting.  

In order for this to work with the GTA world you will require to have a copy of the GTA install directory accessible to your server.

In order to get started you use the [`PhysicsService`](/api/optionals/physics/SlipeServer.Physics.Services.PhysicsService.html) to create physics worlds. A physics world is an isolated physics simulation. You can create empty physics worlds, and manually add models, or you can create physics worlds based on the GTA SA directory, to create a world based on GTA's internal files (.dat, .img, .ipl, .ide, etc)

When using the `CreatePhysicsWorldFromGtaDirectory` method you can specify the [`PhysicsModelLoadMode`](/api/optionals/physics/SlipeServer.Physics.Enum.PhysicsModelLoadMode.html), this determines whether the physics simulation uses .col files, or .dff files for collisions.  
.dff files might be more accurate, but .col files would be more efficient.

Once you have your physics world, you can use this for either physics simulation, or just raycasting.

## Raycasting
Raycasting is done using the `RayCast` or `MultiRayCast` method. Both of these methods take a position, a direction, and a length for the raycast.  
The only difference between the two is that `RayCast` will return the nearest hit, whereas `MultiRayCast` will return all hits across the ray.  

Raycasting is done right away, and does not require the simulation to be running.

## Simulation
Besides raycasting, you can also do simulations. Simulations will compute physics for any dynamic bodies that were added to the simulation.  

A dynamic body is a physics object that can be effected by forces (like gravity, or bumping into other bodies).  
Kinematic bodies can also be added, these are objects that do not have physics applied to them, but other bodies can collide with them. The default GTA world (if you created the physics world based on it) it comprised of just kinematic bodies.
Static bodies 

You can create these bodies and add them to the simulation using the `AddDynamicBody`, `AddKinematicBody` and `AddStatic`.  

These bodies will require a mesh to work, which can be created using `CreateMesh`, `CreateCylinder` or `CreateSphere`. These resulting meshes can then be used to add the bodies.

Here's an example:
```cs
this.physicsWorld = this.physicsService.CreatePhysicsWorldFromGtaDirectory(gtaDirectory ?? "gtasa", "gta.dat", PhysicsModelLoadMode.Dff, builderAction: (builder) =>
{
    builder.SetGravity(Vector3.UnitZ * -1.0f);
});

this.ball = this.physicsWorld.CreateSphere(0.25f);

var physicsBall = this.physicsWorld.AddDynamicBody(this.ball, new Vector3(0, 0, 4), Quaternion.Identity, 1);
var ball = new WorldObject(2114, e.Player.Position + Vector3.UnitZ * 2).AssociateWith(this.server);
physicsBall.CoupleWith(ball);

this.physicsWorld.Start(5);
```

This example will create a basketball world object (in the MTA world), and a sphere collidable body.  
These two are then "coupled". Coupling means the element will move when the physics simulation body moves. This effectively results in a ball that will roll around in the GTA world, based on server side physics simulation.  

## What to do with this
Server side physics can be a very powerful tool. It can be used to do things like calculating server side NPC behaviour, based on sight lines, or used to have physics simulations without having to rely on a single client.

