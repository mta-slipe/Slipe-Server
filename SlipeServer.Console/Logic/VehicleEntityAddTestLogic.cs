using SlipeServer.Server;
using SlipeServer.Server.Elements;
using System.Numerics;

namespace SlipeServer.Console.Logic;

public class VehicleEntityAddTestLogic
{
    public VehicleEntityAddTestLogic(
        IMtaServer<CustomPlayer> server
    )
    {
        int index = 0;
        foreach (var model in Enum.GetValues<VehicleModel>())
        {
            var vehicle = new Vehicle(model, new Vector3(1000 + (index++) * 5, 0, 0)).AssociateWith(server);
        }
    }
}
